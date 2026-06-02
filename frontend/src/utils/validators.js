export function validateUsername(username) {
  if (!username || !username.trim()) return 'Введите имя пользователя'
  if (username.trim().length < 3) return 'Минимум 3 символа'
  return null
}

export function validateEmail(email) {
  if (!email || !email.trim()) return 'Введите почту'
  // simple email regex
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
  if (!phone) return null // optional
  const digits = ('' + phone).replace(/[^0-9]/g, '')
  if (digits.length < 7 || digits.length > 15) return 'Некорректный номер телефона'
  return null
}

export function validateBirthDate(birth_date) {
  if (!birth_date) return null // optional
  const d = new Date(birth_date)
  if (Number.isNaN(d.getTime())) return 'Некорректная дата'
  const now = new Date()
  if (d > now) return 'Дата не может быть в будущем'
  const age = Math.floor((now - d) / (365.25 * 24 * 60 * 60 * 1000))
  if (age < 13) return 'Пользователь должен быть не младше 13 лет'
  return null
}

export default { validateUsername, validateEmail, validatePassword, validatePhone, validateBirthDate }
