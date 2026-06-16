const argon2 = require('argon2');
const jwt = require('jsonwebtoken');
const crypto = require('crypto');
const repo = require('./auth.repository');

function signAccessToken(user) {
  return jwt.sign(
    { sub: user.id, role: user.role, gender: user.gender }, // ← теперь gender в токене
    process.env.JWT_ACCESS_SECRET,
    { expiresIn: '15m' }
  );
}

function makeRefreshToken() {
  return crypto.randomBytes(64).toString('hex');
}

function hashToken(token) {
  return crypto.createHash('sha256').update(token).digest('hex');
}

async function login(username, password) {
  const user = await repo.findUserByUsername(username);
  if (!user) {
    const err = new Error('Invalid credentials');
    err.status = 401;
    throw err;
  }

  const ok = await argon2.verify(user.password_hash, password);
  if (!ok) {
    const err = new Error('Invalid credentials');
    err.status = 401;
    throw err;
  }

  const accessToken = signAccessToken(user);

  const refreshToken = makeRefreshToken();
  const refreshHash = hashToken(refreshToken);
  const expiresAt = new Date(Date.now() + 30 * 24 * 60 * 60 * 1000);
  await repo.saveRefreshSession(user.id, refreshHash, expiresAt);

  return { accessToken, refreshToken, user };
}

async function verifyAccessToken(token) {
  try {
    return jwt.verify(token, process.env.JWT_ACCESS_SECRET);
  } catch (err) {
    const error = new Error('Invalid or expired access token');
    error.status = 401;
    throw error;
  }
}

async function findUserById(id) {
  return repo.findUserById(id);
}

async function register(username, email, password, phone = null, birth_date = null, residence = null, gender = null) {
  const existing = await repo.findUserByUsername(username);
  if (existing) {
    const err = new Error('User already exists');
    err.status = 409;
    throw err;
  }

  const hash = await argon2.hash(password);
  const user = await repo.RegisterUser(username, email, hash, phone, birth_date, residence, gender);
  const accessToken = signAccessToken(user);
  return { user, accessToken };
}

module.exports = { login, register, verifyAccessToken, findUserById, signAccessToken };