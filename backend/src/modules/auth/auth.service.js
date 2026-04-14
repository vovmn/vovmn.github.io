const argon2 = require('argon2') // argon2 — современный алгоритм для хеширования паролей
const jwt = require('jsonwebtoken') // JWT — стандарт для токенов, jsonwebtoken — популярная библиотека для работы с ними
const crypto = require('crypto') // crypto — встроенный модуль для генерации случайных строк и хеширования
const repo = require('./auth.repository') // репозиторий для работы с БД, например, findUserByUsername, saveRefreshSession и т.д.

function signAccessToken(user) {
  return jwt.sign(
    { sub: user.id, role: user.role },
    process.env.JWT_ACCESS_SECRET,
    { expiresIn: '15m' }
  )
}
const TIME_REFRESH = 30 // дней
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

  // создаём refresh token
  const refreshToken = makeRefreshToken()
  const refreshHash = hashToken(refreshToken)
  
  // сохраняем в БД с expiration (30 дней)
  const expiresAt = new Date(Date.now() + TIME_REFRESH * 24 * 60 * 60 * 1000)
  await repo.saveRefreshSession(user.id, refreshHash, expiresAt)

  return { accessToken, refreshToken }
}


async function register(username, email, password) {
  // 1 проверяем, существует ли пользователь
  const existing = await repo.findUserByUsername(username)

  if (existing) {
    const err = new Error('User already exists')
    err.status = 409
    throw err
  }

  // 2 хешируем пароль
  const hash = await argon2.hash(password)

  // 3 создаём пользователя
  const user = await repo.RegisterUser(username, email, hash)

  // 4 создаём access token
  const accessToken = signAccessToken(user)

  // 5 создаём refresh token
  const refreshToken = makeRefreshToken()
  const refreshHash = hashToken(refreshToken)
  
  const expiresAt = new Date(Date.now() + 30 * 24 * 60 * 60 * 1000)
  await repo.saveRefreshSession(user.id, refreshHash, expiresAt)

  return {
    user,
    accessToken,
    refreshToken
  }
}


async function getMe(userId) {
  const user = await repo.findUserById(userId)
  if (!user) {
    const err = new Error('User not found')
    err.status = 404
    throw err
  }
  return user
}

async function refreshAccessToken(refreshToken) {
  const refreshHash = hashToken(refreshToken)
  const session = await repo.findRefreshSession(refreshHash)

  if (!session) {
    const err = new Error('Invalid refresh token')
    err.status = 401
    throw err
  }

  // проверяем expiration
  if (new Date(session.expires_at) < new Date()) {
    const err = new Error('Refresh token expired')
    err.status = 401
    throw err
  }

  // проверяем revoked
  if (session.revoked_at) {
    const err = new Error('Refresh token revoked')
    err.status = 401
    throw err
  }

  // получаем пользователя и выдаём новый access token
  const user = await repo.findUserById(session.user_id)
  const accessToken = signAccessToken(user)

  return { accessToken }
}

module.exports = { login, register, getMe, refreshAccessToken }