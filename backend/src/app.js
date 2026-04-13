const express = require('express')
const pool = require('./db/pool') // твой пул для работы с PostgreSQL, например, с помощью pg.Pool
const cookieParser = require('cookie-parser') // для парсинга cookies, если используешь httpOnly refresh токены в куках
const authRoutes = require('./modules/auth/auth.routes') // роуты для /auth, например, POST /auth/login, POST /auth/refresh и т.д.
const app = express()

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