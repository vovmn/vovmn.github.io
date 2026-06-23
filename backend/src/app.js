const express = require('express')
const cors = require('cors')
const pool = require('./db/pool') // твой пул для работы с PostgreSQL, например, с помощью pg.Pool
const cookieParser = require('cookie-parser') // для парсинга cookies, если используешь httpOnly refresh токены в куках
const authRoutes = require('./modules/auth/auth.routes') // роуты для /auth, например, POST /auth/login, POST /auth/me и т.д.
const app = express()
const questionnaireRoutes = require('./modules/questionnaire/questionnaire.routes');


const allowedOrigins = (process.env.CLIENT_ORIGIN || process.env.CORS_ORIGIN || 'http://localhost:5173')
  .split(',')
  .map((origin) => origin.trim())
  .filter(Boolean)

function isLocalOrigin(origin) {
  return [
    /^http:\/\/localhost(:\d+)?$/,
    /^http:\/\/127\.0\.0\.1(:\d+)?$/,
    /^http:\/\/\[::1\](:\d+)?$/,
  ].some((pattern) => pattern.test(origin))
}

app.use(cors({
  origin: function (origin, callback) {
    if (!origin) return callback(null, true)
    if (allowedOrigins.includes(origin) || isLocalOrigin(origin)) {
      return callback(null, true)
    }
    return callback(new Error(`CORS origin denied: ${origin}`))
  },
  credentials: true,
  allowedHeaders: ['Content-Type', 'Authorization'],
  methods: ['GET', 'POST', 'PUT', 'PATCH', 'DELETE', 'OPTIONS'],
}))
app.use(cookieParser())
app.use(express.json());
app.use('/api/questionnaire', questionnaireRoutes);

// Ensure new user columns exist (safe, idempotent migration at app start)
;(async () => {
  try {
    await pool.query("ALTER TABLE users ADD COLUMN IF NOT EXISTS phone VARCHAR(50);")
    await pool.query("ALTER TABLE users ADD COLUMN IF NOT EXISTS birth_date DATE;")
    await pool.query("ALTER TABLE users ADD COLUMN IF NOT EXISTS residence VARCHAR(255);")
    console.log('DB schema ensured: users.phone, birth_date, residence')
  } catch (err) {
    console.error('Schema migration failed:', err.message)
  }
})()

// Mount auth routes on both /auth and /api/auth to support different frontends
app.use('/auth', authRoutes)
app.use('/api/auth', authRoutes)

app.get('/health', async (req, res) => { // простой тест для проверки, что сервер и БД работают
  try {
    const result = await pool.query('SELECT 1 as test')
    res.json({
      status: 'ok',
      db: result.rows[0],
    })
  } catch (err) {
    console.error(err)
    res.status(500).json({ error: 'Database connection failed' })
  }
})



module.exports = app
