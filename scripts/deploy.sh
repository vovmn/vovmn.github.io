#!/usr/bin/env bash
set -Eeuo pipefail

BASE_DIR="/opt/imulab"
BACKUP_DIR="/opt/imulab-backups"
ARCHIVE=""
IMAGE_BUNDLE=""
SOURCE_SHA256=""
IMAGES_SHA256=""
VERSION=""
PROJECT_NAME="imulab"
HEALTH_URL="http://127.0.0.1/health"

usage() {
  cat <<'USAGE'
Usage: sudo deploy.sh \
  --archive /tmp/imulab-v1.tar.gz \
  --images /tmp/imulab-v1-images.tar \
  --source-sha256 HEX \
  --images-sha256 HEX \
  --version v1 \
  [--base-dir /opt/imulab] [--backup-dir /opt/imulab-backups]
USAGE
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --archive) ARCHIVE="${2:-}"; shift 2 ;;
    --images) IMAGE_BUNDLE="${2:-}"; shift 2 ;;
    --source-sha256) SOURCE_SHA256="${2:-}"; shift 2 ;;
    --images-sha256) IMAGES_SHA256="${2:-}"; shift 2 ;;
    --version) VERSION="${2:-}"; shift 2 ;;
    --base-dir) BASE_DIR="${2:-}"; shift 2 ;;
    --backup-dir) BACKUP_DIR="${2:-}"; shift 2 ;;
    --help|-h) usage; exit 0 ;;
    *) echo "Unknown argument: $1" >&2; usage; exit 2 ;;
  esac
done

if [[ $EUID -ne 0 ]]; then
  echo "Run this script with sudo." >&2
  exit 1
fi

for command in curl docker flock sha256sum tar; do
  command -v "$command" >/dev/null 2>&1 || {
    echo "Required command is unavailable: $command" >&2
    exit 1
  }
done

if [[ -z "$ARCHIVE" || -z "$IMAGE_BUNDLE" || -z "$VERSION" \
   || -z "$SOURCE_SHA256" || -z "$IMAGES_SHA256" ]]; then
  usage
  exit 2
fi
if [[ ! "$VERSION" =~ ^[A-Za-z0-9][A-Za-z0-9._-]{0,63}$ ]]; then
  echo "Invalid version: $VERSION" >&2
  exit 2
fi
if [[ ! "$SOURCE_SHA256" =~ ^[a-f0-9]{64}$ || ! "$IMAGES_SHA256" =~ ^[a-f0-9]{64}$ ]]; then
  echo "Invalid SHA-256 value." >&2
  exit 2
fi
case "$BASE_DIR" in
  /opt/*) ;;
  *) echo "Base directory must be an explicit path inside /opt." >&2; exit 2 ;;
esac
case "$BACKUP_DIR" in
  /opt/*) ;;
  *) echo "Backup directory must be an explicit path inside /opt." >&2; exit 2 ;;
esac
for file in "$ARCHIVE" "$IMAGE_BUNDLE"; do
  [[ -f "$file" ]] || { echo "Artifact not found: $file" >&2; exit 1; }
done

mkdir -p "$BASE_DIR" "$BACKUP_DIR"
exec 9>"$BASE_DIR/.deploy.lock"
if ! flock -n 9; then
  echo "Another deployment is already running." >&2
  exit 1
fi

echo "$SOURCE_SHA256  $ARCHIVE" | sha256sum --check --status || {
  echo "Source archive checksum mismatch." >&2
  exit 1
}
echo "$IMAGES_SHA256  $IMAGE_BUNDLE" | sha256sum --check --status || {
  echo "Image bundle checksum mismatch." >&2
  exit 1
}
if tar -tzf "$ARCHIVE" | grep -Eq '(^/|(^|/)\.\.(/|$))'; then
  echo "Unsafe path found in release archive." >&2
  exit 1
fi

RELEASES_DIR="$BASE_DIR/releases"
SHARED_DIR="$BASE_DIR/shared"
CURRENT_LINK="$BASE_DIR/current"
RELEASE_DIR="$RELEASES_DIR/$VERSION"
ENV_FILE="$SHARED_DIR/.env.production"
STAMP="$(date +%Y%m%d-%H%M%S)"
BACKUP_PATH="$BACKUP_DIR/$STAMP-$VERSION"
ROLLBACK_TAG="rollback-$STAMP"
PREVIOUS_RELEASE="$(readlink -f "$CURRENT_LINK" 2>/dev/null || true)"
DEPLOYMENT_STARTED=0
APP_STOPPED=0
MIGRATION_STARTED=0
DB_BACKUP_READY=0

mkdir -p "$RELEASES_DIR" "$SHARED_DIR" "$BACKUP_PATH"

if [[ ! -f "$ENV_FILE" && -f "$BASE_DIR/.env.production" ]]; then
  install -m 0600 "$BASE_DIR/.env.production" "$ENV_FILE"
fi
if [[ ! -f "$ENV_FILE" ]]; then
  echo "Production environment file is missing: $ENV_FILE" >&2
  exit 1
fi

if [[ "$PREVIOUS_RELEASE" == "$RELEASE_DIR" && -f "$RELEASE_DIR/SOURCE_SHA256" ]]; then
  if [[ "$(<"$RELEASE_DIR/SOURCE_SHA256")" != "$SOURCE_SHA256" ]]; then
    echo "Version $VERSION is active with different source. Use a new version." >&2
    exit 1
  fi
  if curl --fail --silent "$HEALTH_URL" >/dev/null; then
    echo "Version $VERSION is already active and healthy."
    rm -f "$ARCHIVE" "$IMAGE_BUNDLE"
    exit 0
  fi
  echo "Version $VERSION is active but unhealthy; refusing an implicit overwrite." >&2
  exit 1
fi

if [[ -e "$RELEASE_DIR" ]]; then
  mv "$RELEASE_DIR" "${RELEASE_DIR}.failed-${STAMP}"
fi

rollback() {
  local exit_code="${1:-$?}"
  trap - ERR INT TERM
  set +e

  if [[ $APP_STOPPED -eq 1 || $DEPLOYMENT_STARTED -eq 1 ]]; then
    echo "Deployment failed; rolling back to the previous application images." >&2
    docker stop immunolab_frontend immunolab_backend >/dev/null 2>&1 || true

    if [[ $MIGRATION_STARTED -eq 1 && $DB_BACKUP_READY -eq 1 && -s "$BACKUP_PATH/auth_db.dump" ]]; then
      echo "Restoring PostgreSQL from the pre-migration backup." >&2
      docker exec immunolab_db psql -v ON_ERROR_STOP=1 -U auth_user -d postgres -c \
        "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = 'auth_db' AND pid <> pg_backend_pid();" >/dev/null 2>&1 && \
      docker exec immunolab_db dropdb --if-exists --maintenance-db=postgres -U auth_user auth_db && \
      docker exec immunolab_db createdb --maintenance-db=postgres -U auth_user -O auth_user auth_db && \
      docker exec -i immunolab_db pg_restore \
        --no-owner --no-privileges -U auth_user -d auth_db < "$BACKUP_PATH/auth_db.dump" || \
        echo "Automatic database restore failed; preserve $BACKUP_PATH/auth_db.dump." >&2
    fi

    if docker image inspect "imulab-backend:$ROLLBACK_TAG" >/dev/null 2>&1 \
      && docker image inspect "imulab-frontend:$ROLLBACK_TAG" >/dev/null 2>&1; then
      export APP_VERSION="$ROLLBACK_TAG"
      docker compose \
        -p "$PROJECT_NAME" --env-file "$ENV_FILE" \
        -f "$RELEASE_DIR/docker-compose.prod.yml" \
        up -d --no-deps backend frontend || true
    fi
  fi

  if [[ -d "$RELEASE_DIR" && "$(readlink -f "$CURRENT_LINK" 2>/dev/null || true)" != "$RELEASE_DIR" ]]; then
    mv "$RELEASE_DIR" "${RELEASE_DIR}.failed-${STAMP}" || true
  fi
  echo "Backup retained at: $BACKUP_PATH" >&2
  exit "$exit_code"
}
trap 'rollback $?' ERR
trap 'rollback 130' INT
trap 'rollback 143' TERM

install -m 0600 "$ENV_FILE" "$BACKUP_PATH/env.production"
if [[ -n "$PREVIOUS_RELEASE" && -d "$PREVIOUS_RELEASE" ]]; then
  tar -C "$PREVIOUS_RELEASE" -czf "$BACKUP_PATH/source-before-update.tar.gz" .
fi
if docker container inspect immunolab_db >/dev/null 2>&1; then
  docker exec immunolab_db pg_dump -U auth_user -d auth_db -Fc > "$BACKUP_PATH/auth_db.initial.dump"
fi

mkdir -p "$RELEASE_DIR"
tar --no-same-owner -xzf "$ARCHIVE" -C "$RELEASE_DIR"
if [[ ! -f "$RELEASE_DIR/docker-compose.prod.yml" \
   || ! -f "$RELEASE_DIR/backend/scripts/init-db.js" ]]; then
  echo "Release archive is incomplete." >&2
  exit 1
fi
printf '%s\n' "$VERSION" > "$RELEASE_DIR/VERSION"
printf '%s\n' "$SOURCE_SHA256" > "$RELEASE_DIR/SOURCE_SHA256"

docker load -i "$IMAGE_BUNDLE" >/dev/null
for image in init-db backend frontend; do
  docker image inspect "imulab-$image:$VERSION" >/dev/null 2>&1 || {
    echo "Required image is absent after bundle load: imulab-$image:$VERSION" >&2
    exit 1
  }
done

export APP_VERSION="$VERSION"
compose() {
  docker compose \
    -p "$PROJECT_NAME" \
    --env-file "$ENV_FILE" \
    -f "$RELEASE_DIR/docker-compose.prod.yml" \
    "$@"
}

wait_for_health() {
  local container="$1"
  local attempts="${2:-30}"
  for ((i=1; i<=attempts; i++)); do
    if [[ "$(docker inspect -f '{{.State.Health.Status}}' "$container" 2>/dev/null || true)" == "healthy" ]]; then
      return 0
    fi
    sleep 2
  done
  echo "$container did not become healthy." >&2
  return 1
}

if docker container inspect immunolab_backend >/dev/null 2>&1; then
  docker tag "$(docker inspect -f '{{.Image}}' immunolab_backend)" "imulab-backend:$ROLLBACK_TAG"
fi
if docker container inspect immunolab_frontend >/dev/null 2>&1; then
  docker tag "$(docker inspect -f '{{.Image}}' immunolab_frontend)" "imulab-frontend:$ROLLBACK_TAG"
fi

compose up -d db
wait_for_health immunolab_db 30

DEPLOYMENT_STARTED=1
APP_STOPPED=1
docker stop immunolab_frontend immunolab_backend >/dev/null 2>&1 || true
docker exec immunolab_db pg_dump -U auth_user -d auth_db -Fc > "$BACKUP_PATH/auth_db.pre-migration.dump"
mv "$BACKUP_PATH/auth_db.pre-migration.dump" "$BACKUP_PATH/auth_db.dump"
DB_BACKUP_READY=1
MIGRATION_STARTED=1

compose run --rm --no-deps init-db
compose up -d --no-deps backend
wait_for_health immunolab_backend 45
compose up -d --no-deps frontend
wait_for_health immunolab_frontend 45

curl --fail --silent --show-error "$HEALTH_URL" >/dev/null
registration_status="$(curl --silent --output /dev/null --write-out '%{http_code}' -X POST http://127.0.0.1/auth/register)"
if [[ "$registration_status" != "404" ]]; then
  echo "Public registration endpoint unexpectedly returned HTTP $registration_status." >&2
  exit 1
fi

docker exec immunolab_db psql -v ON_ERROR_STOP=1 -U auth_user -d auth_db -Atc "
DO \$\$
DECLARE
  actual_count integer;
  expected_count integer;
BEGIN
  SELECT count(*) INTO actual_count FROM questions WHERE active;
  SELECT question_count INTO expected_count
    FROM questionnaire_releases ORDER BY imported_at DESC LIMIT 1;
  IF expected_count IS NULL OR actual_count <> expected_count THEN
    RAISE EXCEPTION 'Active questionnaire mismatch: actual %, expected %', actual_count, expected_count;
  END IF;
  IF EXISTS (SELECT 1 FROM questions WHERE active AND code IS NULL) THEN
    RAISE EXCEPTION 'Active questions without stable codes exist';
  END IF;
END \$\$;"

rm -f "$CURRENT_LINK.next"
ln -s "$RELEASE_DIR" "$CURRENT_LINK.next"
mv -Tf "$CURRENT_LINK.next" "$CURRENT_LINK"
DEPLOYMENT_STARTED=0
APP_STOPPED=0
MIGRATION_STARTED=0
trap - ERR INT TERM

rm -f "$ARCHIVE" "$IMAGE_BUNDLE"
echo "Deployment completed: $VERSION"
echo "Current release: $RELEASE_DIR"
echo "Backup: $BACKUP_PATH"
