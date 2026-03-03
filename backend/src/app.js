const express = require('express')
const pool = require('./db/pool')

const app = express()

app.use(express.json())

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