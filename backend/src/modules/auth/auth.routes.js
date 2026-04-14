const router = require('express').Router()
const controller = require('./auth.controller')
const requireAuth = require('../../middlewares/requireAuth')

router.post('/login', controller.login) // POST /auth/login, body: { username, password }
router.post('/register', controller.register) // POST /auth/register, body: { username, email, password }
router.post('/refresh', controller.refresh) // POST /auth/refresh, cookies: { refresh_token }
router.get('/me', requireAuth, controller.me) // GET /auth/me, headers: { Authorization: Bearer <token> }
router.post('/logout', requireAuth, (req, res) => {
  res.clearCookie('refresh_token')
  return res.json({ status: 'ok' })
})

module.exports = router