# IMLab project map

Last verified: 2026-10-28 on branch `imulab`.

This map describes the active Vue/Node repository. A separate detached .NET worktree exists beside this checkout, but it is not part of this repository map.

## Runtime overview

| Area | Technology | Primary entry point | Responsibility |
| --- | --- | --- | --- |
| Frontend | Vue 3, Vite, Pinia, Vue Router, Axios | `frontend/src/main.js` | SPA routes, authentication state, patient/admin screens |
| Backend | Node.js, Express 5 (CommonJS) | `backend/src/server.js` -> `backend/src/app.js` | HTTP API, authentication, authorization, questionnaire and appointment workflows |
| Database | PostgreSQL 16 via `pg` | `backend/schema.sql`; `backend/scripts/init-db.js` | Users, sessions, questionnaire definitions/assignments/answers, appointments |
| Edge | nginx in the frontend container | `deployment/nginx.conf` | Static SPA delivery and reverse proxying to the backend |
| Orchestration | Docker Compose | `docker-compose.yml`; `docker-compose.prod.yml` | Database, initialization, backend and frontend services |

## Top-level layout

| Path | Contents and ownership |
| --- | --- |
| `.codex/` | Project Codex defaults and lightweight agent definitions |
| `.github/` | Repository automation metadata |
| `backend/` | Express API, PostgreSQL access, schema initialization and Jest/Supertest tests |
| `deployment/` | Frontend container build and nginx configuration |
| `docs/` | Architecture, project map, decisions, tasks and database documentation |
| `frontend/` | Vue SPA source, frontend package manifest and generated `dist/` output |
| `legacy/` | Historical Vue files; not an active entry point |
| `python/` | Questionnaire source JSON and a legacy/auxiliary database initializer |
| `scripts/` | Local startup and production deployment scripts |
| `docker-compose.yml` | Local four-service stack with published database/backend/frontend ports |
| `docker-compose.prod.yml` | Versioned production images, health checks and environment-provided secrets |
| `package.json` | Root development dependencies only; it defines no runnable scripts |

## Frontend map

- `frontend/src/main.js`: creates the app, Pinia and hash router; defines route guards for authenticated, public-only and admin routes.
- `frontend/src/App.vue`: application shell.
- `frontend/src/pages/`: route-level views: `Admin.vue`, `About.vue`, `documents.vue`, `home.vue` (contacts), `Info.vue`, `login.vue`, `register.vue`, `record.vue`, and `QuestionnaireView.vue`.
- `frontend/src/components/`: shared UI including `Header.vue`, `QuestionWidget.vue`, `DocumentCard.vue`, and `main_content.vue`.
- `frontend/src/stores/auth.js`: Pinia authentication state and current-user lifecycle.
- `frontend/src/services/http.js`: primary Axios client, bearer-token injection and refresh handling. Production defaults to `/api`; development defaults to `http://localhost:5000`.
- `frontend/src/services/authApi.js` and `frontend/src/api/axios.js`: additional API/auth client helpers. Check call sites before consolidating them.
- `frontend/vite.config.js`: Vite development/build configuration.

## Backend map

- `backend/src/server.js`: loads `backend/.env`, imports the app and listens on `PORT` (default `5000`).
- `backend/src/app.js`: CORS/cookie/JSON middleware, route mounting, health check, and current runtime schema-ensure statements.
- `backend/src/db/pool.js`: shared PostgreSQL connection pool.
- `backend/src/middlewares/requireAuth.js`: bearer-token authentication.
- `backend/src/middlewares/requireAdmin.js`: admin-role enforcement.
- `backend/src/modules/auth/`: routes, controller, service and repository for registration, login, current user, refresh and logout.
- `backend/src/modules/admin/`: admin-only user/system/assignment/appointment operations.
- `backend/src/modules/questionnaire/`: patient questionnaire system lookup, assignment start, submission and assignment listing.
- `backend/src/modules/appointments/`: patient appointment listing.

### Mounted HTTP routes

| Mount | Notable endpoints |
| --- | --- |
| `/auth` and `/api/auth` | `POST /login`, `POST /register`, `GET /me`, `POST /logout`, `POST /refresh` |
| `/api/admin` | Users, systems, questionnaire assignments and appointments; router is guarded by auth and admin middleware |
| `/api/questionnaire` | Systems, assignment start, answer submission and the current user's assignments |
| `/api/appointments` | `GET /my` |
| `/health` | Backend and database health probe |

## Database and questionnaire sources

- `backend/schema.sql`: canonical schema for a fresh database according to `docs/database-schema.md`.
- `backend/scripts/init-db.js`: transactional initialization/upgrade and questionnaire import used by Compose.
- `backend/init.sql`: older/parallel SQL initialization input; inspect consumers before changing it.
- `backend/src/app.js`: currently performs additional schema changes at process startup.
- `python/questionnaire.json`: versioned questionnaire data imported by the initializer.
- `python/init_db.py`: auxiliary/legacy Python initializer; it is not used by the documented Compose startup path.
- `docs/database-schema.md`: entity, integrity and questionnaire-assignment lifecycle reference.

## Tests and commands

Run commands from the named directory.

| Purpose | Directory | Command | Notes |
| --- | --- | --- | --- |
| Start frontend dev server | `frontend/` | `npm run dev` | Vite |
| Build frontend | `frontend/` | `npm run build` | Writes generated `frontend/dist/` |
| Check frontend without fixes | `frontend/` | `npx eslint .` | Preferred verification command |
| Apply frontend lint fixes | `frontend/` | `npm run lint` | Mutates files because the script includes `--fix` |
| Start backend | `backend/` | `npm start` | Runs `src/server.js` |
| Start backend with watcher | `backend/` | `npm run dev` | Uses nodemon |
| Run backend tests | `backend/` | `npm test` | Jest/Supertest; database-dependent tests require suitable environment |
| Start local stack | repository root | `docker compose up --build` | Starts db, init-db, backend and frontend |

Backend tests live in `backend/tests/`: `auth.test.js`, `questionnaire.test.js`, `questionnaire-submit-db.test.js`, plus shared setup and a shell helper.

## Deployment map

- `deployment/frontend.Dockerfile`: builds the SPA and packages it with nginx.
- `deployment/nginx.conf`: static hosting, SPA fallback, proxy routes and health endpoint.
- `backend/Dockerfile`: backend and database-initializer image build.
- `docker-compose.prod.yml`: production topology, image versioning, environment requirements and health checks.
- `scripts/deploy.ps1` and `scripts/deploy.sh`: deployment workflows. Inspect both when changing deployment behavior; never execute them without explicit authorization.

## Non-source and sensitive areas

- `frontend/dist/` contains generated build output.
- `backend/*.log` files are runtime/build artifacts and may contain environment details.
- `.ssh/` is sensitive; do not inspect or modify private key material during normal development.
- `node_modules/` is installed dependency output, not project source.
- `legacy/` is historical code and should not be treated as the active implementation without call-site evidence.
