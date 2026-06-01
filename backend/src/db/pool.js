const { Pool } = require('pg')

const config = {}

if (process.env.DATABASE_URL) {
  config.connectionString = process.env.DATABASE_URL
} else {
  config.host = process.env.DB_HOST
  config.port = process.env.DB_PORT
  config.user = process.env.DB_USER
  config.password = process.env.DB_PASSWORD
  config.database = process.env.DB_NAME
}

const pool = new Pool(config)
  host: process.env.DB_HOST,
  port: process.env.DB_PORT,
  user: process.env.DB_USER,
  password: process.env.DB_PASSWORD,
  database: process.env.DB_NAME,
})


module.exports = pool