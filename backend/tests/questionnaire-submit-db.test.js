require('dotenv').config({ path: require('path').resolve(__dirname, '..', '.env') });

const request = require('supertest');
const argon2 = require('argon2');
const pool = require('../src/db/pool');

const username = `answers_test_${Date.now()}`;
const email = `${username}@example.com`;
const password = 'Str0ngPass123';
const testSystemCode = 'jest_answers_system';

let app;
let accessToken;
let userId;

async function ensureTestSchema() {
  await pool.query('CREATE EXTENSION IF NOT EXISTS "uuid-ossp"');

  await pool.query(`
    CREATE TABLE IF NOT EXISTS users (
      id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
      username VARCHAR(100) NOT NULL UNIQUE,
      email VARCHAR(255) UNIQUE,
      password_hash TEXT NOT NULL,
      role VARCHAR(50) NOT NULL DEFAULT 'user',
      phone VARCHAR(50),
      birth_date DATE,
      residence VARCHAR(255),
      gender CHAR(1) CHECK (gender IN ('M', 'F')),
      age SMALLINT,
      age_recorded_at DATE,
      created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    )
  `);

  await pool.query(`
    CREATE TABLE IF NOT EXISTS refresh_tokens (
      id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
      user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
      token_hash TEXT NOT NULL,
      expires_at TIMESTAMP NOT NULL,
      created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    )
  `);

  await pool.query(`
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
      priority VARCHAR(20) NOT NULL DEFAULT 'normal',
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
      answered_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    );
  `);

  await pool.query(`
    ALTER TABLE users ALTER COLUMN email DROP NOT NULL;
    ALTER TABLE users ADD COLUMN IF NOT EXISTS age SMALLINT;
    ALTER TABLE users ADD COLUMN IF NOT EXISTS age_recorded_at DATE;
    ALTER TABLE questions ADD COLUMN IF NOT EXISTS code VARCHAR(100);
    ALTER TABLE questions ADD COLUMN IF NOT EXISTS active BOOLEAN NOT NULL DEFAULT TRUE;
    ALTER TABLE questions ADD COLUMN IF NOT EXISTS allow_unknown BOOLEAN DEFAULT FALSE;
    ALTER TABLE questions ADD COLUMN IF NOT EXISTS help_text TEXT;
    ALTER TABLE questions ADD COLUMN IF NOT EXISTS recall_period VARCHAR(100);
    ALTER TABLE questions ADD COLUMN IF NOT EXISTS display_condition JSONB;
    ALTER TABLE questions ADD COLUMN IF NOT EXISTS validation JSONB;
    ALTER TABLE questions ADD COLUMN IF NOT EXISTS alert JSONB;
    ALTER TABLE user_answers ADD COLUMN IF NOT EXISTS value_unknown BOOLEAN NOT NULL DEFAULT FALSE;
    ALTER TABLE user_answers ADD COLUMN IF NOT EXISTS skipped BOOLEAN NOT NULL DEFAULT FALSE;
    ALTER TABLE user_answers ADD COLUMN IF NOT EXISTS skip_reason VARCHAR(50);
    ALTER TABLE user_questionnaire_assignments ADD COLUMN IF NOT EXISTS priority VARCHAR(20) NOT NULL DEFAULT 'normal';
    ALTER TABLE user_questionnaire_assignments ADD COLUMN IF NOT EXISTS due_at TIMESTAMP;
    ALTER TABLE user_questionnaire_assignments ADD COLUMN IF NOT EXISTS note TEXT;
    ALTER TABLE user_questionnaire_assignments ADD COLUMN IF NOT EXISTS started_at TIMESTAMP;
    ALTER TABLE user_questionnaire_assignments ADD COLUMN IF NOT EXISTS cancelled_at TIMESTAMP;
    ALTER TABLE user_questionnaire_assignments ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP;
  `);
}

async function seedTestQuestionnaire() {
  const systemResult = await pool.query(
    `INSERT INTO body_systems (code, name)
     VALUES ($1, $2)
     ON CONFLICT (code) DO UPDATE SET name = EXCLUDED.name
     RETURNING id`,
    [testSystemCode, 'Jest Answers System']
  );
  const systemId = systemResult.rows[0].id;

  const questionSpecs = [
    ['jest_boolean', 'Jest boolean question', 'boolean', 1],
    ['jest_numeric', 'Jest numeric question', 'numeric', 2],
    ['jest_text', 'Jest text question', 'text', 3],
    ['jest_single_choice', 'Jest single choice question', 'single_choice', 4],
    ['jest_multiple_choice', 'Jest multiple choice question', 'multiple_choice', 5],
  ];

  const questionIds = {};
  for (const [code, text, type, sortOrder] of questionSpecs) {
    const questionResult = await pool.query(
      `INSERT INTO questions (system_id, code, question_text, question_type, sort_order)
       VALUES ($1, $2, $3, $4, $5)
       ON CONFLICT (system_id, code) DO UPDATE SET
         question_text = EXCLUDED.question_text,
         question_type = EXCLUDED.question_type,
         sort_order = EXCLUDED.sort_order
       RETURNING id`,
      [systemId, code, text, type, sortOrder]
    );
    questionIds[type] = questionResult.rows[0].id;
  }

  for (const [questionType, options] of Object.entries({
    single_choice: [
      ['Single option A', 'single_a', 1],
      ['Single option B', 'single_b', 2],
    ],
    multiple_choice: [
      ['Multiple option A', 'multi_a', 1],
      ['Multiple option B', 'multi_b', 2],
    ],
  })) {
    for (const [text, code, sortOrder] of options) {
      await pool.query(
        `INSERT INTO answer_options (question_id, option_text, option_code, sort_order)
         VALUES ($1, $2, $3, $4)
         ON CONFLICT (question_id, option_code) DO UPDATE SET
           option_text = EXCLUDED.option_text,
           sort_order = EXCLUDED.sort_order`,
        [questionIds[questionType], text, code, sortOrder]
      );
    }
  }
}

function makeAnswer(question) {
  switch (question.question_type) {
    case 'boolean':
      return { question_id: question.id, value_boolean: false };
    case 'numeric':
      return { question_id: question.id, value_numeric: 0 };
    case 'single_choice':
      return { question_id: question.id, selected_option_id: question.options[0].id };
    case 'multiple_choice':
      return {
        question_id: question.id,
        selected_option_ids: question.options.map((option) => option.id),
      };
    case 'text':
      return { question_id: question.id, value_text: 'Jest test answer' };
    default:
      return null;
  }
}

function expectedRowsCount(answer) {
  return Array.isArray(answer.selected_option_ids) ? answer.selected_option_ids.length : 1;
}

beforeAll(async () => {
  await ensureTestSchema();
  await seedTestQuestionnaire();
  app = require('../src/app');
});

afterAll(async () => {
  if (userId) {
    await pool.query('DELETE FROM users WHERE id = $1', [userId]);
  }
  await pool.query('DELETE FROM body_systems WHERE code = $1', [testSystemCode]);
  await pool.end();
});

describe('Questionnaire answers persistence', () => {
  test('logs in as a provisioned user, submits answers and stores them in DB', async () => {
    const passwordHash = await argon2.hash(password);
    const created = await pool.query(
      `INSERT INTO users (username, email, password_hash, role, gender, age, age_recorded_at)
       VALUES ($1, $2, $3, 'user', 'F', 30, CURRENT_DATE)
       RETURNING id`,
      [username, email, passwordHash]
    );
    userId = created.rows[0].id;

    const loginRes = await request(app)
      .post('/auth/login')
      .send({ username, password })
      .expect(200);

    accessToken = loginRes.body.access_token;
    expect(accessToken).toBeTruthy();

    const systemResult = await pool.query(
      'SELECT id FROM body_systems WHERE code = $1',
      [testSystemCode]
    );
    const assignmentResult = await pool.query(
      `INSERT INTO user_questionnaire_assignments (user_id, system_id, status)
       VALUES ($1, $2, 'pending') RETURNING id`,
      [userId, systemResult.rows[0].id]
    );
    const assignmentId = assignmentResult.rows[0].id;

    const startRes = await request(app)
      .post(`/api/questionnaire/assignments/${assignmentId}/start`)
      .set('Authorization', `Bearer ${accessToken}`)
      .expect(200);

    expect(startRes.body.assignment_id).toBeTruthy();
    expect(startRes.body.questions).toHaveLength(5);

    const answers = startRes.body.questions.map(makeAnswer).filter(Boolean);

    await request(app)
      .post(`/api/questionnaire/submit/${startRes.body.assignment_id}`)
      .set('Authorization', `Bearer ${accessToken}`)
      .send({ answers })
      .expect(200)
      .expect(({ body }) => {
        expect(body).toEqual({ status: 'ok' });
      });

    const expectedRowCount = answers.reduce(
      (sum, answer) => sum + expectedRowsCount(answer),
      0
    );

    const savedAnswers = await pool.query(
      `SELECT question_id::text,
              assignment_id::text,
              value_boolean,
              value_numeric,
              value_text,
              selected_option_id::text
       FROM user_answers
       WHERE user_id = $1 AND assignment_id = $2
       ORDER BY question_id::text, selected_option_id::text NULLS FIRST`,
      [userId, startRes.body.assignment_id]
    );

    expect(savedAnswers.rows).toHaveLength(expectedRowCount);

    for (const answer of answers) {
      const rowsForQuestion = savedAnswers.rows.filter(
        (row) => row.question_id === String(answer.question_id)
      );

      if (Array.isArray(answer.selected_option_ids)) {
        expect(rowsForQuestion.map((row) => row.selected_option_id).sort()).toEqual(
          answer.selected_option_ids.map(String).sort()
        );
        continue;
      }

      expect(rowsForQuestion).toHaveLength(1);
      const row = rowsForQuestion[0];

      if (Object.prototype.hasOwnProperty.call(answer, 'value_boolean')) {
        expect(row.value_boolean).toBe(answer.value_boolean);
      }
      if (Object.prototype.hasOwnProperty.call(answer, 'value_numeric')) {
        expect(Number(row.value_numeric)).toBe(answer.value_numeric);
      }
      if (Object.prototype.hasOwnProperty.call(answer, 'value_text')) {
        expect(row.value_text).toBe(answer.value_text);
      }
      if (Object.prototype.hasOwnProperty.call(answer, 'selected_option_id')) {
        expect(row.selected_option_id).toBe(String(answer.selected_option_id));
      }
    }

    const assignment = await pool.query(
      `SELECT status, completed_at
       FROM user_questionnaire_assignments
       WHERE id = $1 AND user_id = $2`,
      [startRes.body.assignment_id, userId]
    );

    expect(assignment.rows).toHaveLength(1);
    expect(assignment.rows[0].status).toBe('completed');
    expect(assignment.rows[0].completed_at).toBeTruthy();
  });
});
