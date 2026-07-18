const pool = require('../../db/pool');

async function listUsers(req, res) {
  try {
    const { rows } = await pool.query(
      `SELECT id, username, email, role, gender, phone
       FROM users
       ORDER BY username`
    );
    res.json(rows);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: 'Failed to load users' });
  }
}

async function listSystems(req, res) {
  try {
    const { rows } = await pool.query(
      `SELECT id, code, name
       FROM body_systems
       ORDER BY name`
    );
    res.json(rows);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: 'Failed to load systems' });
  }
}

async function listQuestionnaireAssignments(req, res) {
  try {
    const { rows } = await pool.query(
      `SELECT a.id, a.user_id, u.username, a.system_id, s.code, s.name,
              a.status, a.assigned_at, a.completed_at
       FROM user_questionnaire_assignments a
       JOIN users u ON u.id = a.user_id
       JOIN body_systems s ON s.id = a.system_id
       ORDER BY a.assigned_at DESC
       LIMIT 100`
    );
    res.json(rows);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: 'Failed to load assignments' });
  }
}

async function assignQuestionnaire(req, res) {
  const { user_id, system_id } = req.body || {};

  if (!user_id || !system_id) {
    return res.status(400).json({ error: 'user_id and system_id are required' });
  }

  try {
    const { rows } = await pool.query(
      `INSERT INTO user_questionnaire_assignments (user_id, system_id, status, assigned_at, completed_at)
       VALUES ($1, $2, 'pending', NOW(), NULL)
       ON CONFLICT (user_id, system_id)
       DO UPDATE SET status = 'pending',
                     assigned_at = NOW(),
                     completed_at = NULL
       RETURNING id, user_id, system_id, status, assigned_at, completed_at`,
      [user_id, system_id]
    );

    await pool.query(
      'DELETE FROM user_answers WHERE user_id = $1 AND assignment_id = $2',
      [user_id, rows[0].id]
    );

    res.status(201).json(rows[0]);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: 'Failed to assign questionnaire' });
  }
}

async function listAppointments(req, res) {
  try {
    const { rows } = await pool.query(
      `SELECT a.id, a.user_id, u.username, a.specialist, a.doctor,
              a.scheduled_at, a.status, a.comment, a.created_at
       FROM appointments a
       JOIN users u ON u.id = a.user_id
       ORDER BY a.scheduled_at DESC
       LIMIT 100`
    );
    res.json(rows);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: 'Failed to load appointments' });
  }
}

async function createAppointment(req, res) {
  const { user_id, scheduled_at, specialist, doctor, comment } = req.body || {};

  if (!user_id || !scheduled_at) {
    return res.status(400).json({ error: 'user_id and scheduled_at are required' });
  }

  const scheduledDate = new Date(scheduled_at);
  if (Number.isNaN(scheduledDate.getTime())) {
    return res.status(400).json({ error: 'scheduled_at must be a valid date' });
  }

  try {
    const { rows } = await pool.query(
      `INSERT INTO appointments (user_id, scheduled_at, specialist, doctor, comment, status)
       VALUES ($1, $2, $3, $4, $5, 'scheduled')
       RETURNING id, user_id, scheduled_at, specialist, doctor, comment, status, created_at`,
      [user_id, scheduledDate.toISOString(), specialist || null, doctor || null, comment || null]
    );

    res.status(201).json(rows[0]);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: 'Failed to create appointment' });
  }
}

module.exports = {
  assignQuestionnaire,
  createAppointment,
  listAppointments,
  listQuestionnaireAssignments,
  listSystems,
  listUsers,
};
