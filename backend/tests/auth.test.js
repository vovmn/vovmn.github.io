const request = require('supertest');
const app = require('../src/app');    // ← исправлено
const pool = require('../src/db/pool');

// очищаем тестового пользователя после всех тестов
afterAll(async () => {
  await pool.query("DELETE FROM users WHERE username = 'testuser_qa'");
  await pool.end();
});

describe('Auth API', () => {
  let accessToken = '';

  test('POST /auth/register - successful registration with gender', async () => {
    const res = await request(app)
      .post('/auth/register')
      .send({
        username: 'testuser_qa',
        email: 'testuser_qa@example.com',
        password: 'Str0ngPass123',
        phone: '+79991234567',
        birth_date: '1990-05-15',
        residence: 'Moscow',
        gender: 'M',               // важное поле
      })
      .expect('Content-Type', /json/)
      .expect(201);

    expect(res.body).toHaveProperty('access_token');
    expect(res.body.user).toHaveProperty('id');
    expect(res.body.user.username).toBe('testuser_qa');
    expect(res.body.user.gender).toBe('M');
    accessToken = res.body.access_token;
  });

  test('POST /auth/login - successful login and token contains gender', async () => {
    const res = await request(app)
      .post('/auth/login')
      .send({ username: 'testuser_qa', password: 'Str0ngPass123' })
      .expect(200);

    expect(res.body).toHaveProperty('access_token');
    expect(res.body.user.gender).toBe('M');
    accessToken = res.body.access_token;
  });

  test('GET /auth/me - returns user data including gender', async () => {
    const res = await request(app)
      .get('/auth/me')
      .set('Authorization', `Bearer ${accessToken}`)
      .expect(200);

    expect(res.body.username).toBe('testuser_qa');
    expect(res.body.gender).toBe('M');
    expect(res.body).toHaveProperty('email');
  });

  test('POST /auth/register - rejects invalid gender', async () => {
    const res = await request(app)
      .post('/auth/register')
      .send({
        username: 'testuser2',
        email: 'test2@example.com',
        password: 'pass',
        gender: 'X',
      })
      .expect(400);

    expect(res.body.error).toMatch(/gender must be M or F/i);
  });
});