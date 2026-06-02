const router = require('express').Router()
const controller = require('./auth.controller')

// debug: ensure handlers are functions
try {
	console.log('auth.controller keys:', Object.keys(controller))
	console.log('types:', {
		login: typeof controller.login,
		register: typeof controller.register,
		me: typeof controller.me,
		logout: typeof controller.logout,
		refresh: typeof controller.refresh,
	})
} catch (e) {
	console.error('auth.routes debug error', e)
}

router.post('/login', (req, res, next) => {
	if (typeof controller.login === 'function') return controller.login(req, res, next)
	next(new Error('auth.handler.login is not a function'))
}) // POST /auth/login, body: { username, password }

router.post('/register', (req, res, next) => {
	if (typeof controller.register === 'function') return controller.register(req, res, next)
	next(new Error('auth.handler.register is not a function'))
}) // POST /auth/register, body: { username, password }

router.get('/me', (req, res, next) => {
	if (typeof controller.me === 'function') return controller.me(req, res, next)
	next(new Error('auth.handler.me is not a function'))
}) // GET /auth/me

router.post('/logout', (req, res, next) => {
	if (typeof controller.logout === 'function') return controller.logout(req, res, next)
	next(new Error('auth.handler.logout is not a function'))
}) // POST /auth/logout

router.post('/refresh', (req, res, next) => {
	if (typeof controller.refresh === 'function') return controller.refresh(req, res, next)
	next(new Error('auth.handler.refresh is not a function'))
}) // POST /auth/refresh

module.exports = router