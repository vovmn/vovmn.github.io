const router = require('express').Router();
const ctrl = require('./admin.controller');
const requireAuth = require('../../middlewares/requireAuth');
const requireAdmin = require('../../middlewares/requireAdmin');

router.use(requireAuth, requireAdmin);

router.get('/users', ctrl.listUsers);
router.post('/users', ctrl.createUser);
router.patch('/users/:id/demographics', ctrl.updateUserDemographics);
router.patch('/users/:id/antigens', ctrl.updateUserAntigens);
router.get('/antigens', ctrl.listAntigens);
router.post('/antigens', ctrl.createAntigen);
router.get('/systems', ctrl.listSystems);
router.get('/questionnaire-assignments', ctrl.listQuestionnaireAssignments);
router.post('/questionnaire-assignments', ctrl.assignQuestionnaire);
router.patch('/questionnaire-assignments/:id/cancel', ctrl.cancelQuestionnaireAssignment);
router.get('/appointments', ctrl.listAppointments);
router.post('/appointments', ctrl.createAppointment);

module.exports = router;
