const fs = require('fs');
const path = require('path');
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

const data = JSON.parse(fs.readFileSync(questionnairePath, 'utf8'));
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
      question_text TEXT NOT NULL,
      question_type VARCHAR(20) NOT NULL CHECK (question_type IN ('boolean','numeric','single_choice','multiple_choice','text')),
      gender_filter CHAR(1) DEFAULT NULL CHECK (gender_filter IN ('M','F', NULL)),
      required BOOLEAN DEFAULT TRUE,
      sort_order INTEGER DEFAULT 0,
      created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
      UNIQUE (system_id, question_text)
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
      status VARCHAR(20) DEFAULT 'pending' CHECK (status IN ('pending','in_progress','completed')),
      assigned_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
      completed_at TIMESTAMP,
      UNIQUE (user_id, system_id)
    );

    CREATE TABLE IF NOT EXISTS user_answers (
      id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
      user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
      question_id UUID NOT NULL REFERENCES questions(id) ON DELETE CASCADE,
      assignment_id UUID REFERENCES user_questionnaire_assignments(id),
      value_boolean BOOLEAN,
      value_numeric DOUBLE PRECISION,
      value_text TEXT,
      selected_option_id UUID REFERENCES answer_options(id),
      answered_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
      UNIQUE (user_id, question_id, selected_option_id)
    );

    CREATE INDEX IF NOT EXISTS idx_questions_system ON questions(system_id);
    CREATE INDEX IF NOT EXISTS idx_answer_options_question ON answer_options(question_id);
    CREATE INDEX IF NOT EXISTS idx_assignments_user ON user_questionnaire_assignments(user_id);
    CREATE INDEX IF NOT EXISTS idx_assignments_system ON user_questionnaire_assignments(system_id);
    CREATE INDEX IF NOT EXISTS idx_user_answers_user ON user_answers(user_id);
    CREATE INDEX IF NOT EXISTS idx_user_answers_question ON user_answers(question_id);
    CREATE INDEX IF NOT EXISTS idx_user_answers_assignment ON user_answers(assignment_id);
  `);
}

async function seedQuestionnaire(client) {
  const systemIds = new Map();

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

    const questionResult = await client.query(
      `INSERT INTO questions (system_id, question_text, question_type, gender_filter, sort_order)
       VALUES ($1, $2, $3, $4, $5)
       ON CONFLICT (system_id, question_text) DO UPDATE SET
         question_type = EXCLUDED.question_type,
         gender_filter = EXCLUDED.gender_filter,
         sort_order = EXCLUDED.sort_order
       RETURNING id`,
      [systemId, question.text, question.type, question.gender_filter ?? null, question.sort_order ?? 0]
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
