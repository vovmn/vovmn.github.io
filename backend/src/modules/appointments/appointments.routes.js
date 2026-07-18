const router = require('express').Router();
const ctrl = require('./appointments.controller');
const requireAuth = require('../../middlewares/requireAuth');

router.get('/my', requireAuth, ctrl.myAppointments);

module.exports = router;
