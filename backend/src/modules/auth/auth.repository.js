const pool = require('../../db/pool')

async function findUserByUsername(username) {
  const { rows } = await pool.query(
    `SELECT id, username, password_hash, role
     FROM users
     WHERE username = $1`,
    [username]
  )
  return rows[0] || null
}

async function findUserById(id) {
  const { rows } = await pool.query(
    `SELECT id, username, email, role
     FROM users
     WHERE id = $1`,
    [id]
  )
  return rows[0] || null
}

async function RegisterUser(username, email, hash) {
  const { rows } = await pool.query(
    `INSERT INTO users (username, email, password_hash, role)
     VALUES ($1, $2, $3, $4)
     RETURNING id, username, role`,
    [username, email, hash, 'user']
  )
  return rows[0] || null
}

async function saveRefreshSession(userId, tokenHash, expiresAt) {
  const { rows } = await pool.query(
    `INSERT INTO refresh_tokens (user_id, token_hash, expires_at)
     VALUES ($1, $2, $3)
     RETURNING id`,
    [userId, tokenHash, expiresAt]
  )
  return rows[0] || null
}

async function findRefreshSession(tokenHash) {
  const { rows } = await pool.query(
    `SELECT id, user_id, expires_at, revoked_at
     FROM refresh_tokens
     WHERE token_hash = $1`,
    [tokenHash]
  )
  return rows[0] || null
}

async function revokeRefreshToken(tokenHash) {
  await pool.query(
    `UPDATE refresh_tokens SET revoked_at = NOW()
     WHERE token_hash = $1`,
    [tokenHash]
  )
}

module.exports = {
  findUserByUsername,
  findUserById,
  RegisterUser,
  saveRefreshSession,
  findRefreshSession,
  revokeRefreshToken,
}