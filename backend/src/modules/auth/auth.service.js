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
  const expiresAt = new Date(Date.now() + 30 * 24 * 60 * 60 * 1000)

  await repo.saveRefreshSession(user.id, refreshHash, expiresAt)

  return { accessToken, refreshToken, user }
}

async function verifyAccessToken(token) {
  try {
    return jwt.verify(token, process.env.JWT_ACCESS_SECRET)
  } catch (err) {
    const error = new Error('Invalid or expired access token')
    error.status = 401
    throw error
  }
}

async function findUserById(id) {
  return repo.findUserById(id)
}

async function register(username, email, password, phone = null, birth_date = null, residence = null) {
  // 1 проверяем, существует ли пользователь
  const existing = await repo.findUserByUsername(username)

  if (existing) {
    const err = new Error('User already exists')
    err.status = 409
    throw err
  }

  // 2 хешируем пароль

  const hash = await argon2.hash(password)

  // 3 создаём пользователя (accept optional phone, birth_date, residence)
  // service layer expects repo.RegisterUser(username, email, hash, phone, birth_date, residence)
  // but controller will pass these parameters when available
  const user = await repo.RegisterUser(username, email, hash, phone, birth_date, residence)

  // 4 создаём access token
  const accessToken = signAccessToken(user)

  return {
    user,
    accessToken,
  }
}

module.exports = { login, register, verifyAccessToken, findUserById }

// export signAccessToken so controllers can issue tokens
module.exports.signAccessToken = signAccessToken
