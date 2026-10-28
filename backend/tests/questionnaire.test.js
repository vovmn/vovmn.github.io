const request = require('supertest');
const app = require('../src/app');    // ← исправлено
const pool = require('../src/db/pool');

let accessToken;
let assignmentId;
let nervousAssignmentId;
let reproductiveAssignmentId;

beforeAll(async () => {
  // Регистрируем нового юзера с gender=F, чтобы проверить фильтрацию по полу
  await request(app)
    .post('/auth/register')
    .send({
      username: 'qtest_female',
      email: 'qtest_f@example.com',
      password: 'QuestionnairePass123',
      gender: 'F',
    });

  const loginRes = await request(app)
    .post('/auth/login')
    .send({ username: 'qtest_female', password: 'QuestionnairePass123' });

  accessToken = loginRes.body.access_token;

  const user = await pool.query("SELECT id FROM users WHERE username = 'qtest_female'");
  const system = await pool.query("SELECT id FROM body_systems WHERE code = 'nervous'");
  const assignment = await pool.query(
    `INSERT INTO user_questionnaire_assignments (user_id, system_id, status)
     VALUES ($1, $2, 'pending') RETURNING id`,
    [user.rows[0].id, system.rows[0].id]
  );
  nervousAssignmentId = assignment.rows[0].id;
  const reproductive = await pool.query("SELECT id FROM body_systems WHERE code = 'reproductive'");
  const reproductiveAssignment = await pool.query(
    `INSERT INTO user_questionnaire_assignments (user_id, system_id, status)
     VALUES ($1, $2, 'pending') RETURNING id`,
    [user.rows[0].id, reproductive.rows[0].id]
  );
  reproductiveAssignmentId = reproductiveAssignment.rows[0].id;
});

afterAll(async () => {
  await pool.query("DELETE FROM users WHERE username IN ('qtest_female')");
  await pool.end();
});

describe('Questionnaire API', () => {
  test('GET /api/questionnaire/systems - returns systems list', async () => {
    const res = await request(app)
      .get('/api/questionnaire/systems')
      .set('Authorization', `Bearer ${accessToken}`)
      .expect(200);

    expect(Array.isArray(res.body)).toBe(true);
    expect(res.body.length).toBeGreaterThan(0);
    expect(res.body[0]).toHaveProperty('code');
    expect(res.body[0]).toHaveProperty('name');
    // например, должна быть 'nervous'
    const codes = res.body.map(s => s.code);
    expect(codes).toContain('nervous');
  });

  test('POST /api/questionnaire/start/nervous - starts questionnaire', async () => {
    const res = await request(app)
      .post(`/api/questionnaire/assignments/${nervousAssignmentId}/start`)
      .set('Authorization', `Bearer ${accessToken}`)
      .expect(200);

    expect(res.body).toHaveProperty('assignment_id');
    expect(res.body).toHaveProperty('system_name', 'Нервная система');
    expect(res.body.questions.length).toBeGreaterThan(0);

    // убедимся, что вопросы для женщин не включают мужские фильтрованные
    // В нервной системе все вопросы без фильтра, поэтому они будут
    // Но для репродуктивной системы с gender_filter='M' они не должны появляться (проверим отдельно)
    assignmentId = res.body.assignment_id;
  });

  test('POST /api/questionnaire/submit/:id - submits answers', async () => {
    // Backend требует ответы на все видимые обязательные вопросы.
    const startRes = await request(app)
      .post(`/api/questionnaire/assignments/${nervousAssignmentId}/start`)
      .set('Authorization', `Bearer ${accessToken}`);

    const answers = startRes.body.questions
      .filter((question) => !question.display_condition)
      .map((question) => ({ question_id: question.id, value_boolean: false }));

    const submitRes = await request(app)
      .post(`/api/questionnaire/submit/${startRes.body.assignment_id}`)
      .set('Authorization', `Bearer ${accessToken}`)
      .send({ answers })
      .expect(200);

    expect(submitRes.body).toEqual({ status: 'ok' });
  });

  test('POST /api/questionnaire/start/reproductive - filters by gender (female gets female questions only)', async () => {
    const res = await request(app)
      .post(`/api/questionnaire/assignments/${reproductiveAssignmentId}/start`)
      .set('Authorization', `Bearer ${accessToken}`)
      .expect(200);

    // Убедимся, что там нет мужских вопросов (gender_filter='M')
    const questionTexts = res.body.questions.map(q => q.question_text);
    const maleIndicators = questionTexts.some(t => t.includes('Аденома предстательной') || t.includes('Простатит'));
    expect(maleIndicators).toBe(false);
  });
});
