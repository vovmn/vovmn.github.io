const pool = require('../../db/pool');

async function listUsers(req, res) {
  try {
    const { rows } = await pool.query(
      `SELECT id, username, email, role, gender, phone, birth_date, residence
       FROM users
       WHERE role <> 'admin'
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
      `SELECT s.id, s.code, s.name,
              COUNT(q.id) FILTER (WHERE q.active)::int AS question_count
       FROM body_systems s
       LEFT JOIN questions q ON q.system_id = s.id
       GROUP BY s.id
       ORDER BY s.name`
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
      `SELECT a.id, a.user_id, u.username, u.email, a.system_id, s.code, s.name,
              a.status, a.priority, a.due_at, a.note, a.questionnaire_version,
              a.assigned_at, a.started_at, a.completed_at, a.cancelled_at,
              administrator.username AS assigned_by_name,
              COUNT(ua.id)::int AS answer_count,
              COUNT(ua.id) FILTER (WHERE ua.skipped)::int AS skipped_count
       FROM user_questionnaire_assignments a
       JOIN users u ON u.id = a.user_id
       JOIN body_systems s ON s.id = a.system_id
       LEFT JOIN users administrator ON administrator.id = a.assigned_by
       LEFT JOIN user_answers ua ON ua.assignment_id = a.id
       GROUP BY a.id, u.id, s.id, administrator.id
       ORDER BY a.assigned_at DESC
       LIMIT 250`
    );
    res.json(rows);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: 'Failed to load assignments' });
  }
}

async function assignQuestionnaire(req, res) {
  const { user_id, system_ids, due_at, priority = 'normal', note = null } = req.body || {};

  if (!user_id || !Array.isArray(system_ids) || system_ids.length === 0) {
    return res.status(400).json({ error: 'user_id and system_ids are required' });
  }
  if (!['low', 'normal', 'high', 'urgent'].includes(priority)) {
    return res.status(400).json({ error: 'invalid priority' });
  }
  const dueDate = due_at ? new Date(due_at) : null;
  if (dueDate && Number.isNaN(dueDate.getTime())) {
    return res.status(400).json({ error: 'due_at must be a valid date' });
  }

  const client = await pool.connect();
  try {
    await client.query('BEGIN');
    const target = await client.query(
      `SELECT id FROM users WHERE id = $1 AND role <> 'admin'`,
      [user_id]
    );
    if (target.rows.length === 0) {
      await client.query('ROLLBACK');
      return res.status(404).json({ error: 'Patient not found' });
    }
    const systems = await client.query(
      `SELECT id FROM body_systems WHERE id = ANY($1::uuid[])`,
      [system_ids]
    );
    if (systems.rows.length !== new Set(system_ids.map(String)).size) {
      await client.query('ROLLBACK');
      return res.status(400).json({ error: 'One or more questionnaire systems are invalid' });
    }

    const created = [];
    for (const systemId of [...new Set(system_ids.map(String))]) {
      const { rows } = await client.query(
        `INSERT INTO user_questionnaire_assignments (
           user_id, system_id, assigned_by, status, priority, due_at, note,
           questionnaire_version, assigned_at, updated_at
         )
         VALUES ($1, $2, $3, 'pending', $4, $5, $6, '2.0.0-draft', NOW(), NOW())
         RETURNING id, user_id, system_id, status, priority, due_at, note, assigned_at`,
        [user_id, systemId, req.user.sub, priority, dueDate?.toISOString() || null, note || null]
      );
      created.push(rows[0]);
    }
    await client.query('COMMIT');

    res.status(201).json({ assignments: created });
  } catch (err) {
    await client.query('ROLLBACK');
    console.error(err);
    res.status(500).json({ error: 'Failed to assign questionnaire' });
  } finally {
    client.release();
  }
}

async function cancelQuestionnaireAssignment(req, res) {
  try {
    const { rows } = await pool.query(
      `UPDATE user_questionnaire_assignments
       SET status = 'cancelled', cancelled_at = NOW(), updated_at = NOW()
       WHERE id = $1 AND status IN ('pending','in_progress')
       RETURNING id, status, cancelled_at`,
      [req.params.id]
    );
    if (rows.length === 0) return res.status(409).json({ error: 'Assignment cannot be cancelled' });
    res.json(rows[0]);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: 'Failed to cancel assignment' });
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
  cancelQuestionnaireAssignment,
  createAppointment,
  listAppointments,
  listQuestionnaireAssignments,
  listSystems,
  listUsers,
};
