const authService = require('./auth.service');

async function login(req, res) {
  try {
    const { username, password } = req.body || {};
    if (!username || !password) {
      return res.status(400).json({ error: 'username and password are required' });
    }

    const { accessToken, refreshToken, user } = await authService.login(username, password);

    res.cookie('refresh_token', refreshToken, {
      httpOnly: true,
      sameSite: 'lax',
      secure: process.env.NODE_ENV === 'production',
      path: '/auth',
      maxAge: 30 * 24 * 60 * 60 * 1000,
    });

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
    const status = err.status || 500;
    return res.status(status).json({ error: err.message || 'Internal server error' });
  }
}

async function register(req, res) {
  try {
    let { username, email, password, phone, birth_date, residence, gender } = req.body || {};
    if (!username || !email || !password) {
      return res.status(400).json({ error: 'username, email and password are required' });
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

    const { user, accessToken } = await authService.register(
      username, email, password, phone, birth_date, residence, gender
    );

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
    const status = err.status || 500;
    return res.status(status).json({ error: err.message || 'Internal server error' });
  }
}

async function me(req, res) {
  try {
    const authorization = req.headers.authorization || '';
    let token = authorization.replace(/^Bearer\s+/i, '');
    if (!token && req.body && typeof req.body.token === 'string') {
      token = req.body.token;
    }
    if (!token && req.cookies && req.cookies.access_token) {
      token = req.cookies.access_token;
    }

    if (!token) {
      return res.status(401).json({ error: 'Authorization token required' });
    }

    let payload;
    try {
      payload = await authService.verifyAccessToken(token);
    } catch (err) {
      console.warn('auth.me: access token verify failed', err.message);
      return res.status(401).json({ error: 'Invalid or expired access token' });
    }

    console.log('auth.me token:', token);
    console.log('auth.me payload:', payload);

    const user = await authService.findUserById(payload.sub);
    if (!user) {
      if (payload.username) {
        const alt = await authService.findUserByUsername?.(payload.username);
        if (alt) {
          return res.json({
            id: alt.id, username: alt.username, email: alt.email, role: alt.role,
            gender: alt.gender, phone: alt.phone, birth_date: alt.birth_date, residence: alt.residence
          });
        }
      }
      return res.status(404).json({ error: 'User not found' });
    }

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
    const status = err.status || 401;
    return res.status(status).json({ error: err.message || 'Unauthorized' });
  }
}

async function logout(req, res) {
  res.clearCookie('refresh_token', { path: '/auth' });
  return res.json({ success: true });
}

async function refresh(req, res) {
  try {
    const token = req.cookies && req.cookies.refresh_token;
    if (!token) return res.status(401).json({ error: 'Refresh token required' });

    const hash = require('crypto').createHash('sha256').update(token).digest('hex');
    const session = await authService.findRefreshSession(hash);
    if (!session) return res.status(401).json({ error: 'Invalid or expired refresh token' });

    const user = await authService.findUserById(session.user_id);
    if (!user) return res.status(404).json({ error: 'User not found' });

    const accessToken = authService.signAccessToken(user);
    return res.json({ access_token: accessToken });
  } catch (err) {
    const status = err.status || 401;
    return res.status(status).json({ error: err.message || 'Unauthorized' });
  }
}

module.exports = { login, register, me, logout, refresh };