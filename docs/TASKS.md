# IMLab task register

Last reviewed: 2026-10-28.

Use this file for repository-level technical work. Do not invent owners or dates. Each completed item should include verification evidence.

## Now — safety and source-of-truth cleanup

- [ ] Remove the `DB_PASSWORD` value log from `backend/src/server.js`; verify no other logs expose secrets.
- [ ] Consolidate schema ownership around `backend/schema.sql` and `backend/scripts/init-db.js`; remove runtime schema mutation from `backend/src/app.js` only after fresh/existing database tests cover the transition.
- [ ] Define a non-mutating frontend check script (current `npm run lint` uses `--fix`).
- [ ] Decide and enforce policy for tracked/generated `frontend/dist/` assets and `backend/*.log` files.

## Next — reliability and maintainability

- [ ] Run backend integration tests against an isolated disposable database in CI.
- [ ] Add frontend tests for auth navigation, refresh behavior, admin access and questionnaire submission.
- [ ] Consolidate or clearly separate the Axios clients in `frontend/src/api/axios.js` and `frontend/src/services/http.js`.
- [ ] Standardize questionnaire authentication middleware with the shared backend auth boundary.
- [ ] Replace the generic root README with setup, environment, database, test and deployment guidance linked to these docs.
- [ ] Document whether `backend/init.sql` and `python/init_db.py` remain supported or can be retired.

## Later — operational hardening

- [ ] Move all development credentials to a documented local environment template and prevent unsafe values from reaching production.
- [ ] Add backup/restore rehearsal and rollback verification for the production deployment workflow.
- [ ] Add API contract/error-shape documentation for frontend/backend integration.
- [ ] Review repository contents for sensitive or large artifacts and define retention rules without deleting evidence blindly.

## In progress / requires reconciliation

- [ ] Reconcile the existing uncommitted questionnaire, admin, frontend, schema and deployment changes before splitting them into commits. Preserve current work and verify behavior before marking individual items complete.

## Done

- [x] 2026-10-28 — Added verified project/architecture maps, decision/task logs, repository guidance, project Codex configuration, and lightweight Explorer/Coder/Reviewer agents. Verification: strict Codex configuration parsing plus targeted repository checks.

## Completion template

When moving an item to Done, record:

- date;
- changed files or commit/PR;
- exact checks run and their result;
- any residual risk or intentionally skipped check.
