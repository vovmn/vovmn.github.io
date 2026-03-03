const express = require('express')
const pool = require('./db/pool')
const cookieParser = require('cookie-parser')
const authRoutes = require('./modules/auth/auth.routes')
const app = express()

app.use(cookieParser())
app.use(express.json())
app.use('/auth', authRoutes)

app.get('/health', async (req, res) => {
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