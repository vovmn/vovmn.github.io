const path = require('path')
require('dotenv').config({ path: path.resolve(__dirname, '..', '.env') })

if (!process.env.JWT_ACCESS_SECRET || Buffer.byteLength(process.env.JWT_ACCESS_SECRET) < 32) {
  throw new Error('JWT_ACCESS_SECRET must contain at least 32 bytes')
}

const app = require('./app')

const PORT = process.env.PORT || 5000

app.listen(PORT, () => {
  console.log(`Server running on http://localhost:${PORT}`)
})
