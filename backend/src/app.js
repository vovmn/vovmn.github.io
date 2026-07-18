const express = require('express');
const cors = require('cors');
const cookieParser = require('cookie-parser');
const pool = require('./db/pool');
const adminRoutes = require('./modules/admin/admin.routes');
const appointmentRoutes = require('./modules/appointments/appointments.routes');
const authRoutes = require('./modules/auth/auth.routes');
const questionnaireRoutes = require('./modules/questionnaire/questionnaire.routes');

const app = express();

const allowedOrigins = (process.env.CLIENT_ORIGIN || process.env.CORS_ORIGIN || 'http://localhost:5173')
  .split(',')
  .map((origin) => origin.trim())
  .filter(Boolean);

function isLocalOrigin(origin) {
  return [
    /^http:\/\/localhost(:\d+)?$/,
    /^http:\/\/127\.0\.0\.1(:\d+)?$/,
    /^http:\/\/\[::1\](:\d+)?$/,
  ].some((pattern) => pattern.test(origin));
}

app.use(cors({
  origin(origin, callback) {
    if (!origin) return callback(null, true);
    if (allowedOrigins.includes(origin) || isLocalOrigin(origin)) {
      return callback(null, true);
    }
    return callback(new Error(`CORS origin denied: ${origin}`));
  },
  credentials: true,
  allowedHeaders: ['Content-Type', 'Authorization'],
  methods: ['GET', 'POST', 'PUT', 'PATCH', 'DELETE', 'OPTIONS'],
}));

app.use(cookieParser());
app.use(express.json());

app.use('/api/admin', adminRoutes);
app.use('/api/appointments', appointmentRoutes);
app.use('/api/questionnaire', questionnaireRoutes);

;(async () => {
  try {
    await pool.query('CREATE EXTENSION IF NOT EXISTS "uuid-ossp";');
    await pool.query('ALTER TABLE users ADD COLUMN IF NOT EXISTS phone VARCHAR(50);');
    await pool.query('ALTER TABLE users ADD COLUMN IF NOT EXISTS birth_date DATE;');
    await pool.query('ALTER TABLE users ADD COLUMN IF NOT EXISTS residence VARCHAR(255);');
    await pool.query(`
      CREATE TABLE IF NOT EXISTS appointments (
        id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
        user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
        scheduled_at TIMESTAMP NOT NULL,
        specialist VARCHAR(255),
        doctor VARCHAR(255),
        status VARCHAR(20) DEFAULT 'scheduled' CHECK (status IN ('scheduled','completed','cancelled')),
        comment TEXT,
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
      );
      CREATE INDEX IF NOT EXISTS idx_appointments_user ON appointments(user_id);
      CREATE INDEX IF NOT EXISTS idx_appointments_scheduled_at ON appointments(scheduled_at);
    `);
    console.log('DB schema ensured');
  } catch (err) {
    console.error('Schema migration failed:', err.message);
  }
})();

app.use('/auth', authRoutes);
app.use('/api/auth', authRoutes);

app.get('/health', async (req, res) => {
  try {
    const result = await pool.query('SELECT 1 as test');
    res.json({
      status: 'ok',
      db: result.rows[0],
    });
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: 'Database connection failed' });
  }
});

module.exports = app;
