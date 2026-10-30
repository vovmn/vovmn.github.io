# IMLab architecture

Last verified: 2026-10-28.

## System context

IMLab is a single-page patient/admin application backed by one Express API and one PostgreSQL database.

```text
Browser
  -> nginx (static Vue SPA)
       -> /api, /auth, /health proxy
            -> Express application
                 -> PostgreSQL

Compose startup: PostgreSQL -> init-db -> backend -> frontend/nginx
```

In local development, Vite serves the frontend separately and the Axios client defaults to `http://localhost:5000`. In the production build, the client uses relative `/api` URLs and nginx proxies API traffic to the backend container.

## Frontend boundary

`frontend/src/main.js` owns route registration and navigation guards. The app uses hash history, so browser routes remain behind `#` and do not require server-side route rewriting beyond the SPA fallback.

Pinia's auth store holds the access token and current user. Protected navigation checks for an access token, loads the current user when needed, and redirects unauthenticated users to Login. Admin navigation additionally checks `user.role === 'admin'`. These client checks improve UX but are not a security boundary; backend middleware must enforce authorization.

The frontend currently has more than one Axios helper. `frontend/src/services/http.js` is the visible general-purpose client with bearer-token and refresh behavior. Call sites are the authority when changing or consolidating API access.

## Backend boundary

`backend/src/server.js` is the process entry point. It loads the local environment and starts the Express app exported by `backend/src/app.js`.

`app.js` composes:

1. CORS with credentials for configured or local origins.
2. Cookie and JSON parsing.
3. Admin, appointment and questionnaire modules.
4. Authentication routes at both `/auth` and `/api/auth`.
5. A database-backed `/health` endpoint.

Feature modules generally follow route -> controller -> database access. The auth module adds service and repository layers. Shared authentication/authorization middleware lives under `backend/src/middlewares/`.

## Authentication and authorization

- There is no public registration route. Administrators provision patients through `POST /api/admin/users`.
- New patients require a username, password, recorded age and gender; contact/profile fields are optional.
- Legacy patients with incomplete age/gender are listed for remediation in the admin UI and cannot receive or open questionnaires until those fields are complete.
- Passwords are handled with Argon2 in the auth flow.
- Successful authentication produces an access token used as a bearer token by protected API calls.
- Refresh state is persisted in the database and a refresh cookie is used by the refresh endpoint.
- `requireAuth` validates protected requests; `requireAdmin` additionally enforces the admin role.
- Admin routes are protected on the server. Frontend route guards must never be treated as sufficient authorization.

Exact token lifetimes, cookie attributes and invalidation behavior belong to the auth implementation and tests; update those together when the policy changes.

## Questionnaire model and flow

The database model separates reusable definitions from patient activity:

- body systems, questions and answer options define questionnaire content;
- `user_questionnaire_assignments` creates an independent patient assignment with lifecycle and priority;
- `user_answers` belongs to an assignment, allowing repeated assignments of the same system;
- questionnaire releases record the imported data version and checksum.

The observed application flow is:

```text
Admin creates assignment
  -> patient lists assignments
  -> patient starts one assignment
  -> backend returns its active questions/options
  -> patient submits validated answers
  -> assignment becomes completed
```

The canonical state and integrity rules are documented in `docs/database-schema.md` and enforced primarily in PostgreSQL plus the questionnaire controller.

## Schema ownership and initialization

The intended fresh-database source is `backend/schema.sql`. Compose runs `backend/scripts/init-db.js` after PostgreSQL becomes healthy; that script upgrades schema and imports `python/questionnaire.json` transactionally.

There are currently overlapping schema mechanisms: `backend/init.sql`, the transactional initializer, and schema-ensure statements in `backend/src/app.js`. This is architectural debt, not three independent sources of truth. Until consolidated, any schema change must be checked against all consumers and tested on both a fresh and an existing database.

## Deployment topology

Local Compose publishes PostgreSQL, backend and frontend ports and contains development credentials. Production Compose requires secrets through environment variables, uses version-tagged images, health-checks backend/frontend, and exposes only the frontend HTTP port. nginx serves static assets and forwards application traffic.

The deployment scripts build and validate releases and include database backup/rollback behavior. They are operational tools with production impact and must not be run as ordinary tests.

## Trust boundaries and invariants

- Only backend authorization protects patient/admin data.
- Database constraints and transactions protect assignment and answer integrity.
- Environment variables and `.env` files carry credentials; logs and generated output must not reveal them.
- Questionnaire JSON is data input, not executable configuration. Its version/checksum must match imported database state.
- Generated frontend assets are replaceable build output; source changes belong under `frontend/src/`.

## Known architectural debt

- Schema changes are split across multiple mechanisms, including backend startup.
- `backend/src/server.js` currently logs an environment value named `DB_PASSWORD`; remove secret-value logging before production use.
- Development Compose contains hard-coded database and JWT secrets and must remain local-only.
- Multiple Axios helpers and multiple questionnaire authentication patterns increase drift risk.
- Generated assets and log files are present in the repository working tree, obscuring source-only diffs.
- The root README is still a generic Vue template and does not describe the full system.

Track remediation and evidence in `docs/TASKS.md`; record durable choices in `docs/DECISIONS.md`.
