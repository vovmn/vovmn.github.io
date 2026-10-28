const fs = require('fs');
const path = require('path');
const crypto = require('crypto');
const { Pool } = require('pg');

const connectionString = process.env.DATABASE_URL;
const questionnairePath = process.env.QUESTIONNAIRE_PATH || path.resolve(__dirname, '..', 'questionnaire.json');

if (!connectionString) {
  console.error('DATABASE_URL is required');
  process.exit(1);
}

if (!fs.existsSync(questionnairePath)) {
  console.error(`Questionnaire file not found: ${questionnairePath}`);
  process.exit(1);
}

const questionnaireSource = fs.readFileSync(questionnairePath, 'utf8');
const data = JSON.parse(questionnaireSource);
const questionnaireChecksum = crypto.createHash('sha256').update(questionnaireSource).digest('hex');
const schemaVersion = '20261028_02';
const pool = new Pool({ connectionString });

async function createSchema(client) {
  await client.query('CREATE EXTENSION IF NOT EXISTS "uuid-ossp"');

  await client.query(`
    CREATE TABLE IF NOT EXISTS users (
      id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
      username VARCHAR(100) NOT NULL UNIQUE,
      email VARCHAR(255) NOT NULL UNIQUE,
      password_hash TEXT NOT NULL,
      role VARCHAR(50) NOT NULL DEFAULT 'user',
      phone VARCHAR(50),
      birth_date DATE,
      residence VARCHAR(255),
      gender CHAR(1) CHECK (gender IN ('M', 'F')),
      created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    );

    CREATE TABLE IF NOT EXISTS schema_migrations (
      version VARCHAR(50) PRIMARY KEY,
      description TEXT NOT NULL,
      applied_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
    );

    CREATE TABLE IF NOT EXISTS questionnaire_releases (
      version VARCHAR(50) PRIMARY KEY,
      source_sha256 CHAR(64) NOT NULL,
      question_count INTEGER NOT NULL CHECK (question_count >= 0),
      imported_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
    );

    CREATE TABLE IF NOT EXISTS refresh_tokens (
      id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
      user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
      token_hash TEXT NOT NULL,
      expires_at TIMESTAMP NOT NULL,
      created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    );

    CREATE TABLE IF NOT EXISTS body_systems (
      id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
      name VARCHAR(100) NOT NULL,
      code VARCHAR(50) NOT NULL UNIQUE
    );

    CREATE TABLE IF NOT EXISTS questions (
      id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
      system_id UUID NOT NULL REFERENCES body_systems(id) ON DELETE CASCADE,
      code VARCHAR(100),
      question_text TEXT NOT NULL,
      question_type VARCHAR(20) NOT NULL CHECK (question_type IN ('boolean','numeric','single_choice','multiple_choice','text')),
      gender_filter CHAR(1) DEFAULT NULL CHECK (gender_filter IN ('M','F', NULL)),
      required BOOLEAN DEFAULT TRUE,
      active BOOLEAN NOT NULL DEFAULT TRUE,
      allow_unknown BOOLEAN DEFAULT FALSE,
      help_text TEXT,
      recall_period VARCHAR(100),
      display_condition JSONB,
      validation JSONB,
      alert JSONB,
      sort_order INTEGER DEFAULT 0,
      created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    );

    CREATE TABLE IF NOT EXISTS answer_options (
      id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
      question_id UUID NOT NULL REFERENCES questions(id) ON DELETE CASCADE,
      option_text TEXT NOT NULL,
      option_code VARCHAR(50),
      sort_order INTEGER DEFAULT 0,
      UNIQUE (question_id, option_code)
    );

    CREATE TABLE IF NOT EXISTS user_questionnaire_assignments (
      id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
      user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
      system_id UUID NOT NULL REFERENCES body_systems(id),
      assigned_by UUID REFERENCES users(id) ON DELETE SET NULL,
      status VARCHAR(20) DEFAULT 'pending' CHECK (status IN ('pending','in_progress','completed','cancelled')),
      priority VARCHAR(20) NOT NULL DEFAULT 'normal' CHECK (priority IN ('low','normal','high','urgent')),
      due_at TIMESTAMP,
      note TEXT,
      questionnaire_version VARCHAR(50),
      assigned_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
      started_at TIMESTAMP,
      completed_at TIMESTAMP,
      cancelled_at TIMESTAMP,
      updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    );

    CREATE TABLE IF NOT EXISTS user_answers (
      id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
      user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
      question_id UUID NOT NULL REFERENCES questions(id) ON DELETE CASCADE,
      assignment_id UUID REFERENCES user_questionnaire_assignments(id),
      value_boolean BOOLEAN,
      value_numeric DOUBLE PRECISION,
      value_text TEXT,
      value_unknown BOOLEAN NOT NULL DEFAULT FALSE,
      skipped BOOLEAN NOT NULL DEFAULT FALSE,
      skip_reason VARCHAR(50),
      selected_option_id UUID REFERENCES answer_options(id),
      answered_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
      CHECK (skip_reason IS NULL OR skip_reason IN ('unknown','not_applicable','prefer_not_to_answer','other'))
    );

    CREATE TABLE IF NOT EXISTS appointments (
      id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
      user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
      scheduled_at TIMESTAMP NOT NULL,
      specialist VARCHAR(255),
      doctor VARCHAR(255),
      status VARCHAR(20) DEFAULT 'scheduled' CHECK (status IN ('scheduled','completed','cancelled')),
      comment TEXT,
      created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    );

    CREATE INDEX IF NOT EXISTS idx_questions_system ON questions(system_id);
    CREATE INDEX IF NOT EXISTS idx_answer_options_question ON answer_options(question_id);
    CREATE INDEX IF NOT EXISTS idx_assignments_user ON user_questionnaire_assignments(user_id);
    CREATE INDEX IF NOT EXISTS idx_assignments_system ON user_questionnaire_assignments(system_id);
    CREATE INDEX IF NOT EXISTS idx_user_answers_user ON user_answers(user_id);
    CREATE INDEX IF NOT EXISTS idx_user_answers_question ON user_answers(question_id);
    CREATE INDEX IF NOT EXISTS idx_refresh_tokens_user_expires ON refresh_tokens(user_id, expires_at);
    CREATE UNIQUE INDEX IF NOT EXISTS uq_refresh_tokens_hash ON refresh_tokens(token_hash);
    CREATE INDEX IF NOT EXISTS idx_appointments_user ON appointments(user_id);
    CREATE INDEX IF NOT EXISTS idx_appointments_scheduled_at ON appointments(scheduled_at);
    CREATE INDEX IF NOT EXISTS idx_appointments_status_scheduled
      ON appointments(status, scheduled_at);
  `);

  await client.query(`
    ALTER TABLE questions ADD COLUMN IF NOT EXISTS code VARCHAR(100);
    ALTER TABLE questions ADD COLUMN IF NOT EXISTS active BOOLEAN NOT NULL DEFAULT TRUE;
    ALTER TABLE questions ADD COLUMN IF NOT EXISTS allow_unknown BOOLEAN DEFAULT FALSE;
    ALTER TABLE questions ADD COLUMN IF NOT EXISTS help_text TEXT;
    ALTER TABLE questions ADD COLUMN IF NOT EXISTS recall_period VARCHAR(100);
    ALTER TABLE questions ADD COLUMN IF NOT EXISTS display_condition JSONB;
    ALTER TABLE questions ADD COLUMN IF NOT EXISTS validation JSONB;
    ALTER TABLE questions ADD COLUMN IF NOT EXISTS alert JSONB;
    ALTER TABLE questions DROP CONSTRAINT IF EXISTS questions_system_id_question_text_key;
    ALTER TABLE user_answers ADD COLUMN IF NOT EXISTS value_unknown BOOLEAN NOT NULL DEFAULT FALSE;
    ALTER TABLE user_answers ADD COLUMN IF NOT EXISTS skipped BOOLEAN NOT NULL DEFAULT FALSE;
    ALTER TABLE user_answers ADD COLUMN IF NOT EXISTS skip_reason VARCHAR(50);
    ALTER TABLE user_answers ADD COLUMN IF NOT EXISTS assignment_id UUID;
    ALTER TABLE user_questionnaire_assignments ADD COLUMN IF NOT EXISTS assigned_by UUID REFERENCES users(id) ON DELETE SET NULL;
    ALTER TABLE user_questionnaire_assignments ADD COLUMN IF NOT EXISTS priority VARCHAR(20) NOT NULL DEFAULT 'normal';
    ALTER TABLE user_questionnaire_assignments ADD COLUMN IF NOT EXISTS due_at TIMESTAMP;
    ALTER TABLE user_questionnaire_assignments ADD COLUMN IF NOT EXISTS note TEXT;
    ALTER TABLE user_questionnaire_assignments ADD COLUMN IF NOT EXISTS questionnaire_version VARCHAR(50);
    ALTER TABLE user_questionnaire_assignments ADD COLUMN IF NOT EXISTS started_at TIMESTAMP;
    ALTER TABLE user_questionnaire_assignments ADD COLUMN IF NOT EXISTS cancelled_at TIMESTAMP;
    ALTER TABLE user_questionnaire_assignments ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP;
    ALTER TABLE user_questionnaire_assignments DROP CONSTRAINT IF EXISTS user_questionnaire_assignments_user_id_system_id_key;
    ALTER TABLE user_questionnaire_assignments DROP CONSTRAINT IF EXISTS user_questionnaire_assignments_status_check;
    ALTER TABLE user_questionnaire_assignments ADD CONSTRAINT user_questionnaire_assignments_status_check
      CHECK (status IN ('pending','in_progress','completed','cancelled'));
    ALTER TABLE user_questionnaire_assignments DROP CONSTRAINT IF EXISTS user_questionnaire_assignments_priority_check;
    ALTER TABLE user_questionnaire_assignments ADD CONSTRAINT user_questionnaire_assignments_priority_check
      CHECK (priority IN ('low','normal','high','urgent'));
    ALTER TABLE user_answers DROP CONSTRAINT IF EXISTS user_answers_skip_reason_check;
    ALTER TABLE user_answers ADD CONSTRAINT user_answers_skip_reason_check
      CHECK (skip_reason IS NULL OR skip_reason IN ('unknown','not_applicable','prefer_not_to_answer','other'));
    ALTER TABLE user_answers DROP CONSTRAINT IF EXISTS user_answers_user_id_question_id_selected_option_id_key;
    DROP INDEX IF EXISTS uq_answers_assignment_question_option;
    CREATE UNIQUE INDEX uq_answers_assignment_question_option
      ON user_answers(assignment_id, question_id, selected_option_id) NULLS NOT DISTINCT
      WHERE assignment_id IS NOT NULL;
    CREATE UNIQUE INDEX IF NOT EXISTS uq_questions_system_code ON questions(system_id, code);
    CREATE INDEX IF NOT EXISTS idx_questions_active_flow
      ON questions(system_id, gender_filter, sort_order) WHERE active;
    CREATE INDEX IF NOT EXISTS idx_user_answers_assignment ON user_answers(assignment_id);
    CREATE INDEX IF NOT EXISTS idx_assignments_status_due
      ON user_questionnaire_assignments(status, due_at);
    CREATE INDEX IF NOT EXISTS idx_assignments_user_status_due
      ON user_questionnaire_assignments(user_id, status, due_at);
  `);

  await client.query(`
    ALTER TABLE users DROP CONSTRAINT IF EXISTS users_role_check;
    ALTER TABLE users ADD CONSTRAINT users_role_check
      CHECK (role IN ('user','admin')) NOT VALID;

    ALTER TABLE questions DROP CONSTRAINT IF EXISTS questions_active_code_check;
    ALTER TABLE questions ADD CONSTRAINT questions_active_code_check
      CHECK (NOT active OR code IS NOT NULL) NOT VALID;

    ALTER TABLE user_questionnaire_assignments DROP CONSTRAINT IF EXISTS assignments_lifecycle_check;
    ALTER TABLE user_questionnaire_assignments ADD CONSTRAINT assignments_lifecycle_check
      CHECK (
        (status = 'pending' AND completed_at IS NULL AND cancelled_at IS NULL)
        OR (status = 'in_progress' AND started_at IS NOT NULL AND completed_at IS NULL AND cancelled_at IS NULL)
        OR (status = 'completed' AND completed_at IS NOT NULL AND cancelled_at IS NULL)
        OR (status = 'cancelled' AND cancelled_at IS NOT NULL)
      ) NOT VALID;

    ALTER TABLE user_answers DROP CONSTRAINT IF EXISTS user_answers_payload_check;
    ALTER TABLE user_answers ADD CONSTRAINT user_answers_payload_check
      CHECK (
        (skipped = TRUE AND skip_reason IS NOT NULL AND value_unknown = FALSE
          AND num_nonnulls(value_boolean, value_numeric, value_text, selected_option_id) = 0)
        OR
        (skipped = FALSE AND skip_reason IS NULL AND (
          (value_unknown = TRUE AND num_nonnulls(value_boolean, value_numeric, value_text, selected_option_id) = 0)
          OR
          (value_unknown = FALSE AND num_nonnulls(value_boolean, value_numeric, value_text, selected_option_id) = 1)
        ))
      ) NOT VALID;

    ALTER TABLE user_answers DROP CONSTRAINT IF EXISTS user_answers_assignment_user_fkey;
    ALTER TABLE user_questionnaire_assignments DROP CONSTRAINT IF EXISTS assignments_id_user_unique;
    ALTER TABLE user_questionnaire_assignments ADD CONSTRAINT assignments_id_user_unique UNIQUE (id, user_id);
    ALTER TABLE user_answers ADD CONSTRAINT user_answers_assignment_user_fkey
      FOREIGN KEY (assignment_id, user_id)
      REFERENCES user_questionnaire_assignments(id, user_id)
      ON DELETE CASCADE NOT VALID;

    ALTER TABLE user_answers DROP CONSTRAINT IF EXISTS user_answers_option_question_fkey;
    ALTER TABLE answer_options DROP CONSTRAINT IF EXISTS answer_options_id_question_unique;
    ALTER TABLE answer_options ADD CONSTRAINT answer_options_id_question_unique UNIQUE (id, question_id);
    ALTER TABLE user_answers ADD CONSTRAINT user_answers_option_question_fkey
      FOREIGN KEY (selected_option_id, question_id)
      REFERENCES answer_options(id, question_id)
      ON DELETE RESTRICT NOT VALID;

    DO $$
    BEGIN
      IF NOT EXISTS (SELECT 1 FROM user_answers WHERE assignment_id IS NULL) THEN
        ALTER TABLE user_answers ALTER COLUMN assignment_id SET NOT NULL;
      END IF;
    END $$;

    CREATE OR REPLACE FUNCTION set_assignment_updated_at()
    RETURNS TRIGGER AS $$
    BEGIN
      NEW.updated_at = CURRENT_TIMESTAMP;
      RETURN NEW;
    END;
    $$ LANGUAGE plpgsql;

    DROP TRIGGER IF EXISTS trg_assignment_updated_at ON user_questionnaire_assignments;
    CREATE TRIGGER trg_assignment_updated_at
      BEFORE UPDATE ON user_questionnaire_assignments
      FOR EACH ROW EXECUTE FUNCTION set_assignment_updated_at();
  `);
}

async function seedQuestionnaire(client) {
  const systemIds = new Map();

  if (Array.isArray(data.retired_question_texts) && data.retired_question_texts.length > 0) {
    await client.query(
      'UPDATE questions SET active = FALSE WHERE question_text = ANY($1::text[])',
      [data.retired_question_texts]
    );
  }

  for (const system of data.body_systems || []) {
    const result = await client.query(
      `INSERT INTO body_systems (code, name)
       VALUES ($1, $2)
       ON CONFLICT (code) DO UPDATE SET name = EXCLUDED.name
       RETURNING id`,
      [system.code, system.name]
    );
    systemIds.set(system.code, result.rows[0].id);
  }

  for (const question of data.questions || []) {
    const systemId = systemIds.get(question.system_code);
    if (!systemId) continue;

    await client.query(
      `UPDATE questions SET code = $3
       WHERE system_id = $1 AND question_text = $2 AND code IS NULL`,
      [systemId, question.text, question.code]
    );

    const questionResult = await client.query(
      `INSERT INTO questions (
         system_id, code, question_text, question_type, gender_filter, required, active,
         allow_unknown, help_text, recall_period, display_condition, validation, alert, sort_order
       )
       VALUES ($1, $2, $3, $4, $5, $6, TRUE, $7, $8, $9, $10::jsonb, $11::jsonb, $12::jsonb, $13)
       ON CONFLICT (system_id, code) DO UPDATE SET
         question_text = EXCLUDED.question_text,
         question_type = EXCLUDED.question_type,
         gender_filter = EXCLUDED.gender_filter,
         required = EXCLUDED.required,
         active = TRUE,
         allow_unknown = EXCLUDED.allow_unknown,
         help_text = EXCLUDED.help_text,
         recall_period = EXCLUDED.recall_period,
         display_condition = EXCLUDED.display_condition,
         validation = EXCLUDED.validation,
         alert = EXCLUDED.alert,
         sort_order = EXCLUDED.sort_order
       RETURNING id`,
      [
        systemId,
        question.code,
        question.text,
        question.type,
        question.gender_filter ?? null,
        question.required !== false,
        question.allow_unknown === true,
        question.help_text ?? null,
        question.recall_period ?? null,
        question.display_condition ? JSON.stringify(question.display_condition) : null,
        question.validation ? JSON.stringify(question.validation) : null,
        question.alert ? JSON.stringify(question.alert) : null,
        question.sort_order ?? 0,
      ]
    );

    for (const option of question.options || []) {
      await client.query(
        `INSERT INTO answer_options (question_id, option_text, option_code, sort_order)
         VALUES ($1, $2, $3, $4)
         ON CONFLICT (question_id, option_code) DO UPDATE SET
           option_text = EXCLUDED.option_text,
           sort_order = EXCLUDED.sort_order`,
        [questionResult.rows[0].id, option.text, option.code, option.sort_order ?? 0]
      );
    }
  }

  for (const system of data.body_systems || []) {
    const systemId = systemIds.get(system.code);
    const currentCodes = (data.questions || [])
      .filter((question) => question.system_code === system.code)
      .map((question) => question.code)
      .filter(Boolean);

    await client.query(
      `UPDATE questions
       SET active = FALSE
       WHERE system_id = $1
         AND (code IS NULL OR NOT (code = ANY($2::text[])))`,
      [systemId, currentCodes]
    );
  }

  await client.query(
    `INSERT INTO questionnaire_releases (version, source_sha256, question_count, imported_at)
     VALUES ($1, $2, $3, NOW())
     ON CONFLICT (version) DO UPDATE SET
       source_sha256 = EXCLUDED.source_sha256,
       question_count = EXCLUDED.question_count,
       imported_at = EXCLUDED.imported_at`,
    [data.instrument_version || 'unversioned', questionnaireChecksum, (data.questions || []).length]
  );

  await client.query(
    `INSERT INTO schema_migrations (version, description)
     VALUES ($1, $2)
     ON CONFLICT (version) DO NOTHING`,
    [schemaVersion, 'Security hardening and unique refresh-token hashes']
  );
}

async function main() {
  const client = await pool.connect();

  try {
    await client.query('BEGIN');
    await createSchema(client);
    await seedQuestionnaire(client);
    await client.query('COMMIT');
    console.log('Database schema and questionnaire data are ready');
  } catch (error) {
    await client.query('ROLLBACK');
    console.error(error);
    process.exitCode = 1;
  } finally {
    client.release();
    await pool.end();
  }
}

main();
