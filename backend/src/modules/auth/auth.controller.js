const authService = require('./auth.service')

async function login(req, res) {
  try {
    const { username, password } = req.body || {}

    if (!username || !password) {
      return res.status(400).json({ error: 'username and password are required' })
    }

    const { accessToken, refreshToken } = await authService.login(username, password)

    // refresh-cookie (если используешь refresh)
    res.cookie('refresh_token', refreshToken, {
      httpOnly: true,
      sameSite: 'lax',
      secure: false, // в проде true (https)
      path: '/auth',
      maxAge: 30 * 24 * 60 * 60 * 1000, // 30 дней
    })

    return res.json({ access_token: accessToken })
  } catch (err) {
    const status = err.status || 500
    return res.status(status).json({ error: err.message || 'Internal server error' })
  }
}

module.exports = { login }