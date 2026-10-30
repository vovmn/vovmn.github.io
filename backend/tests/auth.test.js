const argon2 = require('argon2');
const request = require('supertest');
const app = require('../src/app');
const pool = require('../src/db/pool');

const adminUsername = 'admin_user_management_qa';
const patientUsername = 'patient_user_management_qa';
const injectionUsername = "patient'); DROP TABLE users;--";
const secondPatientUsername = 'patient_antigen_conflict_qa';
const antigenName = 'HLA-B27-QA';
const secondAntigenName = 'HLA-DR4-QA';
const password = 'Str0ngPass123';

let adminToken = '';
let patientToken = '';

beforeAll(async () => {
  const passwordHash = await argon2.hash(password);
  await pool.query('DELETE FROM users WHERE username = ANY($1::text[])', [[adminUsername, patientUsername, injectionUsername, secondPatientUsername]]);
  await pool.query('DELETE FROM antigens WHERE name = ANY($1::text[])', [[antigenName, secondAntigenName]]);
  await pool.query(
    `INSERT INTO users (username, email, password_hash, role, gender)
     VALUES ($1, NULL, $2, 'admin', 'F')`,
    [adminUsername, passwordHash]
  );

  const loginRes = await request(app).post('/auth/login').send({ username: adminUsername, password }).expect(200);
  adminToken = loginRes.body.access_token;
});

afterAll(async () => {
  await pool.query('DELETE FROM users WHERE username = ANY($1::text[])', [[adminUsername, patientUsername, injectionUsername, secondPatientUsername]]);
  await pool.query('DELETE FROM antigens WHERE name = ANY($1::text[])', [[antigenName, secondAntigenName]]);
  await pool.end();
});

describe('Admin-managed users and authentication', () => {
  test('public registration endpoint is unavailable', async () => {
    await request(app).post('/auth/register').send({ username: 'blocked', password, age: 30, gender: 'M' }).expect(404);
    await request(app).post('/api/auth/register').send({ username: 'blocked', password, age: 30, gender: 'M' }).expect(404);
  });

  test('creating a user requires authentication', async () => {
    await request(app).post('/api/admin/users').send({ username: patientUsername, password, age: 31, gender: 'F' }).expect(401);
  });

  test('admin creates a patient with only the four required fields', async () => {
    const res = await request(app)
      .post('/api/admin/users')
      .set('Authorization', `Bearer ${adminToken}`)
      .send({ username: patientUsername, password, age: 31, gender: 'F' })
      .expect('Content-Type', /json/)
      .expect(201);

    expect(res.body).toMatchObject({ username: patientUsername, email: null, age: 31, gender: 'F', role: 'user' });
    expect(res.body).not.toHaveProperty('password_hash');
    expect(res.body).not.toHaveProperty('access_token');
  });

  test('non-admin cannot create another user', async () => {
    const loginRes = await request(app).post('/auth/login').send({ username: patientUsername, password }).expect(200);
    patientToken = loginRes.body.access_token;

    await request(app)
      .post('/api/admin/users')
      .set('Authorization', `Bearer ${patientToken}`)
      .send({ username: 'forbidden_patient', password, age: 25, gender: 'M' })
      .expect(403);
  });

  test('age must match an optional birth date', async () => {
    const res = await request(app)
      .post('/api/admin/users')
      .set('Authorization', `Bearer ${adminToken}`)
      .send({ username: 'contradictory_patient', password, age: 20, gender: 'F', birth_date: '1950-01-01' })
      .expect(400);
    expect(res.body.error).toMatch(/age does not match/i);
  });

  test('admin can remediate patient demographics', async () => {
    const patient = await pool.query('SELECT id FROM users WHERE username = $1', [patientUsername]);
    const res = await request(app)
      .patch(`/api/admin/users/${patient.rows[0].id}/demographics`)
      .set('Authorization', `Bearer ${adminToken}`)
      .send({ age: 32, gender: 'F' })
      .expect(200);
    expect(res.body).toMatchObject({ age: 32, gender: 'F', profile_complete: true });
  });

  test('admin assigns multiple antigens and shares one antigen between patients', async () => {
    const antigenRes = await request(app)
      .post('/api/admin/antigens')
      .set('Authorization', `Bearer ${adminToken}`)
      .send({ name: antigenName })
      .expect(201);

    expect(antigenRes.body).toMatchObject({ name: antigenName, assigned_user_count: 0, assigned_users: [] });
    expect(antigenRes.body.id).toMatch(/^[0-9a-f-]{36}$/i);
    const secondAntigenRes = await request(app)
      .post('/api/admin/antigens')
      .set('Authorization', `Bearer ${adminToken}`)
      .send({ name: secondAntigenName })
      .expect(201);

    const patient = await pool.query('SELECT id FROM users WHERE username = $1', [patientUsername]);
    const assigned = await request(app)
      .patch(`/api/admin/users/${patient.rows[0].id}/antigens`)
      .set('Authorization', `Bearer ${adminToken}`)
      .send({ antigen_ids: [antigenRes.body.id, secondAntigenRes.body.id] })
      .expect(200);
    expect(assigned.body.antigen_ids).toEqual(expect.arrayContaining([antigenRes.body.id, secondAntigenRes.body.id]));
    expect(assigned.body.antigens).toHaveLength(2);

    const secondPatient = await request(app)
      .post('/api/admin/users')
      .set('Authorization', `Bearer ${adminToken}`)
      .send({ username: secondPatientUsername, password, age: 28, gender: 'M' })
      .expect(201);
    await request(app)
      .patch(`/api/admin/users/${secondPatient.body.id}/antigens`)
      .set('Authorization', `Bearer ${adminToken}`)
      .send({ antigen_ids: [antigenRes.body.id] })
      .expect(200);

    const users = await request(app)
      .get('/api/admin/users')
      .set('Authorization', `Bearer ${adminToken}`)
      .expect(200);
    expect(users.body.find((user) => user.id === patient.rows[0].id)).toMatchObject({
      antigen_ids: expect.arrayContaining([antigenRes.body.id, secondAntigenRes.body.id]),
    });
    expect(users.body.find((user) => user.id === secondPatient.body.id).antigen_ids).toContain(antigenRes.body.id);
  });

  test('login SQL injection payload is treated as a literal username', async () => {
    const res = await request(app)
      .post('/auth/login')
      .send({ username: "' OR '1'='1' --", password: 'arbitrary-password' })
      .expect(401);
    expect(res.body).not.toHaveProperty('access_token');
  });

  test('SQL-like username is stored literally without damaging the users table', async () => {
    await request(app)
      .post('/api/admin/users')
      .set('Authorization', `Bearer ${adminToken}`)
      .send({ username: injectionUsername, password, age: 44, gender: 'M' })
      .expect(201);

    await request(app).post('/auth/login').send({ username: injectionUsername, password }).expect(200);
    const table = await pool.query("SELECT to_regclass('public.users') AS table_name");
    expect(table.rows[0].table_name).toBe('users');
  });

  test('invalid UUID injection payloads are rejected as client errors', async () => {
    const headers = { Authorization: `Bearer ${adminToken}` };
    await request(app)
      .post('/api/admin/questionnaire-assignments')
      .set(headers)
      .send({ user_id: "' OR '1'='1", system_ids: ["x'); DROP TABLE users;--"] })
      .expect(400);
    await request(app).patch('/api/admin/questionnaire-assignments/not-a-uuid/cancel').set(headers).expect(400);
    await request(app).patch('/api/admin/users/not-a-uuid/antigens').set(headers).send({ antigen_ids: [] }).expect(400);
    await request(app)
      .post('/api/admin/appointments')
      .set(headers)
      .send({ user_id: "' OR '1'='1", scheduled_at: '2030-01-01T10:00:00Z' })
      .expect(400);
  });

  test('current user includes age and refresh token rotates once', async () => {
    const meRes = await request(app).get('/auth/me').set('Authorization', `Bearer ${patientToken}`).expect(200);
    expect(meRes.body).toMatchObject({ username: patientUsername, age: 32, gender: 'F' });
    expect(meRes.body.antigens).toHaveLength(2);

    const loginRes = await request(app).post('/auth/login').send({ username: patientUsername, password }).expect(200);
    const firstCookie = loginRes.headers['set-cookie'][0].split(';')[0];
    const refreshRes = await request(app).post('/auth/refresh').set('Cookie', firstCookie).expect(200);
    const rotatedCookie = refreshRes.headers['set-cookie'][0].split(';')[0];

    await request(app).post('/auth/refresh').set('Cookie', firstCookie).expect(401);
    await request(app).post('/auth/logout').set('Cookie', rotatedCookie).expect(200);
    await request(app).post('/auth/refresh').set('Cookie', rotatedCookie).expect(401);
  });
});
