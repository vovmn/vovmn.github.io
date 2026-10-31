const express = require('express');
const request = require('supertest');

jest.mock('../src/modules/auth/auth.service', () => ({
  login: jest.fn(async () => ({
    accessToken: 'access-token',
    refreshToken: 'refresh-token',
    user: {
      id: '00000000-0000-4000-8000-000000000001',
      username: 'Administrator',
      email: null,
      role: 'admin',
      gender: null,
      age: null,
      age_recorded_at: null,
      antigens: [],
    },
  })),
  rotateRefreshToken: jest.fn(async () => ({
    accessToken: 'rotated-access-token',
    refreshToken: 'rotated-refresh-token',
  })),
  revokeRefreshToken: jest.fn(async () => {}),
}));

const controller = require('../src/modules/auth/auth.controller');

function makeApp() {
  const app = express();
  app.set('trust proxy', 1);
  app.use(express.json());
  app.post('/auth/login', controller.login);
  app.post('/auth/refresh', controller.refresh);
  return app;
}

describe('refresh cookie transport', () => {
  test('allows a refresh cookie on the current HTTP FRP endpoint', async () => {
    const response = await request(makeApp())
      .post('/auth/login')
      .send({ username: 'Administrator', password: 'test-password' })
      .expect(200);

    const cookie = response.headers['set-cookie'][0];
    expect(cookie).toContain('HttpOnly');
    expect(cookie).toContain('SameSite=Lax');
    expect(cookie).not.toContain('Secure');
  });

  test('keeps the refresh cookie secure when HTTPS is forwarded', async () => {
    const response = await request(makeApp())
      .post('/auth/login')
      .set('X-Forwarded-Proto', 'https')
      .send({ username: 'Administrator', password: 'test-password' })
      .expect(200);

    expect(response.headers['set-cookie'][0]).toContain('Secure');
  });
});
