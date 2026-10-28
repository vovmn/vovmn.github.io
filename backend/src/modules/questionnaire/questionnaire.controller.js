const pool = require('../../db/pool');

function getAnswerValue(answer) {
  if (!answer) return undefined;
  if (answer.skipped === true) return 'skipped';
  if (answer.value_unknown === true) return 'unknown';
  if (Object.prototype.hasOwnProperty.call(answer, 'value_boolean')) return answer.value_boolean;
  if (Object.prototype.hasOwnProperty.call(answer, 'value_numeric')) return answer.value_numeric;
  if (Object.prototype.hasOwnProperty.call(answer, 'value_text')) return answer.value_text;
  if (Object.prototype.hasOwnProperty.call(answer, 'selected_option_id')) return answer.selected_option_id;
  if (Array.isArray(answer.selected_option_ids)) return answer.selected_option_ids;
  return undefined;
}

function conditionMatches(condition, answersByCode) {
  if (!condition) return true;
  if (Array.isArray(condition.all)) {
    return condition.all.every((item) => conditionMatches(item, answersByCode));
  }
  if (Array.isArray(condition.any)) {
    return condition.any.some((item) => conditionMatches(item, answersByCode));
  }

  const actual = getAnswerValue(answersByCode.get(condition.question_code));
  switch (condition.operator) {
    case 'equals': return actual === condition.value;
    case 'not_equals': return actual !== undefined && actual !== condition.value;
    case 'in': return Array.isArray(condition.values) && condition.values.includes(actual);
    default: return false;
  }
}

function hasAnswerValue(answer, question) {
  if (!answer) return false;
  if (answer.skipped === true) return true;
  if (answer.value_unknown === true) return question.allow_unknown === true;
  switch (question.question_type) {
    case 'boolean': return typeof answer.value_boolean === 'boolean';
    case 'numeric': return answer.value_numeric !== null
      && answer.value_numeric !== ''
      && Number.isFinite(Number(answer.value_numeric));
    case 'single_choice': return Boolean(answer.selected_option_id);
    case 'multiple_choice': return Array.isArray(answer.selected_option_ids) && answer.selected_option_ids.length > 0;
    case 'text': return typeof answer.value_text === 'string' && answer.value_text.trim().length > 0;
    default: return false;
  }
}

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
  const { assignmentId } = req.params;
  const userId = req.user.sub;
  const userGender = req.user.gender;

  try {
    const assignmentResult = await pool.query(
      `SELECT a.id, a.system_id, a.status, a.due_at, a.priority, a.note,
              s.code AS system_code, s.name AS system_name
       FROM user_questionnaire_assignments a
       JOIN body_systems s ON s.id = a.system_id
       WHERE a.id = $1 AND a.user_id = $2`,
      [assignmentId, userId]
    );
    if (assignmentResult.rows.length === 0) {
      return res.status(404).json({ error: 'Assignment not found' });
    }
    const assignment = assignmentResult.rows[0];
    if (assignment.status === 'cancelled') {
      return res.status(409).json({ error: 'Assignment is cancelled' });
    }

    await pool.query(
      `UPDATE user_questionnaire_assignments
       SET status = CASE WHEN status = 'pending' THEN 'in_progress' ELSE status END,
           started_at = COALESCE(started_at, NOW()), updated_at = NOW()
       WHERE id = $1`,
      [assignmentId]
    );

    const questionsResult = await pool.query(
      `SELECT q.id, q.code, q.question_text, q.question_type, q.sort_order, q.required,
              q.allow_unknown, q.help_text, q.recall_period, q.display_condition,
              q.validation, q.alert,
              COALESCE(json_agg(json_build_object('id', ao.id, 'text', ao.option_text, 'code', ao.option_code)
                       ORDER BY ao.sort_order) FILTER (WHERE ao.id IS NOT NULL), '[]') AS options
       FROM questions q
       LEFT JOIN answer_options ao ON ao.question_id = q.id
       WHERE q.system_id = $1
         AND q.active = TRUE
         AND (q.gender_filter IS NULL OR q.gender_filter = $2)
       GROUP BY q.id
       ORDER BY q.sort_order`,
      [assignment.system_id, userGender]
    );

    res.json({
      assignment_id: assignment.id,
      system_code: assignment.system_code,
      system_name: assignment.system_name,
      due_at: assignment.due_at,
      priority: assignment.priority,
      note: assignment.note,
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
      'SELECT id, user_id, system_id, status FROM user_questionnaire_assignments WHERE id = $1',
      [assignmentId]
    );
    if (assignCheck.rows.length === 0 || String(assignCheck.rows[0].user_id) !== String(userId)) {
      await client.query('ROLLBACK');
      return res.status(403).json({ error: 'Forbidden' });
    }
    if (assignCheck.rows[0].status === 'cancelled') {
      await client.query('ROLLBACK');
      return res.status(409).json({ error: 'Assignment is cancelled' });
    }

    if (answers.some((ans) => !ans?.question_id)) {
      await client.query('ROLLBACK');
      return res.status(400).json({ error: 'answers must contain question_id' });
    }

    const questionIds = answers.map((ans) => String(ans.question_id));
    if (new Set(questionIds).size !== questionIds.length) {
      await client.query('ROLLBACK');
      return res.status(400).json({ error: 'answers contain duplicate question_id values' });
    }

    const questionsResult = await client.query(
      `SELECT q.id, q.code, q.question_type, q.required, q.allow_unknown,
              q.display_condition, q.validation,
              COALESCE(json_agg(json_build_object('id', ao.id, 'code', ao.option_code)
                       ORDER BY ao.sort_order) FILTER (WHERE ao.id IS NOT NULL), '[]') AS options
       FROM questions q
       LEFT JOIN answer_options ao ON ao.question_id = q.id
       WHERE q.system_id = $1
         AND q.active = TRUE
         AND (q.gender_filter IS NULL OR q.gender_filter = $2)
       GROUP BY q.id
       ORDER BY q.sort_order`,
      [assignCheck.rows[0].system_id, req.user.gender]
    );

    const questionsById = new Map(questionsResult.rows.map((q) => [String(q.id), q]));
    const answersByCode = new Map();
    for (const answer of answers) {
      const question = questionsById.get(String(answer.question_id));
      if (!question) {
        await client.query('ROLLBACK');
        return res.status(400).json({ error: 'answers contain a question outside this questionnaire' });
      }
      answersByCode.set(question.code, answer);
    }

    const visibleQuestions = questionsResult.rows.filter((q) => conditionMatches(q.display_condition, answersByCode));
    const visibleIds = new Set(visibleQuestions.map((q) => String(q.id)));
    if (answers.some((answer) => !visibleIds.has(String(answer.question_id)))) {
      await client.query('ROLLBACK');
      return res.status(400).json({ error: 'answers contain a hidden or inapplicable question' });
    }

    for (const question of visibleQuestions) {
      const answer = answers.find((item) => String(item.question_id) === String(question.id));
      if (question.required && !hasAnswerValue(answer, question)) {
        await client.query('ROLLBACK');
        return res.status(400).json({ error: `required question is unanswered: ${question.code}` });
      }
      if (!answer) continue;
      if (answer.skipped === true) {
        const allowedReasons = new Set(['unknown', 'not_applicable', 'prefer_not_to_answer', 'other']);
        if (!allowedReasons.has(answer.skip_reason)) {
          await client.query('ROLLBACK');
          return res.status(400).json({ error: `invalid skip reason: ${question.code}` });
        }
        continue;
      }
      if (answer.value_unknown === true) {
        if (!question.allow_unknown) {
          await client.query('ROLLBACK');
          return res.status(400).json({ error: `unknown is not allowed for question: ${question.code}` });
        }
        continue;
      }

      if (question.question_type === 'boolean' && typeof answer.value_boolean !== 'boolean') {
        await client.query('ROLLBACK');
        return res.status(400).json({ error: `boolean answer expected: ${question.code}` });
      }
      if (question.question_type === 'numeric') {
        const value = Number(answer.value_numeric);
        const rules = question.validation || {};
        if (!Number.isFinite(value)
          || (rules.min !== undefined && value < rules.min)
          || (rules.max !== undefined && value > rules.max)) {
          await client.query('ROLLBACK');
          return res.status(400).json({ error: `numeric answer is out of range: ${question.code}` });
        }
      }
      if (question.question_type === 'text'
        && (typeof answer.value_text !== 'string' || !answer.value_text.trim())) {
        await client.query('ROLLBACK');
        return res.status(400).json({ error: `text answer expected: ${question.code}` });
      }

      const optionIds = new Set(question.options.map((option) => String(option.id)));
      if (question.question_type === 'single_choice'
        && !optionIds.has(String(answer.selected_option_id))) {
        await client.query('ROLLBACK');
        return res.status(400).json({ error: `invalid option for question: ${question.code}` });
      }
      if (question.question_type === 'multiple_choice') {
        if (!Array.isArray(answer.selected_option_ids)
          || answer.selected_option_ids.length === 0
          || new Set(answer.selected_option_ids.map(String)).size !== answer.selected_option_ids.length
          || answer.selected_option_ids.some((id) => !optionIds.has(String(id)))) {
          await client.query('ROLLBACK');
          return res.status(400).json({ error: `invalid options for question: ${question.code}` });
        }
      }
    }

    await client.query(
      'DELETE FROM user_answers WHERE user_id = $1 AND assignment_id = $2',
      [userId, assignmentId]
    );

    for (const ans of answers) {
      const selectedOptionIds = Array.isArray(ans.selected_option_ids)
        ? ans.selected_option_ids
        : ans.selected_option_id
          ? [ans.selected_option_id]
          : [null];

      for (const selectedOptionId of selectedOptionIds) {
        await client.query(
          `INSERT INTO user_answers (user_id, question_id, assignment_id,
                  value_boolean, value_numeric, value_text, value_unknown,
                  skipped, skip_reason, selected_option_id)
           VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10)`,
          [
            userId,
            ans.question_id,
            assignmentId,
            Object.prototype.hasOwnProperty.call(ans, 'value_boolean') ? ans.value_boolean : null,
            Object.prototype.hasOwnProperty.call(ans, 'value_numeric') ? ans.value_numeric : null,
            Object.prototype.hasOwnProperty.call(ans, 'value_text') ? ans.value_text : null,
            ans.value_unknown === true,
            ans.skipped === true,
            ans.skipped === true ? ans.skip_reason : null,
            selectedOptionId
          ]
        );
      }
    }

    await client.query(
      `UPDATE user_questionnaire_assignments
       SET status = $1, completed_at = NOW(), updated_at = NOW()
       WHERE id = $2`,
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
      `SELECT a.id, a.status, a.priority, a.due_at, a.note,
              a.assigned_at, a.started_at, a.completed_at, a.cancelled_at,
              s.code, s.name,
              COUNT(ua.id)::int AS answer_count,
              COUNT(ua.id) FILTER (WHERE ua.skipped)::int AS skipped_count
       FROM user_questionnaire_assignments a
       JOIN body_systems s ON s.id = a.system_id
       LEFT JOIN user_answers ua ON ua.assignment_id = a.id
       WHERE a.user_id = $1
       GROUP BY a.id, s.id
       ORDER BY
         CASE a.status WHEN 'pending' THEN 1 WHEN 'in_progress' THEN 2 WHEN 'completed' THEN 3 ELSE 4 END,
         a.due_at NULLS LAST, a.assigned_at DESC`,
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
