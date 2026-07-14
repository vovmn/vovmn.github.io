#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
COMPOSE_DIR="$ROOT_DIR/src"

cd "$COMPOSE_DIR"

docker compose version
docker compose pull db || true
docker compose pull reverse-proxy || true
docker compose build web
docker compose up -d --remove-orphans
docker compose ps

for attempt in {1..30}; do
  if curl -fsS http://localhost/openapi/v1.json >/dev/null; then
    echo "Immunitas is available on http://localhost"
    exit 0
  fi

  echo "Waiting for Immunitas to start... ($attempt/30)"
  sleep 2
done

docker compose logs --tail=120 web reverse-proxy
exit 1
