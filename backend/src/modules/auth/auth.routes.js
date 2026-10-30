const router = require('express').Router()
const controller = require('./auth.controller')
const requireAuth = require('../../middlewares/requireAuth')

router.post('/login', controller.login)
router.get('/me', requireAuth, controller.me)
router.post('/logout', controller.logout)
router.post('/refresh', controller.refresh)

module.exports = router
