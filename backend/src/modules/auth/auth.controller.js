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
        gender: user.gender,
        age: user.age,
        age_recorded_at: user.age_recorded_at,
        antigens: user.antigens,
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
      age: user.age,
      age_recorded_at: user.age_recorded_at,
      antigens: user.antigens,
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

module.exports = { login, me, logout, refresh };
