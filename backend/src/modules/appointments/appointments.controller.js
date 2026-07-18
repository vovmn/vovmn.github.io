const pool = require('../../db/pool');

async function myAppointments(req, res) {
  try {
    const { rows } = await pool.query(
      `SELECT id, scheduled_at, specialist, doctor, status, comment, created_at
       FROM appointments
       WHERE user_id = $1
       ORDER BY scheduled_at DESC`,
      [req.user.sub]
    );

    res.json(rows);
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: 'Failed to load appointments' });
  }
}

module.exports = { myAppointments };
