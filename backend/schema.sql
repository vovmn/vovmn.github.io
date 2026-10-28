-- Canonical Immunolab PostgreSQL schema.
-- Runtime upgrades and questionnaire seeding are performed by scripts/init-db.js.

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE schema_migrations (
  version VARCHAR(50) PRIMARY KEY,
  description TEXT NOT NULL,
  applied_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE users (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  username VARCHAR(100) NOT NULL UNIQUE,
  email VARCHAR(255) NOT NULL UNIQUE,
  password_hash TEXT NOT NULL,
  role VARCHAR(50) NOT NULL DEFAULT 'user' CHECK (role IN ('user','admin')),
  phone VARCHAR(50),
  birth_date DATE,
  residence VARCHAR(255),
  gender CHAR(1) CHECK (gender IN ('M','F')),
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE refresh_tokens (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  token_hash TEXT NOT NULL,
  expires_at TIMESTAMP NOT NULL,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE body_systems (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  name VARCHAR(100) NOT NULL,
  code VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE questionnaire_releases (
  version VARCHAR(50) PRIMARY KEY,
  source_sha256 CHAR(64) NOT NULL,
  question_count INTEGER NOT NULL CHECK (question_count >= 0),
  imported_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE questions (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  system_id UUID NOT NULL REFERENCES body_systems(id) ON DELETE CASCADE,
  code VARCHAR(100),
  question_text TEXT NOT NULL,
  question_type VARCHAR(20) NOT NULL
    CHECK (question_type IN ('boolean','numeric','single_choice','multiple_choice','text')),
  gender_filter CHAR(1) CHECK (gender_filter IN ('M','F')),
  required BOOLEAN NOT NULL DEFAULT TRUE,
  active BOOLEAN NOT NULL DEFAULT TRUE,
  allow_unknown BOOLEAN NOT NULL DEFAULT FALSE,
  help_text TEXT,
  recall_period VARCHAR(100),
  display_condition JSONB,
  validation JSONB,
  alert JSONB,
  sort_order INTEGER NOT NULL DEFAULT 0,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT questions_system_code_unique UNIQUE (system_id, code),
  CONSTRAINT questions_active_code_check CHECK (NOT active OR code IS NOT NULL)
);

CREATE TABLE answer_options (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  question_id UUID NOT NULL REFERENCES questions(id) ON DELETE CASCADE,
  option_text TEXT NOT NULL,
  option_code VARCHAR(50) NOT NULL,
  sort_order INTEGER NOT NULL DEFAULT 0,
  CONSTRAINT answer_options_question_code_unique UNIQUE (question_id, option_code),
  CONSTRAINT answer_options_id_question_unique UNIQUE (id, question_id)
);

CREATE TABLE user_questionnaire_assignments (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  system_id UUID NOT NULL REFERENCES body_systems(id),
  assigned_by UUID REFERENCES users(id) ON DELETE SET NULL,
  status VARCHAR(20) NOT NULL DEFAULT 'pending'
    CHECK (status IN ('pending','in_progress','completed','cancelled')),
  priority VARCHAR(20) NOT NULL DEFAULT 'normal'
    CHECK (priority IN ('low','normal','high','urgent')),
  due_at TIMESTAMP,
  note TEXT,
  questionnaire_version VARCHAR(50),
  assigned_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  started_at TIMESTAMP,
  completed_at TIMESTAMP,
  cancelled_at TIMESTAMP,
  updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT assignments_id_user_unique UNIQUE (id, user_id),
  CONSTRAINT assignments_lifecycle_check CHECK (
    (status = 'pending' AND completed_at IS NULL AND cancelled_at IS NULL)
    OR (status = 'in_progress' AND started_at IS NOT NULL AND completed_at IS NULL AND cancelled_at IS NULL)
    OR (status = 'completed' AND completed_at IS NOT NULL AND cancelled_at IS NULL)
    OR (status = 'cancelled' AND cancelled_at IS NOT NULL)
  )
);

CREATE TABLE user_answers (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  question_id UUID NOT NULL REFERENCES questions(id) ON DELETE CASCADE,
  assignment_id UUID NOT NULL,
  value_boolean BOOLEAN,
  value_numeric DOUBLE PRECISION,
  value_text TEXT,
  value_unknown BOOLEAN NOT NULL DEFAULT FALSE,
  skipped BOOLEAN NOT NULL DEFAULT FALSE,
  skip_reason VARCHAR(50)
    CHECK (skip_reason IS NULL OR skip_reason IN ('unknown','not_applicable','prefer_not_to_answer','other')),
  selected_option_id UUID,
  answered_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT user_answers_assignment_user_fkey
    FOREIGN KEY (assignment_id, user_id)
    REFERENCES user_questionnaire_assignments(id, user_id) ON DELETE CASCADE,
  CONSTRAINT user_answers_option_question_fkey
    FOREIGN KEY (selected_option_id, question_id)
    REFERENCES answer_options(id, question_id) ON DELETE RESTRICT,
  CONSTRAINT user_answers_payload_check CHECK (
    (skipped = TRUE AND skip_reason IS NOT NULL AND value_unknown = FALSE
      AND num_nonnulls(value_boolean, value_numeric, value_text, selected_option_id) = 0)
    OR
    (skipped = FALSE AND skip_reason IS NULL AND (
      (value_unknown = TRUE
        AND num_nonnulls(value_boolean, value_numeric, value_text, selected_option_id) = 0)
      OR
      (value_unknown = FALSE
        AND num_nonnulls(value_boolean, value_numeric, value_text, selected_option_id) = 1)
    ))
  )
);

CREATE TABLE appointments (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  scheduled_at TIMESTAMP NOT NULL,
  specialist VARCHAR(255),
  doctor VARCHAR(255),
  status VARCHAR(20) NOT NULL DEFAULT 'scheduled'
    CHECK (status IN ('scheduled','completed','cancelled')),
  comment TEXT,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE OR REPLACE FUNCTION set_assignment_updated_at()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at = CURRENT_TIMESTAMP;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_assignment_updated_at
  BEFORE UPDATE ON user_questionnaire_assignments
  FOR EACH ROW EXECUTE FUNCTION set_assignment_updated_at();

CREATE INDEX idx_refresh_tokens_user_expires ON refresh_tokens(user_id, expires_at);
CREATE UNIQUE INDEX uq_refresh_tokens_hash ON refresh_tokens(token_hash);
CREATE INDEX idx_questions_system ON questions(system_id);
CREATE INDEX idx_questions_active_flow
  ON questions(system_id, gender_filter, sort_order) WHERE active;
CREATE INDEX idx_answer_options_question ON answer_options(question_id);
CREATE INDEX idx_assignments_user ON user_questionnaire_assignments(user_id);
CREATE INDEX idx_assignments_system ON user_questionnaire_assignments(system_id);
CREATE INDEX idx_assignments_status_due ON user_questionnaire_assignments(status, due_at);
CREATE INDEX idx_assignments_user_status_due
  ON user_questionnaire_assignments(user_id, status, due_at);
CREATE INDEX idx_user_answers_user ON user_answers(user_id);
CREATE INDEX idx_user_answers_question ON user_answers(question_id);
CREATE INDEX idx_user_answers_assignment ON user_answers(assignment_id);
CREATE UNIQUE INDEX uq_answers_assignment_question_option
  ON user_answers(assignment_id, question_id, selected_option_id) NULLS NOT DISTINCT;
CREATE INDEX idx_appointments_user ON appointments(user_id);
CREATE INDEX idx_appointments_scheduled_at ON appointments(scheduled_at);
CREATE INDEX idx_appointments_status_scheduled ON appointments(status, scheduled_at);

INSERT INTO schema_migrations (version, description)
VALUES (
  '20261028_02',
  'Security hardening and unique refresh-token hashes'
);
