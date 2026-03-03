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

module.exports = { findUserByUsername }