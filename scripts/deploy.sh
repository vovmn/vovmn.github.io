#!/usr/bin/env bash
set -Eeuo pipefail

BASE_DIR="/opt/imulab"
BACKUP_DIR="/opt/imulab-backups"
ARCHIVE=""
VERSION=""
PROJECT_NAME="imulab"
HEALTH_URL="http://127.0.0.1/health"

usage() {
  echo "Usage: sudo deploy.sh --archive /tmp/release.tar.gz --version v0.2.1 [--base-dir /opt/imulab]"
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --archive) ARCHIVE="${2:-}"; shift 2 ;;
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

if [[ -z "$ARCHIVE" || -z "$VERSION" ]]; then
  usage
  exit 2
fi

if [[ ! "$VERSION" =~ ^[A-Za-z0-9][A-Za-z0-9._-]{0,63}$ ]]; then
  echo "Invalid version: $VERSION" >&2
  exit 2
fi

if [[ ! -f "$ARCHIVE" ]]; then
  echo "Release archive not found: $ARCHIVE" >&2
  exit 1
fi

case "$BASE_DIR" in
  /opt/*) ;;
  *) echo "Base directory must be an explicit path inside /opt." >&2; exit 2 ;;
esac

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

install -m 0600 "$ENV_FILE" "$BACKUP_PATH/env.production"

if docker container inspect immunolab_db >/dev/null 2>&1; then
  docker exec immunolab_db pg_dump -U auth_user -d auth_db -Fc > "$BACKUP_PATH/auth_db.dump"
fi

if [[ -L "$CURRENT_LINK" && -d "$(readlink -f "$CURRENT_LINK")" ]]; then
  tar -C "$(readlink -f "$CURRENT_LINK")" -czf "$BACKUP_PATH/source-before-update.tar.gz" .
elif [[ -f "$BASE_DIR/docker-compose.prod.yml" ]]; then
  tar \
    --exclude='./releases' \
    --exclude='./shared' \
    -C "$BASE_DIR" -czf "$BACKUP_PATH/source-before-update.tar.gz" .
fi

if [[ -e "$RELEASE_DIR" ]]; then
  echo "Release already exists: $RELEASE_DIR" >&2
  exit 1
fi

mkdir -p "$RELEASE_DIR"
tar --no-same-owner -xzf "$ARCHIVE" -C "$RELEASE_DIR"

if [[ ! -f "$RELEASE_DIR/docker-compose.prod.yml" || ! -f "$RELEASE_DIR/backend/scripts/init-db.js" ]]; then
  echo "Release archive is incomplete." >&2
  exit 1
fi

printf '%s\n' "$VERSION" > "$RELEASE_DIR/VERSION"

compose() {
  docker compose \
    -p "$PROJECT_NAME" \
    --env-file "$ENV_FILE" \
    -f "$RELEASE_DIR/docker-compose.prod.yml" \
    "$@"
}

rollback() {
  local exit_code=$?
  trap - ERR
  set +e

  if [[ $APP_STOPPED -eq 1 || $DEPLOYMENT_STARTED -eq 1 ]]; then
    echo "Deployment failed. Rolling back to $ROLLBACK_TAG..." >&2
    docker stop immunolab_frontend immunolab_backend >/dev/null 2>&1 || true

    if [[ $MIGRATION_STARTED -eq 1 && $DB_BACKUP_READY -eq 1 && -s "$BACKUP_PATH/auth_db.dump" ]]; then
      echo "Restoring PostgreSQL from the pre-migration backup..." >&2
      if ! docker exec -i immunolab_db pg_restore \
        --clean --if-exists --no-owner --no-privileges \
        -U auth_user -d auth_db < "$BACKUP_PATH/auth_db.dump"; then
        echo "Automatic database restore failed; keep the dump for manual recovery." >&2
      fi
    fi

    if docker image inspect "imulab-backend:$ROLLBACK_TAG" >/dev/null 2>&1 \
      && docker image inspect "imulab-frontend:$ROLLBACK_TAG" >/dev/null 2>&1; then
      export APP_VERSION="$ROLLBACK_TAG"
      compose up -d --no-deps backend frontend || true
    else
      echo "Previous application images are unavailable; no application rollback was possible." >&2
    fi
  fi
  echo "Database backup: $BACKUP_PATH/auth_db.dump" >&2
  exit "$exit_code"
}
trap rollback ERR

if docker container inspect immunolab_backend >/dev/null 2>&1; then
  docker tag "$(docker inspect -f '{{.Image}}' immunolab_backend)" "imulab-backend:$ROLLBACK_TAG"
fi
if docker container inspect immunolab_frontend >/dev/null 2>&1; then
  docker tag "$(docker inspect -f '{{.Image}}' immunolab_frontend)" "imulab-frontend:$ROLLBACK_TAG"
fi

export APP_VERSION="$VERSION"
compose build init-db backend frontend
compose up -d db

for _ in {1..30}; do
  if [[ "$(docker inspect -f '{{.State.Health.Status}}' immunolab_db 2>/dev/null || true)" == "healthy" ]]; then
    break
  fi
  sleep 2
done

if [[ "$(docker inspect -f '{{.State.Health.Status}}' immunolab_db 2>/dev/null || true)" != "healthy" ]]; then
  echo "PostgreSQL did not become healthy." >&2
  exit 1
fi

DEPLOYMENT_STARTED=1

# Stop writes before taking the authoritative pre-migration snapshot. The first
# dump above protects against build-time failures; this one guarantees that an
# automatic restore cannot discard requests accepted during the image build.
APP_STOPPED=1
docker stop immunolab_frontend immunolab_backend >/dev/null 2>&1 || true
docker exec immunolab_db pg_dump -U auth_user -d auth_db -Fc \
  > "$BACKUP_PATH/auth_db.pre-migration.dump"
mv "$BACKUP_PATH/auth_db.pre-migration.dump" "$BACKUP_PATH/auth_db.dump"
DB_BACKUP_READY=1
MIGRATION_STARTED=1

compose run --rm --no-deps init-db
compose up -d --no-deps backend

for _ in {1..30}; do
  if [[ "$(docker inspect -f '{{.State.Health.Status}}' immunolab_backend 2>/dev/null || true)" == "healthy" ]]; then
    break
  fi
  sleep 2
done

if [[ "$(docker inspect -f '{{.State.Health.Status}}' immunolab_backend 2>/dev/null || true)" != "healthy" ]]; then
  echo "Backend did not become healthy." >&2
  exit 1
fi

compose up -d --no-deps frontend

for _ in {1..30}; do
  if curl --fail --silent --show-error "$HEALTH_URL" >/dev/null; then
    break
  fi
  sleep 2
done

curl --fail --silent --show-error "$HEALTH_URL" >/dev/null

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

ln -sfn "$RELEASE_DIR" "$CURRENT_LINK"
DEPLOYMENT_STARTED=0
APP_STOPPED=0
MIGRATION_STARTED=0
trap - ERR
rm -f "$ARCHIVE"

echo "Deployment completed: $VERSION"
echo "Current release: $RELEASE_DIR"
echo "Backup: $BACKUP_PATH"
