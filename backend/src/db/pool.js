const { Pool } = require('pg')
console.log('ENV CHECK:', process.env.DB_PASSWORD)
const pool = new Pool({
  host: process.env.DB_HOST,
  port: process.env.DB_PORT,
  user: process.env.DB_USER,
  password: process.env.DB_PASSWORD,
  database: process.env.DB_NAME,
})


module.exports = pool