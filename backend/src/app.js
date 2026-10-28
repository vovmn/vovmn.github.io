const express = require('express');
const cors = require('cors');
const cookieParser = require('cookie-parser');
const helmet = require('helmet');
const { rateLimit } = require('express-rate-limit');
const pool = require('./db/pool');
const adminRoutes = require('./modules/admin/admin.routes');
const appointmentRoutes = require('./modules/appointments/appointments.routes');
const authRoutes = require('./modules/auth/auth.routes');
const questionnaireRoutes = require('./modules/questionnaire/questionnaire.routes');

const app = express();

app.disable('x-powered-by');
app.set('trust proxy', 1);
app.use(helmet());

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
    const error = new Error('CORS origin denied');
    error.status = 403;
    return callback(error);
  },
  credentials: true,
  allowedHeaders: ['Content-Type', 'Authorization'],
  methods: ['GET', 'POST', 'PUT', 'PATCH', 'DELETE', 'OPTIONS'],
}));

app.use(cookieParser());
app.use(express.json({ limit: '256kb' }));

const apiLimiter = rateLimit({
  windowMs: 15 * 60 * 1000,
  limit: 300,
  standardHeaders: 'draft-8',
  legacyHeaders: false,
});
const authLimiter = rateLimit({
  windowMs: 15 * 60 * 1000,
  limit: 30,
  standardHeaders: 'draft-8',
  legacyHeaders: false,
});

app.use('/api', apiLimiter);
app.use(['/auth', '/api/auth'], authLimiter);

app.use('/api/admin', adminRoutes);
app.use('/api/appointments', appointmentRoutes);
app.use('/api/questionnaire', questionnaireRoutes);

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

app.use((err, req, res, next) => {
  if (res.headersSent) return next(err);
  const status = Number.isInteger(err.status) ? err.status : 500;
  if (status >= 500) console.error('Unhandled request error:', err.message);
  return res.status(status).json({
    error: status >= 500 ? 'Internal server error' : err.message,
  });
});

module.exports = app;
