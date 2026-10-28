const authService = require('./auth.service');

function sendError(res, err, fallback = 'Internal server error') {
  const status = Number.isInteger(err.status) ? err.status : 500;
  return res.status(status).json({
    error: status >= 500 ? fallback : (err.message || fallback),
  });
}

function setRefreshCookie(res, refreshToken) {
  res.cookie('refresh_token', refreshToken, {
    httpOnly: true,
    sameSite: 'lax',
    secure: process.env.NODE_ENV === 'production',
    path: '/',
    maxAge: 30 * 24 * 60 * 60 * 1000,
  });
}

async function login(req, res) {
  try {
    const { username, password } = req.body || {};
    if (typeof username !== 'string' || typeof password !== 'string' || !username || !password) {
      return res.status(400).json({ error: 'username and password are required' });
    }

    const { accessToken, refreshToken, user } = await authService.login(username, password);
    setRefreshCookie(res, refreshToken);

    return res.json({
      access_token: accessToken,
      user: {
        id: user.id,
        username: user.username,
        email: user.email,
        role: user.role,
        gender: user.gender,        // ← добавлено
      },
    });
  } catch (err) {
    return sendError(res, err);
  }
}

async function register(req, res) {
  try {
    let { username, email, password, phone, birth_date, residence, gender } = req.body || {};
    if (!username || !email || !password) {
      return res.status(400).json({ error: 'username, email and password are required' });
    }
    if (typeof password !== 'string' || password.length < 12 || password.length > 128) {
      return res.status(400).json({ error: 'password must contain 12 to 128 characters' });
    }
    if (typeof username !== 'string' || username.length < 3 || username.length > 100) {
      return res.status(400).json({ error: 'username must contain 3 to 100 characters' });
    }
    if (typeof email !== 'string' || email.length > 255 || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      return res.status(400).json({ error: 'valid email is required' });
    }

    // нормализация телефона
    if (phone) {
      const digits = phone.replace(/[^+0-9]/g, '');
      let norm = digits;
      if (!/^[+]/.test(norm) && /^[0-9]+$/.test(norm)) {
        norm = '+' + norm;
      }
      const numOnly = norm.replace(/[^0-9]/g, '');
      if (numOnly.length < 7 || numOnly.length > 15) {
        return res.status(400).json({ error: 'Invalid phone number format' });
      }
      phone = norm;
    }

    if (birth_date) {
      const d = new Date(birth_date);
      if (Number.isNaN(d.getTime())) {
        return res.status(400).json({ error: 'Invalid birth_date' });
      }
      const now = new Date();
      if (d > now) return res.status(400).json({ error: 'birth_date cannot be in the future' });
      const age = Math.floor((now - d) / (365.25 * 24 * 60 * 60 * 1000));
      if (age < 13) return res.status(400).json({ error: 'User must be at least 13 years old' });
      birth_date = d.toISOString().slice(0, 10);
    }

    // валидация gender (опционально)
    if (gender && !['M', 'F'].includes(gender)) {
      return res.status(400).json({ error: 'gender must be M or F' });
    }

    const { user, accessToken, refreshToken } = await authService.register(
      username, email, password, phone, birth_date, residence, gender
    );
    setRefreshCookie(res, refreshToken);

    return res.status(201).json({
      access_token: accessToken,
      user: {
        id: user.id,
        username: user.username,
        email: user.email,
        role: user.role,
        phone: user.phone,
        birth_date: user.birth_date,
        residence: user.residence,
        gender: user.gender,          // ← добавлено
      },
    });
  } catch (err) {
    return sendError(res, err);
  }
}

async function me(req, res) {
  try {
    const user = await authService.findUserById(req.user.sub);
    if (!user) return res.status(404).json({ error: 'User not found' });

    return res.json({
      id: user.id,
      username: user.username,
      email: user.email,
      role: user.role,
      phone: user.phone,
      birth_date: user.birth_date,
      residence: user.residence,
      gender: user.gender,               // ← добавлено
    });
  } catch (err) {
    return sendError(res, err, 'Unauthorized');
  }
}

async function logout(req, res) {
  try {
    await authService.revokeRefreshToken(req.cookies && req.cookies.refresh_token);
    res.clearCookie('refresh_token', { path: '/' });
    return res.json({ success: true });
  } catch (err) {
    return sendError(res, err);
  }
}

async function refresh(req, res) {
  try {
    const token = req.cookies && req.cookies.refresh_token;
    if (!token) return res.status(401).json({ error: 'Refresh token required' });

    const { accessToken, refreshToken } = await authService.rotateRefreshToken(token);
    setRefreshCookie(res, refreshToken);
    return res.json({ access_token: accessToken });
  } catch (err) {
    return sendError(res, err, 'Unauthorized');
  }
}

module.exports = { login, register, me, logout, refresh };
