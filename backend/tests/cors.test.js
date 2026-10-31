const request = require('supertest');
const app = require('../src/app');
const pool = require('../src/db/pool');

afterAll(async () => {
  await pool.end();
});

describe('CORS policy', () => {
  test('allows the public origin served by the reverse proxy', async () => {
    await request(app)
      .options('/auth/login')
      .set('Host', 'clinic.example')
      .set('X-Forwarded-Host', 'clinic.example')
      .set('X-Forwarded-Proto', 'https')
      .set('Origin', 'https://clinic.example')
      .set('Access-Control-Request-Method', 'POST')
      .expect('Access-Control-Allow-Origin', 'https://clinic.example')
      .expect(204);
  });

  test('rejects an unrelated web origin', async () => {
    const response = await request(app)
      .options('/auth/login')
      .set('Host', 'clinic.example')
      .set('X-Forwarded-Host', 'clinic.example')
      .set('X-Forwarded-Proto', 'https')
      .set('Origin', 'https://evil.example')
      .set('Access-Control-Request-Method', 'POST')
      .expect(403);

    expect(response.body.error).toBe('CORS origin denied');
  });
});
