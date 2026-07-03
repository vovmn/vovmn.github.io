export function validateUsername(username) {
  if (!username || !username.trim()) return 'Введите имя пользователя'
  if (username.trim().length < 3) return 'Минимум 3 символа'
  return null
}

export function validateEmail(email) {
  if (!email || !email.trim()) return 'Введите почту'

  const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  if (!re.test(email.trim())) return 'Неверный формат почты'

  return null
}

export function validatePassword(password) {
  if (!password) return 'Введите пароль'
  if (password.length < 6) return 'Пароль минимум 6 символов'
  return null
}

export function validatePhone(phone) {
  if (!phone) return null

  const digits = String(phone).replace(/[^0-9]/g, '')
  if (digits.length < 7 || digits.length > 15) return 'Некорректный номер телефона'

  return null
}

export function validateBirthDate(birthDate) {
  if (!birthDate) return null

  const date = new Date(birthDate)
  if (Number.isNaN(date.getTime())) return 'Некорректная дата'
  if (date > new Date()) return 'Дата не может быть в будущем'

  const age = Math.floor((Date.now() - date.getTime()) / (365.25 * 24 * 60 * 60 * 1000))
  if (age < 13) return 'Пользователь должен быть не младше 13 лет'

  return null
}

export default {
  validateBirthDate,
  validateEmail,
  validatePassword,
  validatePhone,
  validateUsername,
}
