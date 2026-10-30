const pool = require('../../db/pool');

async function findUserByUsername(username) {
  const { rows } = await pool.query(
    `SELECT u.id, u.username, u.email, u.password_hash, u.role, u.gender, u.age,
            u.age_recorded_at,
            COALESCE((
              SELECT json_agg(json_build_object('id', antigen.id, 'name', antigen.name) ORDER BY antigen.name)
              FROM user_antigens assignment
              JOIN antigens antigen ON antigen.id = assignment.antigen_id
              WHERE assignment.user_id = u.id
            ), '[]'::json) AS antigens
     FROM users u
     WHERE u.username = $1`,
    [username]
  );
  return rows[0] || null;
}

async function createUser({ username, email = null, passwordHash, phone = null, birthDate = null, residence = null, gender, age }) {
  const { rows } = await pool.query(
    `INSERT INTO users (
       username, email, password_hash, role, phone, birth_date, residence, gender, age, age_recorded_at
     )
     VALUES ($1, $2, $3, 'user', $4, $5, $6, $7, $8, CURRENT_DATE)
     RETURNING id, username, email, role, phone, birth_date, residence, gender, age, age_recorded_at`,
    [username, email, passwordHash, phone, birthDate, residence, gender, age]
  );
  return rows[0] || null;
}

async function findUserById(id) {
  const { rows } = await pool.query(
    `SELECT u.id, u.username, u.email, u.role, u.phone, u.birth_date, u.residence,
            u.gender, u.age, u.age_recorded_at,
            COALESCE((
              SELECT json_agg(json_build_object('id', antigen.id, 'name', antigen.name) ORDER BY antigen.name)
              FROM user_antigens assignment
              JOIN antigens antigen ON antigen.id = assignment.antigen_id
              WHERE assignment.user_id = u.id
            ), '[]'::json) AS antigens
     FROM users u
     WHERE u.id = $1`,
    [id]
  );
  return rows[0] || null;
}

async function saveRefreshSession(userId, tokenHash, expiresAt) {
  const { rows } = await pool.query(
    `INSERT INTO refresh_tokens (user_id, token_hash, expires_at)
     VALUES ($1, $2, $3)
     RETURNING id`,
    [userId, tokenHash, expiresAt]
  );
  return rows[0] || null;
}

async function findRefreshSession(tokenHash) {
  const { rows } = await pool.query(
    `SELECT id, user_id, expires_at
     FROM refresh_tokens
     WHERE token_hash = $1 AND expires_at > now()`,
    [tokenHash]
  );
  return rows[0] || null;
}

async function consumeRefreshSession(tokenHash) {
  const { rows } = await pool.query(
    `DELETE FROM refresh_tokens
     WHERE token_hash = $1 AND expires_at > now()
     RETURNING id, user_id, expires_at`,
    [tokenHash]
  );
  return rows[0] || null;
}

async function deleteRefreshSession(tokenHash) {
  await pool.query('DELETE FROM refresh_tokens WHERE token_hash = $1', [tokenHash]);
}

module.exports = {
  findUserByUsername,
  findUserById,
  createUser,
  saveRefreshSession,
  findRefreshSession,
  consumeRefreshSession,
  deleteRefreshSession,
};
