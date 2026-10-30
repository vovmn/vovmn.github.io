const crypto = require('crypto');
const argon2 = require('argon2');
const { Pool } = require('pg');

const connectionString = process.env.DATABASE_URL;
const username = (process.env.ADMIN_USERNAME || 'Administrator').trim();

if (!connectionString) {
  console.error('DATABASE_URL is required');
  process.exit(1);
}
if (!/^[A-Za-z0-9_.-]{3,100}$/.test(username)) {
  console.error('ADMIN_USERNAME must contain 3 to 100 safe characters');
  process.exit(1);
}

function generatePassword() {
  return `Aa9!${crypto.randomBytes(24).toString('base64url')}`;
}

async function main() {
  const passwordWasProvided = Boolean(process.env.ADMIN_PASSWORD);
  const password = process.env.ADMIN_PASSWORD || generatePassword();
  const hasRequiredClasses = /[a-z]/.test(password)
    && /[A-Z]/.test(password)
    && /\d/.test(password)
    && /[^A-Za-z0-9]/.test(password);
  if (password.length < 16 || password.length > 128 || !hasRequiredClasses) {
    throw new Error('ADMIN_PASSWORD must contain 16 to 128 characters, upper/lowercase letters, a digit and a symbol');
  }

  const pool = new Pool({ connectionString });
  try {
    const passwordHash = await argon2.hash(password);
    const { rows } = await pool.query(
      `INSERT INTO users (username, email, password_hash, role)
       VALUES ($1, NULL, $2, 'admin')
       ON CONFLICT (username) DO NOTHING
       RETURNING id, username`,
      [username, passwordHash]
    );
    if (rows.length === 0) {
      throw new Error(`User ${username} already exists; password was not changed`);
    }

    console.log(JSON.stringify({
      username,
      ...(passwordWasProvided ? {} : { password }),
      id: rows[0].id,
    }));
  } finally {
    await pool.end();
  }
}

main().catch((error) => {
  console.error(error.message);
  process.exit(1);
});
