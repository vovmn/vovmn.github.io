[CmdletBinding()]
param(
    [string]$Server = '192.168.0.42',
    [string]$SshUser = 'user',
    [string]$RemotePath = '/opt/imulab',
    [string]$Version = (Get-Date -Format 'yyyyMMdd-HHmmss'),
    [switch]$SkipTests
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
    throw "RemotePath must be an explicit path inside /opt."
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$remoteTarget = "${SshUser}@${Server}"
$remoteArchive = "/tmp/imulab-${Version}.tar.gz"
$remoteInstaller = "/tmp/imulab-deploy-${Version}.sh"
$tempRoot = Join-Path ([IO.Path]::GetTempPath()) "imulab-deploy-$([Guid]::NewGuid().ToString('N'))"
$archivePath = Join-Path $tempRoot "imulab-${Version}.tar.gz"

foreach ($command in @('ssh', 'scp', 'tar', 'node', 'npm')) {
    if (-not (Get-Command $command -ErrorAction SilentlyContinue)) {
        throw "Required command is not available: $command"
    }
}

New-Item -ItemType Directory -Path $tempRoot | Out-Null

try {
    Write-Host "[1/6] Checking source files..."
    Invoke-Native node '--check' (Join-Path $repoRoot 'backend/scripts/init-db.js')
    Invoke-Native node '--check' (Join-Path $repoRoot 'backend/src/modules/questionnaire/questionnaire.controller.js')
    Invoke-Native node '--check' (Join-Path $repoRoot 'backend/src/modules/admin/admin.controller.js')

    Write-Host "[2/6] Building frontend..."
    Push-Location (Join-Path $repoRoot 'frontend')
    try {
        Invoke-Native npm 'ci' '--no-audit' '--no-fund'
        Invoke-Native npm 'run' 'build'
    }
    finally {
        Pop-Location
    }

    if (-not $SkipTests) {
        if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
            throw 'Docker is required for database tests. Start Docker Desktop or use -SkipTests for an emergency deployment.'
        }

        Write-Host "[3/6] Starting the local test database and running backend tests..."
        Push-Location $repoRoot
        try {
            Invoke-Native docker 'compose' 'up' '-d' 'db'
            Invoke-Native docker 'compose' 'run' '--rm' 'init-db'
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
    else {
        Write-Warning 'Backend tests were skipped.'
    }

    Write-Host "[4/6] Creating release archive $Version..."
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

    $archiveHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $archivePath).Hash.ToLowerInvariant()
    Write-Host "Archive SHA-256: $archiveHash"

    Write-Host "[5/6] Uploading release to $remoteTarget..."
    Invoke-Native scp $archivePath "${remoteTarget}:${remoteArchive}"
    Invoke-Native scp (Join-Path $repoRoot 'scripts/deploy.sh') "${remoteTarget}:${remoteInstaller}"

    Write-Host "[6/6] Deploying on the server..."
    $remoteCommand = "sudo mkdir -p '$RemotePath/scripts' && sudo install -o root -g root -m 0755 '$remoteInstaller' '$RemotePath/scripts/deploy.sh' && sudo '$RemotePath/scripts/deploy.sh' --archive '$remoteArchive' --version '$Version' --base-dir '$RemotePath'"
    Invoke-Native ssh '-tt' $remoteTarget $remoteCommand

    Write-Host "Deployment $Version completed successfully." -ForegroundColor Green
    Write-Host "Site: http://$Server/"
}
finally {
    if (Test-Path -LiteralPath $archivePath) {
        Remove-Item -LiteralPath $archivePath -Force
    }
    if (Test-Path -LiteralPath $tempRoot) {
        Remove-Item -LiteralPath $tempRoot -Force
    }
}
