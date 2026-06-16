const pool = require('../../db/pool');

async function findUserByUsername(username) {
  const { rows } = await pool.query(
    `SELECT id, username, email, password_hash, role, gender
     FROM users
     WHERE username = $1`,
    [username]
  );
  return rows[0] || null;
}

async function RegisterUser(username, email, hash, phone = null, birth_date = null, residence = null, gender = null) {
  const { rows } = await pool.query(
    `INSERT INTO users (username, email, password_hash, role, phone, birth_date, residence, gender)
     VALUES ($1, $2, $3, $4, $5, $6, $7, $8)
     RETURNING id, username, email, role, phone, birth_date, residence, gender`,
    [username, email, hash, 'user', phone, birth_date, residence, gender]
  );
  return rows[0] || null;
}

async function findUserById(id) {
  const { rows } = await pool.query(
    `SELECT id, username, email, role, phone, birth_date, residence, gender
     FROM users
     WHERE id = $1`,
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

module.exports = { findUserByUsername, findUserById, RegisterUser, saveRefreshSession, findRefreshSession };