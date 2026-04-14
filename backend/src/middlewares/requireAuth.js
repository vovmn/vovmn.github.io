const jwt = require('jsonwebtoken')

function requireAuth(req, res, next) {
  const authHeader = req.headers.authorization || ''
  const token = authHeader.replace('Bearer ', '')

  if (!token) {
    return res.status(401).json({ error: 'Missing token' })
  }

  try {
    const payload = jwt.verify(token, process.env.JWT_ACCESS_SECRET)
    req.user = payload
    next()
  } catch (err) {
    return res.status(401).json({ error: 'Invalid token' })
  }
}

module.exports = requireAuth
