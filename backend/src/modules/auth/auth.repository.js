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

async function RegisterUser(username, email, hash) {
  const { rows } = await pool.query(
    `INSERT INTO users (username, email, password_hash, role)
     VALUES ($1, $2, $3, $4)
     RETURNING id, username, role`,
    [username, email, hash, 'user']
  )
  return rows[0] || null
}

module.exports = { findUserByUsername, RegisterUser }