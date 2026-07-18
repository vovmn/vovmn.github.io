const router = require('express').Router();
const ctrl = require('./admin.controller');
const requireAuth = require('../../middlewares/requireAuth');
const requireAdmin = require('../../middlewares/requireAdmin');

router.use(requireAuth, requireAdmin);

router.get('/users', ctrl.listUsers);
router.get('/systems', ctrl.listSystems);
router.get('/questionnaire-assignments', ctrl.listQuestionnaireAssignments);
router.post('/questionnaire-assignments', ctrl.assignQuestionnaire);
router.get('/appointments', ctrl.listAppointments);
router.post('/appointments', ctrl.createAppointment);

module.exports = router;
