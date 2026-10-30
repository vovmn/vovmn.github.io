const pool = require('../../db/pool');
const authService = require('../auth/auth.service');

const UUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

function isUuid(value) {
  return typeof value === 'string' && UUID_PATTERN.test(value);
}

function optionalString(value, maxLength) {
  if (value === undefined || value === null || value === '') return null;
  if (typeof value !== 'string' || value.length > maxLength) return undefined;
  return value.trim() || null;
}

function calculateAge(birthDate, today = new Date()) {
  const parsed = birthDate instanceof Date ? birthDate : new Date(birthDate);
  let age = today.getUTCFullYear() - parsed.getUTCFullYear();
  const birthdayPending = today.getUTCMonth() < parsed.getUTCMonth()
    || (today.getUTCMonth() === parsed.getUTCMonth() && today.getUTCDate() < parsed.getUTCDate());
  if (birthdayPending) age -= 1;
  return age;
}

async function createUser(req, res) {
  let { username, password, age, gender, email, phone, birth_date, residence } = req.body || {};
  username = typeof username === 'string' ? username.trim() : username;
  email = optionalString(email, 255);
  phone = optionalString(phone, 50);
  residence = optionalString(residence, 255);

  if (!username || !password || age === undefined || !gender) {
    return res.status(400).json({ error: 'username, password, age and gender are required' });
  }
  if (typeof username !== 'string' || username.length < 3 || username.length > 100) {
    return res.status(400).json({ error: 'username must contain 3 to 100 characters' });
  }
  if (typeof password !== 'string' || password.length < 12 || password.length > 128) {
    return res.status(400).json({ error: 'password must contain 12 to 128 characters' });
  }
  if (!Number.isInteger(age) || age < 0 || age > 120) {
    return res.status(400).json({ error: 'age must be an integer from 0 to 120' });
  }
  if (!['M', 'F'].includes(gender)) {
    return res.status(400).json({ error: 'gender must be M or F' });
  }
  if (email === undefined || (email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email))) {
    return res.status(400).json({ error: 'email must be valid when provided' });
  }
  if (phone === undefined || residence === undefined) {
    return res.status(400).json({ error: 'optional text fields are invalid' });
  }
  if (phone) {
    const normalized = phone.replace(/[^+0-9]/g, '');
    const digits = normalized.replace(/\D/g, '');
    if (digits.length < 7 || digits.length > 15) {
      return res.status(400).json({ error: 'phone must contain 7 to 15 digits' });
    }
    phone = normalized.startsWith('+') ? normalized : `+${normalized}`;
  }

  let birthDate = null;
  if (birth_date) {
    if (typeof birth_date !== 'string' || !/^\d{4}-\d{2}-\d{2}$/.test(birth_date)) {
      return res.status(400).json({ error: 'birth_date must use YYYY-MM-DD format' });
    }
    const parsed = new Date(birth_date);
    if (Number.isNaN(parsed.getTime()) || parsed.toISOString().slice(0, 10) !== birth_date || parsed > new Date()) {
      return res.status(400).json({ error: 'birth_date must be a valid past date' });
    }
    const calculatedAge = calculateAge(parsed);
    if (calculatedAge !== age) {
      return res.status(400).json({ error: 'age does not match birth_date' });
    }
    birthDate = parsed.toISOString().slice(0, 10);
  }

  try {
    const user = await authService.createUser({
      username,
      password,
      age,
      gender,
      email,
      phone,
      birthDate,
      residence,
    });
    return res.status(201).json(user);
  } catch (err) {
    if (err.status === 409 || err.code === '23505') {
      return res.status(409).json({ error: 'username or email already exists' });
    }
    console.error(err);
    return res.status(500).json({ error: 'Failed to create user' });
  }
}

async function updateUserDemographics(req, res) {
  const { age, gender } = req.body || {};
  if (!isUuid(req.params.id)) {
    return res.status(400).json({ error: 'user id must be a valid UUID' });
  }
  if (!Number.isInteger(age) || age < 0 || age > 120) {
    return res.status(400).json({ error: 'age must be an integer from 0 to 120' });
  }
  if (!['M', 'F'].includes(gender)) {
    return res.status(400).json({ error: 'gender must be M or F' });
  }

  try {
    const existing = await pool.query(
      `SELECT birth_date FROM users WHERE id = $1 AND role <> 'admin'`,
      [req.params.id]
    );
    if (existing.rows.length === 0) return res.status(404).json({ error: 'Patient not found' });
    if (existing.rows[0].birth_date && calculateAge(existing.rows[0].birth_date) !== age) {
      return res.status(400).json({ error: 'age does not match the stored birth_date' });
    }

    const { rows } = await pool.query(
      `UPDATE users
       SET age = $1, gender = $2, age_recorded_at = CURRENT_DATE
       WHERE id = $3 AND role <> 'admin'
       RETURNING id, username, email, role, gender, age, age_recorded_at, phone, birth_date, residence,
                 TRUE AS profile_complete`,
      [age, gender, req.params.id]
    );
    return res.json(rows[0]);
  } catch (err) {
    console.error(err);
    return res.status(500).json({ error: 'Failed to update patient demographics' });
  }
}

async function listUsers(req, res) {
  try {
    const { rows } = await pool.query(
      `SELECT u.id, u.username, u.email, u.role, u.gender, u.age, u.age_recorded_at,
              u.phone, u.birth_date, u.residence,
              COALESCE(antigen_data.antigens, '[]'::json) AS antigens,
              COALESCE(antigen_data.antigen_ids, ARRAY[]::uuid[]) AS antigen_ids,
              (u.gender IS NOT NULL AND u.age IS NOT NULL AND u.age_recorded_at IS NOT NULL) AS profile_complete
       FROM users u
       LEFT JOIN LATERAL (
         SELECT json_agg(json_build_object('id', antigen.id, 'name', antigen.name) ORDER BY antigen.name) AS antigens,
                array_agg(antigen.id ORDER BY antigen.name) AS antigen_ids
         FROM user_antigens assignment
         JOIN antigens antigen ON antigen.id = assignment.antigen_id
         WHERE assignment.user_id = u.id
       ) antigen_data ON TRUE
       WHERE u.role <> 'admin'
       ORDER BY u.username`
    );
    res.json(rows);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: 'Failed to load users' });
  }
}

async function listAntigens(req, res) {
  try {
    const { rows } = await pool.query(
      `SELECT antigen.id, antigen.name, antigen.created_at,
              COUNT(patient.id)::int AS assigned_user_count,
              COALESCE(
                json_agg(json_build_object('id', patient.id, 'username', patient.username) ORDER BY patient.username)
                  FILTER (WHERE patient.id IS NOT NULL),
                '[]'::json
              ) AS assigned_users
       FROM antigens antigen
       LEFT JOIN user_antigens assignment ON assignment.antigen_id = antigen.id
       LEFT JOIN users patient ON patient.id = assignment.user_id
       GROUP BY antigen.id
       ORDER BY antigen.name`
    );
    return res.json(rows);
  } catch (err) {
    console.error(err);
    return res.status(500).json({ error: 'Failed to load antigens' });
  }
}

async function createAntigen(req, res) {
  const name = typeof req.body?.name === 'string' ? req.body.name.trim() : '';
  if (!name || name.length > 100) {
    return res.status(400).json({ error: 'antigen name must contain 1 to 100 characters' });
  }

  try {
    const { rows } = await pool.query(
      `INSERT INTO antigens (name)
       VALUES ($1)
       RETURNING id, name, created_at`,
      [name]
    );
    return res.status(201).json({ ...rows[0], assigned_user_count: 0, assigned_users: [] });
  } catch (err) {
    if (err.code === '23505') {
      return res.status(409).json({ error: 'an antigen with this name already exists' });
    }
    console.error(err);
    return res.status(500).json({ error: 'Failed to create antigen' });
  }
}

async function updateUserAntigens(req, res) {
  if (!isUuid(req.params.id)) {
    return res.status(400).json({ error: 'user id must be a valid UUID' });
  }
  const antigenIds = req.body?.antigen_ids;
  if (!Array.isArray(antigenIds) || antigenIds.some((id) => !isUuid(id))) {
    return res.status(400).json({ error: 'antigen_ids must be an array of valid UUIDs' });
  }
  const uniqueAntigenIds = [...new Set(antigenIds)];

  const client = await pool.connect();
  try {
    await client.query('BEGIN');
    const patient = await client.query(
      `SELECT id FROM users WHERE id = $1 AND role <> 'admin' FOR UPDATE`,
      [req.params.id]
    );
    if (patient.rows.length === 0) {
      await client.query('ROLLBACK');
      return res.status(404).json({ error: 'Patient not found' });
    }
    if (uniqueAntigenIds.length > 0) {
      const antigens = await client.query(
        'SELECT id FROM antigens WHERE id = ANY($1::uuid[])',
        [uniqueAntigenIds]
      );
      if (antigens.rows.length !== uniqueAntigenIds.length) {
        await client.query('ROLLBACK');
        return res.status(404).json({ error: 'One or more antigens were not found' });
      }
    }

    await client.query('DELETE FROM user_antigens WHERE user_id = $1', [req.params.id]);
    if (uniqueAntigenIds.length > 0) {
      await client.query(
        `INSERT INTO user_antigens (user_id, antigen_id)
         SELECT $1, unnest($2::uuid[])`,
        [req.params.id, uniqueAntigenIds]
      );
    }
    const { rows } = await client.query(
      `SELECT u.id, u.username,
              COALESCE(
                json_agg(json_build_object('id', antigen.id, 'name', antigen.name) ORDER BY antigen.name)
                  FILTER (WHERE antigen.id IS NOT NULL),
                '[]'::json
              ) AS antigens,
              COALESCE(array_agg(antigen.id ORDER BY antigen.name) FILTER (WHERE antigen.id IS NOT NULL), ARRAY[]::uuid[]) AS antigen_ids
       FROM users u
       LEFT JOIN user_antigens assignment ON assignment.user_id = u.id
       LEFT JOIN antigens antigen ON antigen.id = assignment.antigen_id
       WHERE u.id = $1
       GROUP BY u.id`,
      [req.params.id]
    );
    await client.query('COMMIT');
    return res.json(rows[0]);
  } catch (err) {
    await client.query('ROLLBACK');
    console.error(err);
    return res.status(500).json({ error: 'Failed to assign antigens' });
  } finally {
    client.release();
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
  if (!isUuid(user_id) || system_ids.some((id) => !isUuid(id))) {
    return res.status(400).json({ error: 'user_id and system_ids must contain valid UUIDs' });
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
      `SELECT id, gender, age, age_recorded_at FROM users WHERE id = $1 AND role <> 'admin'`,
      [user_id]
    );
    if (target.rows.length === 0) {
      await client.query('ROLLBACK');
      return res.status(404).json({ error: 'Patient not found' });
    }
    if (!target.rows[0].gender || target.rows[0].age === null || !target.rows[0].age_recorded_at) {
      await client.query('ROLLBACK');
      return res.status(409).json({ error: 'Patient age and gender must be completed before assignment' });
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
  if (!isUuid(req.params.id)) {
    return res.status(400).json({ error: 'assignment id must be a valid UUID' });
  }
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
  if (!isUuid(user_id)) {
    return res.status(400).json({ error: 'user_id must be a valid UUID' });
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
  createAntigen,
  createAppointment,
  createUser,
  listAntigens,
  listAppointments,
  listQuestionnaireAssignments,
  listSystems,
  listUsers,
  updateUserAntigens,
  updateUserDemographics,
};
