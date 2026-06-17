const pool = require('../../db/pool');

async function getSystems(req, res) {
  try {
    const result = await pool.query('SELECT id, code, name FROM body_systems ORDER BY name');
    res.json(result.rows);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: 'Server error' });
  }
}

async function startQuestionnaire(req, res) {
  const { systemCode } = req.params;
  const userId = req.user.sub;
  const userGender = req.user.gender;

  try {
    const sysResult = await pool.query('SELECT id, name FROM body_systems WHERE code = $1', [systemCode]);
    if (sysResult.rows.length === 0) {
      return res.status(404).json({ error: 'System not found' });
    }
    const system = sysResult.rows[0];

    const assignResult = await pool.query(
      `INSERT INTO user_questionnaire_assignments (user_id, system_id, status)
       VALUES ($1, $2, 'in_progress')
       ON CONFLICT (user_id, system_id) DO UPDATE SET status = 'in_progress', assigned_at = NOW()
       RETURNING id`,
      [userId, system.id]
    );
    const assignmentId = assignResult.rows[0].id;

    const questionsResult = await pool.query(
      `SELECT q.id, q.question_text, q.question_type, q.sort_order, q.required,
              COALESCE(json_agg(json_build_object('id', ao.id, 'text', ao.option_text, 'code', ao.option_code)
                       ORDER BY ao.sort_order) FILTER (WHERE ao.id IS NOT NULL), '[]') AS options
       FROM questions q
       LEFT JOIN answer_options ao ON ao.question_id = q.id
       WHERE q.system_id = $1
         AND (q.gender_filter IS NULL OR q.gender_filter = $2)
       GROUP BY q.id
       ORDER BY q.sort_order`,
      [system.id, userGender]
    );

    res.json({
      assignment_id: assignmentId,
      system_name: system.name,
      questions: questionsResult.rows
    });
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: 'Server error' });
  }
}

async function submitAnswers(req, res) {
  const { assignmentId } = req.params;
  const { answers } = req.body;
  const userId = req.user.sub;

  if (!Array.isArray(answers)) {
    return res.status(400).json({ error: 'answers must be an array' });
  }

  const client = await pool.connect();
  try {
    await client.query('BEGIN');

    const assignCheck = await client.query(
      'SELECT id, user_id, system_id FROM user_questionnaire_assignments WHERE id = $1',
      [assignmentId]
    );
    if (assignCheck.rows.length === 0 || assignCheck.rows[0].user_id !== userId) {
      await client.query('ROLLBACK');
      return res.status(403).json({ error: 'Forbidden' });
    }

    for (const ans of answers) {
      await client.query(
        `INSERT INTO user_answers (user_id, question_id, assignment_id,
                value_boolean, value_numeric, value_text, selected_option_id)
         VALUES ($1, $2, $3, $4, $5, $6, $7)
         ON CONFLICT (user_id, question_id, selected_option_id)
         DO UPDATE SET value_boolean = EXCLUDED.value_boolean,
                       value_numeric = EXCLUDED.value_numeric,
                       value_text = EXCLUDED.value_text,
                       selected_option_id = EXCLUDED.selected_option_id`,
        [
          userId,
          ans.question_id,
          assignmentId,
          ans.value_boolean || null,
          ans.value_numeric || null,
          ans.value_text || null,
          ans.selected_option_id || null
        ]
      );
    }

    await client.query(
      'UPDATE user_questionnaire_assignments SET status = $1, completed_at = NOW() WHERE id = $2',
      ['completed', assignmentId]
    );

    await client.query('COMMIT');
    res.json({ status: 'ok' });
  } catch (err) {
    await client.query('ROLLBACK');
    console.error(err);
    res.status(500).json({ error: 'Submission failed' });
  } finally {
    client.release();
  }
}

async function myAssignments(req, res) {
  const userId = req.user.sub;
  try {
    const result = await pool.query(
      `SELECT a.id, a.status, a.assigned_at, a.completed_at,
              s.code, s.name
       FROM user_questionnaire_assignments a
       JOIN body_systems s ON s.id = a.system_id
       WHERE a.user_id = $1
       ORDER BY a.assigned_at DESC`,
      [userId]
    );
    res.json(result.rows);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: 'Server error' });
  }
}

module.exports = {
  getSystems,
  startQuestionnaire,
  submitAnswers,
  myAssignments
};