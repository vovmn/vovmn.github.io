[CmdletBinding()]
param(
    [string]$Server = '185.239.51.17',
    [ValidateRange(1, 65535)]
    [int]$SshPort = 2223,
    [string]$SshUser = 'user',
    [string]$RemotePath = '/opt/imulab',
    [string]$Version = (Get-Date -Format 'yyyyMMdd-HHmmss'),
    [string]$IdentityFile = '',
    [switch]$SkipTests,
    [switch]$PrepareOnly,
    [switch]$KeepArtifacts
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Invoke-Native {
    param(
        [Parameter(Mandatory)] [string]$FilePath,
        [Parameter(ValueFromRemainingArguments)] [string[]]$Arguments
    )

    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Command failed with exit code ${LASTEXITCODE}: $FilePath $($Arguments -join ' ')"
    }
}

function Restore-EnvironmentValue {
    param([string]$Name, [AllowNull()][string]$Value)
    if ($null -eq $Value) {
        Remove-Item "Env:$Name" -ErrorAction SilentlyContinue
    }
    else {
        Set-Item "Env:$Name" $Value
    }
}

if ($Version -notmatch '^[A-Za-z0-9][A-Za-z0-9._-]{0,63}$') {
    throw "Invalid version: $Version"
}
if ($Server -notmatch '^[A-Za-z0-9.-]+$') {
    throw "Invalid server name or address: $Server"
}
if ($SshUser -notmatch '^[A-Za-z_][A-Za-z0-9_-]*$') {
    throw "Invalid SSH user: $SshUser"
}
if ($RemotePath -notmatch '^/opt/[A-Za-z0-9._/-]+$' -or $RemotePath.Contains('..')) {
    throw 'RemotePath must be an explicit path inside /opt.'
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$remoteTarget = "${SshUser}@${Server}"
$sshPortText = [string]$SshPort
$artifactRoot = Join-Path ([IO.Path]::GetTempPath()) "imulab-release-$Version-$([Guid]::NewGuid().ToString('N'))"
$archiveName = "imulab-$Version.tar.gz"
$imageBundleName = "imulab-$Version-images.tar"
$installerName = "imulab-deploy-$Version.sh"
$archivePath = Join-Path $artifactRoot $archiveName
$imageBundlePath = Join-Path $artifactRoot $imageBundleName
$installerPath = Join-Path $artifactRoot $installerName
$remoteArchive = "/tmp/$archiveName"
$remoteImageBundle = "/tmp/$imageBundleName"
$remoteInstaller = "/tmp/$installerName"

foreach ($command in @('docker', 'node', 'npm', 'scp', 'ssh', 'tar')) {
    if (-not (Get-Command $command -ErrorAction SilentlyContinue)) {
        throw "Required command is not available: $command"
    }
}

$sshOptions = @('-p', $sshPortText, '-o', 'ConnectTimeout=15', '-o', 'StrictHostKeyChecking=accept-new')
$scpOptions = @('-P', $sshPortText, '-o', 'ConnectTimeout=15', '-o', 'StrictHostKeyChecking=accept-new')
if ($IdentityFile) {
    $identityPath = (Resolve-Path -LiteralPath $IdentityFile).Path
    $sshOptions += @('-i', $identityPath)
    $scpOptions += @('-i', $identityPath)
}

New-Item -ItemType Directory -Path $artifactRoot | Out-Null
$preserveArtifacts = $KeepArtifacts.IsPresent -or $PrepareOnly.IsPresent

try {
    Write-Host "[1/7] Preflight checks" -ForegroundColor Cyan
    Invoke-Native docker 'info'
    Invoke-Native node '--check' (Join-Path $repoRoot 'backend/scripts/init-db.js')
    Invoke-Native node '--check' (Join-Path $repoRoot 'backend/src/modules/admin/admin.controller.js')
    Invoke-Native node '--check' (Join-Path $repoRoot 'backend/src/modules/questionnaire/questionnaire.controller.js')

    Write-Host "[2/7] Frontend production build" -ForegroundColor Cyan
    Push-Location (Join-Path $repoRoot 'frontend')
    try {
        Invoke-Native npm 'ci' '--no-audit' '--no-fund'
        Invoke-Native npm 'run' 'build'
    }
    finally {
        Pop-Location
    }

    if ($SkipTests) {
        Write-Warning 'Backend tests were explicitly skipped.'
    }
    else {
        Write-Host "[3/7] Backend tests with a disposable database" -ForegroundColor Cyan
        $listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 0)
        $listener.Start()
        $testDbPort = ([Net.IPEndPoint]$listener.LocalEndpoint).Port
        $listener.Stop()
        $testProject = "imulab-test-$([Guid]::NewGuid().ToString('N').Substring(0, 12))"
        $previousDbPort = $env:DB_PORT
        $previousDatabaseUrl = $env:DATABASE_URL
        $env:DB_PORT = [string]$testDbPort
        $env:DATABASE_URL = "postgres://auth_user:superpassword@127.0.0.1:${testDbPort}/auth_db"

        try {
            Push-Location $repoRoot
            try {
                Invoke-Native docker 'compose' '-p' $testProject 'up' '-d' 'db'
                Invoke-Native docker 'compose' '-p' $testProject 'run' '--build' '--rm' 'init-db'
            }
            finally {
                Pop-Location
            }

            Push-Location (Join-Path $repoRoot 'backend')
            try {
                Invoke-Native npm 'ci' '--no-audit' '--no-fund'
                Invoke-Native npm 'test' '--' '--runInBand'
            }
            finally {
                Pop-Location
            }
        }
        finally {
            Push-Location $repoRoot
            try {
                & docker compose -p $testProject down -v --remove-orphans
                if ($LASTEXITCODE -ne 0) {
                    Write-Warning "Could not fully remove disposable project $testProject"
                }
            }
            finally {
                Pop-Location
            }
            Restore-EnvironmentValue 'DB_PORT' $previousDbPort
            Restore-EnvironmentValue 'DATABASE_URL' $previousDatabaseUrl
        }
    }

    Write-Host "[4/7] Versioned production images" -ForegroundColor Cyan
    Push-Location $repoRoot
    try {
        Invoke-Native docker 'build' '--pull=false' '-t' "imulab-backend:$Version" '-f' 'backend/Dockerfile' 'backend'
        Invoke-Native docker 'tag' "imulab-backend:$Version" "imulab-init-db:$Version"
        Invoke-Native docker 'build' '--pull=false' '-t' "imulab-frontend:$Version" '-f' 'deployment/frontend.Dockerfile' '.'
    }
    finally {
        Pop-Location
    }
    Invoke-Native docker 'save' '-o' $imageBundlePath "imulab-init-db:$Version" "imulab-backend:$Version" "imulab-frontend:$Version"

    Write-Host "[5/7] Immutable release package" -ForegroundColor Cyan
    $tarArguments = @(
        '-czf', $archivePath,
        '--exclude=./.git',
        '--exclude=./.codex',
        '--exclude=./.ssh',
        '--exclude=./node_modules',
        '--exclude=./frontend/node_modules',
        '--exclude=./backend/node_modules',
        '--exclude=./frontend/dist',
        '--exclude=./backend/.env',
        '--exclude=./frontend/.env',
        '--exclude=./.env',
        '--exclude=./.env.*',
        '--exclude=*.log',
        '-C', $repoRoot,
        '.'
    )
    Invoke-Native tar @tarArguments
    Copy-Item -LiteralPath (Join-Path $repoRoot 'scripts/deploy.sh') -Destination $installerPath

    $archiveHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $archivePath).Hash.ToLowerInvariant()
    $imageBundleHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $imageBundlePath).Hash.ToLowerInvariant()
    Write-Host "Source SHA-256: $archiveHash"
    Write-Host "Images SHA-256: $imageBundleHash"

    if ($PrepareOnly) {
        Write-Host "Artifacts prepared in $artifactRoot" -ForegroundColor Green
        return
    }

    Write-Host "[6/7] Upload to ${remoteTarget}:${SshPort}" -ForegroundColor Cyan
    Invoke-Native scp @scpOptions $archivePath $imageBundlePath $installerPath "${remoteTarget}:/tmp/"

    Write-Host "[7/7] Transactional server activation" -ForegroundColor Cyan
    $remoteCommand = "sudo mkdir -p '$RemotePath/scripts' && sudo install -o root -g root -m 0755 '$remoteInstaller' '$RemotePath/scripts/deploy.sh' && sudo '$RemotePath/scripts/deploy.sh' --archive '$remoteArchive' --images '$remoteImageBundle' --source-sha256 '$archiveHash' --images-sha256 '$imageBundleHash' --version '$Version' --base-dir '$RemotePath' && rm -f '$remoteInstaller'"
    Invoke-Native ssh @sshOptions '-tt' $remoteTarget $remoteCommand

    Write-Host "Deployment $Version completed successfully." -ForegroundColor Green
    Write-Host "Internal health endpoint on the server: http://127.0.0.1/health"
}
finally {
    if ($preserveArtifacts) {
        Write-Host "Artifacts retained: $artifactRoot"
    }
    elseif (Test-Path -LiteralPath $artifactRoot) {
        $resolvedArtifactRoot = (Resolve-Path -LiteralPath $artifactRoot).Path
        $expectedTempParent = [IO.Path]::GetTempPath().TrimEnd('\')
        if (-not $resolvedArtifactRoot.StartsWith($expectedTempParent, [StringComparison]::OrdinalIgnoreCase)) {
            throw "Refusing to remove unexpected artifact directory: $resolvedArtifactRoot"
        }
        Remove-Item -LiteralPath $resolvedArtifactRoot -Recurse -Force
    }
}
