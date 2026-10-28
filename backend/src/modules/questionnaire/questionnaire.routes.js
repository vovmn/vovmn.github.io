const router = require('express').Router();
const ctrl = require('./questionnaire.controller');
const requireAuth = require('../../middlewares/requireAuth');

router.get('/systems', requireAuth, ctrl.getSystems);
router.post('/assignments/:assignmentId/start', requireAuth, ctrl.startQuestionnaire);
router.post('/submit/:assignmentId', requireAuth, ctrl.submitAnswers);
router.get('/my-assignments', requireAuth, ctrl.myAssignments);

module.exports = router;
