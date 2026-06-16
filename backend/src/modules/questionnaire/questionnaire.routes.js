const router = require('express').Router();
const ctrl = require('./questionnaire.controller');

// Middleware для проверки JWT — можно использовать тот же, что и в auth, но упростим
// Предполагаем, что есть middleware, который добавляет req.user (из access token)
// Добавлю простой verifyToken, если у тебя его нет отдельно:
const jwt = require('jsonwebtoken');

function verifyToken(req, res, next) {
  const authHeader = req.headers.authorization;
  if (!authHeader || !authHeader.startsWith('Bearer ')) {
    return res.status(401).json({ error: 'Access token required' });
  }
  const token = authHeader.split(' ')[1];
  try {
    const decoded = jwt.verify(token, process.env.JWT_ACCESS_SECRET);
    req.user = decoded; // decoded содержит sub, role, gender
    next();
  } catch (err) {
    return res.status(401).json({ error: 'Invalid or expired token' });
  }
}

router.get('/systems', verifyToken, ctrl.getSystems);
router.post('/start/:systemCode', verifyToken, ctrl.startQuestionnaire);
router.post('/submit/:assignmentId', verifyToken, ctrl.submitAnswers);

module.exports = router;