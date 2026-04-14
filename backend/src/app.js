const express = require('express')
const pool = require('./db/pool') // твой пул для работы с PostgreSQL, например, с помощью pg.Pool
const cookieParser = require('cookie-parser') // для парсинга cookies, если используешь httpOnly refresh токены в куках
const authRoutes = require('./modules/auth/auth.routes') // роуты для /auth, например, POST /auth/login, POST /auth/refresh и т.д.
const app = express()

// CORS middleware
app.use((req, res, next) => {
  res.header('Access-Control-Allow-Origin', 'http://localhost:5173')
  res.header('Access-Control-Allow-Headers', 'Origin, X-Requested-With, Content-Type, Accept, Authorization')
  res.header('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, OPTIONS')
  res.header('Access-Control-Allow-Credentials', 'true')
  if (req.method === 'OPTIONS') return res.sendStatus(200)
  next()
})

app.use(cookieParser())
app.use(express.json())
app.use('/auth', authRoutes)

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