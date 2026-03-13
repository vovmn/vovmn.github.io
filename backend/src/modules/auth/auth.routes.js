const router = require('express').Router()
const controller = require('./auth.controller')

router.post('/login', controller.login) // POST /auth/login, body: { username, password }
router.post('/register', controller.register) // POST /auth/register, body: { username, password }

module.exports = router