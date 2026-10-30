# IMLab architecture decisions

This is a lightweight decision log. Append new decisions; do not rewrite prior entries solely to match the current implementation. “Observed” means the repository proves the choice, while the original rationale was not recorded.

## ADR-001 — Vue single-page application with hash routing

- Date recorded: 2026-10-28
- Status: Observed / accepted implementation
- Decision: The frontend is a Vue 3 SPA built with Vite and uses Vue Router hash history.
- Evidence: `frontend/src/main.js`, `frontend/package.json`.
- Consequences: Static hosting does not need server rewrites for every client route; shared URLs contain a hash fragment.
- Original rationale: Not recorded.

## ADR-002 — One Express API with feature modules

- Date recorded: 2026-10-28
- Status: Observed / accepted implementation
- Decision: A single Express 5 process exposes auth, admin, questionnaire and appointment modules backed by PostgreSQL.
- Evidence: `backend/src/server.js`, `backend/src/app.js`, `backend/src/modules/`.
- Consequences: Deployment is simple and modules share one process and database pool; module boundaries are conventions rather than separate services.
- Original rationale: Not recorded.

## ADR-003 — PostgreSQL and assignment-centric questionnaires

- Date recorded: 2026-10-28
- Status: Observed / accepted implementation
- Decision: PostgreSQL is the persistence layer. Each patient questionnaire run has its own assignment, and answers attach to that assignment rather than directly to a patient/system pair.
- Evidence: `backend/schema.sql`, `docs/database-schema.md`, `backend/src/modules/questionnaire/questionnaire.controller.js`.
- Consequences: The same body system can be assigned repeatedly without overwriting prior answers; lifecycle and ownership constraints are database concerns.
- Original rationale: Partly documented as integrity rules in `docs/database-schema.md`; no earlier ADR found.

## ADR-004 — Transactional initialization with versioned questionnaire data

- Date recorded: 2026-10-28
- Status: Observed / accepted direction
- Decision: Compose runs `backend/scripts/init-db.js` to upgrade the database and import `python/questionnaire.json`; releases record a version and checksum.
- Evidence: `docker-compose.yml`, `docker-compose.prod.yml`, `backend/scripts/init-db.js`, `docs/database-schema.md`.
- Consequences: Database setup is repeatable and questionnaire data can be audited. Runtime schema mutations in `backend/src/app.js` remain to be consolidated into this path.
- Original rationale: Not recorded beyond the current database documentation.

## ADR-005 — nginx edge and relative production API URLs

- Date recorded: 2026-10-28
- Status: Observed / accepted implementation
- Decision: The production frontend container serves the SPA through nginx, and browser API calls use same-origin relative paths that nginx proxies to the backend.
- Evidence: `deployment/frontend.Dockerfile`, `deployment/nginx.conf`, `frontend/src/services/http.js`, `docker-compose.prod.yml`.
- Consequences: Production avoids a browser-visible backend host and centralizes routing/CORS at the edge; proxy paths and backend mounts must remain aligned.
- Original rationale: Not recorded.

## ADR-006 — Project-scoped, lightweight Codex subagents

- Date recorded: 2026-10-28
- Status: Accepted
- Decision: Keep orchestration defaults and Explorer/Coder/Reviewer definitions under `.codex/`. Use GPT-6 Luna with bounded instructions and reduced optional capabilities; keep permissions and the main model inherited from the user's Codex configuration.
- Evidence: `.codex/config.toml`, `.codex/agents/`, `AGENTS.md`.
- Consequences: Exploration and review can stay out of the main agent context. Subagents are reserved for independent bounded work because parallel agents add their own model/tool usage and can increase total tokens.
- Rationale: Reduce expensive main-agent context and keep repeatable tasks narrow without overwriting personal Codex settings.

## ADR-007 — Administrator-managed patient accounts and recorded age

- Date recorded: 2026-10-28
- Status: Accepted
- Decision: Remove public registration. Only authenticated administrators create patient accounts. A patient must have username, password hash, gender and an age stored with `age_recorded_at`; contact details and birth date are optional.
- Evidence: `backend/src/modules/auth/auth.routes.js`, `backend/src/modules/admin/`, `backend/schema.sql`, `frontend/src/pages/Admin.vue`.
- Consequences: Account creation has a server-side authorization boundary. Recorded age can become stale, so medical consumers must use `age_recorded_at`; the system never invents a birth date from an integer age.
- Rationale: The clinic controls access while retaining the minimal fields requested for questionnaire filtering and interpretation.
