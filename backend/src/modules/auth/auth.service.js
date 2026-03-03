const argon2 = require('argon2')
const jwt = require('jsonwebtoken')
const crypto = require('crypto')
const repo = require('./auth.repository')

function signAccessToken(user) {
  return jwt.sign(
    { sub: user.id, role: user.role },
    process.env.JWT_ACCESS_SECRET,
    { expiresIn: '15m' }
  )
}

// refresh лучше делать “случайной строкой”, а не JWT
function makeRefreshToken() {
  return crypto.randomBytes(64).toString('hex') // 128 hex chars
}

// хешируем refresh перед записью в БД
function hashToken(token) {
  return crypto.createHash('sha256').update(token).digest('hex')
}

async function login(username, password) {
  const user = await repo.findUserByUsername(username)

  // не палим “юзер не найден” — всегда одинаково
  if (!user) {
    const err = new Error('Invalid credentials')
    err.status = 401
    throw err
  }

const ok = await argon2.verify(user.password_hash, password)
  if (!ok) {
    const err = new Error('Invalid credentials')
    err.status = 401
    throw err
  }

  const accessToken = signAccessToken(user)

  // если делаешь refresh:
  const refreshToken = makeRefreshToken()
  const refreshHash = hashToken(refreshToken)

  // TODO: сохранить refreshHash в БД + expires_at (например +30 дней)
  // await repo.saveRefreshSession(user.id, refreshHash, expiresAt)

  return { accessToken, refreshToken }
}

module.exports = { login }