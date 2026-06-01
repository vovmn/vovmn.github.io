const path = require('path')
require('dotenv').config({ path: path.resolve(__dirname, '..', '.env') })
console.log('CWD:', process.cwd())
console.log('ENV DB_PASSWORD:', process.env.DB_PASSWORD)

const app = require('./app')

const PORT = process.env.PORT || 5000

app.listen(PORT, () => {
  console.log(`Server running on http://localhost:${PORT}`)
})