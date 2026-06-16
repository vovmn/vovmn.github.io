const request = require('supertest');
const app = require('../src/app');    // ← исправлено
const pool = require('../src/db/pool');

let accessToken;
let assignmentId;

beforeAll(async () => {
  // Регистрируем нового юзера с gender=F, чтобы проверить фильтрацию по полу
  await request(app)
    .post('/auth/register')
    .send({
      username: 'qtest_female',
      email: 'qtest_f@example.com',
      password: 'pass123456',
      gender: 'F',
    });

  const loginRes = await request(app)
    .post('/auth/login')
    .send({ username: 'qtest_female', password: 'pass123456' });

  accessToken = loginRes.body.access_token;
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
      .post('/api/questionnaire/start/nervous')
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
    // Для теста достаточно отправить ответ на первый boolean-вопрос
    const answers = [
      {
        question_id: '',   // заполним динамически
        value_boolean: true,
      },
    ];

    // Получим вопросы из стартового запроса (не будем повторно дёргать, возьмём assignment)
    const startRes = await request(app)
      .post('/api/questionnaire/start/nervous')
      .set('Authorization', `Bearer ${accessToken}`);

    const firstBoolQ = startRes.body.questions.find(q => q.question_type === 'boolean');
    if (!firstBoolQ) {
      console.warn('Нет boolean вопроса, пропускаем submit тест');
      return;
    }

    answers[0].question_id = firstBoolQ.id;

    const submitRes = await request(app)
      .post(`/api/questionnaire/submit/${startRes.body.assignment_id}`)
      .set('Authorization', `Bearer ${accessToken}`)
      .send({ answers })
      .expect(200);

    expect(submitRes.body).toEqual({ status: 'ok' });
  });

  test('POST /api/questionnaire/start/reproductive - filters by gender (female gets female questions only)', async () => {
    const res = await request(app)
      .post('/api/questionnaire/start/reproductive')
      .set('Authorization', `Bearer ${accessToken}`)
      .expect(200);

    // Убедимся, что там нет мужских вопросов (gender_filter='M')
    const questionTexts = res.body.questions.map(q => q.question_text);
    const maleIndicators = questionTexts.some(t => t.includes('Аденома предстательной') || t.includes('Простатит'));
    expect(maleIndicators).toBe(false);
  });
});