<template>
  <div class="container">
    <form @submit.prevent="onSubmit" class="auth_form">
      <h2 class="form_title">Регистрация</h2>

      <div class="data">
        <label for="username">Имя пользователя:</label>
        <input
          type="text"
          id="username"
          v-model.trim="form.username"
          placeholder="Введите имя пользователя"
          class="form_input"
        />
        <p v-if="errors.username" class="error">{{ errors.username }}</p>
      </div>

      <div class="data">
        <label for="email">Почта:</label>
        <input
          type="email"
          id="email"
          v-model.trim="form.email"
          placeholder="Введите почту"
          class="form_input"
        />
        <p v-if="errors.email" class="error">{{ errors.email }}</p>
      </div>

      <div class="data">
        <label for="phone">Телефон:</label>
        <input
          type="tel"
          id="phone"
          v-model="form.phone"
          @input="onPhoneInput"
          placeholder="+7 (999) 999-99-99"
          class="form_input"
        />
        <p v-if="errors.phone" class="error">{{ errors.phone }}</p>
      </div>

      <div class="data">
        <label for="birth">Дата рождения:</label>
        <input
          type="date"
          id="birth"
          v-model="form.birth_date"
          class="form_input"
        />
        <p v-if="errors.birth_date" class="error">{{ errors.birth_date }}</p>
      </div>

      <div class="data">
        <label for="residence">Место проживания:</label>
        <input
          type="text"
          id="residence"
          v-model.trim="form.residence"
          placeholder="Город, адрес"
          class="form_input"
        />
      </div>

      <div class="data">
        <label for="password">Пароль:</label>
        <input
          type="password"
          id="password"
          v-model="form.password"
          placeholder="Введите пароль"
          class="form_input"
        />
        <p v-if="errors.password" class="error">{{ errors.password }}</p>
      </div>

      <div class="data">
        <label for="repassword">Повторите пароль:</label>
        <input
          type="password"
          id="repassword"
          v-model="form.repassword"
          placeholder="Введите пароль еще раз"
          class="form_input"
        />
        <p v-if="errors.repassword" class="error">{{ errors.repassword }}</p>
      </div>

      <p v-if="errors.common" class="error common">{{ errors.common }}</p>

      <button type="submit" class="submit_btn" :disabled="loading">{{ loading ? 'Регистрация…' : 'Зарегистрироваться' }}</button>

      <div class="form_footer">
        <router-link to="/forgot">забыли пароль?</router-link>
        <p class="register_text">Уже есть аккаунт?<router-link to="/login">Войти</router-link></p>
      </div>
    </form>
  </div>
</template>

<script setup>
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { authApi } from '@/services/authApi'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const auth = useAuthStore()

const form = reactive({
  username: '',
  email: '',
  phone: '',
  birth_date: '',
  residence: '',
  password: '',
  repassword: '',
})

import { validateUsername, validateEmail, validatePassword, validatePhone, validateBirthDate } from '@/utils/validators'

const errors = reactive({ username: '', email: '', phone: '', birth_date: '', password: '', repassword: '', common: '' })
const loading = ref(false)

function formatPhone(digits) {
  // digits: only digits, possibly leading country code
  if (!digits) return ''
  // if starts with 8 -> convert to 7
  if (digits.length && digits[0] === '8') digits = '7' + digits.slice(1)
  // ensure leading +
  if (digits.length && digits[0] !== '+') digits = '+' + digits
  // remove non digits except leading +
  digits = digits.replace(/[^+0-9]/g, '')
  const num = digits.replace(/[^0-9]/g, '')
  // format as +X (XXX) XXX-XX-XX for up to 11-12 digits
  let out = digits[0] === '+' ? '+' : ''
  let rest = num
  if (rest.length > 0) {
    const cc = rest.slice(0, 1); rest = rest.slice(1); out += cc
  }
  if (rest.length > 0) {
    const part = rest.slice(0, 3); rest = rest.slice(3); out += ' (' + part + ')'
  }
  if (rest.length > 0) {
    const part = rest.slice(0, 3); rest = rest.slice(3); out += ' ' + part
  }
  if (rest.length > 0) {
    const part = rest.slice(0, 2); rest = rest.slice(2); out += '-' + part
  }
  if (rest.length > 0) {
    const part = rest.slice(0, 2); rest = rest.slice(2); out += '-' + part
  }
  return out.trim()
}

function onPhoneInput(e) {
  const raw = e.target.value || ''
  const digits = raw.replace(/[^0-9]/g, '')
  // build formatted string
  const formatted = formatPhone(digits)
  form.phone = formatted
}

function validate() {
  errors.common = ''
  // field-level validation using centralized validators
  errors.username = validateUsername(form.username)
  errors.email = validateEmail(form.email)
  errors.password = validatePassword(form.password)
  errors.repassword = form.password !== form.repassword ? 'Пароли не совпадают' : ''
  errors.phone = validatePhone(form.phone)
  errors.birth_date = validateBirthDate(form.birth_date)

  // overall required fields
  if (errors.username || errors.email || errors.password || errors.repassword || errors.phone || errors.birth_date) {
    errors.common = 'Проверьте поля и исправьте ошибки'
    return false
  }
  return true
}

async function onSubmit() {
  if (!validate()) return
  loading.value = true
  try {
    const payload = {
      username: form.username,
      email: form.email,
      password: form.password,
      phone: form.phone || null,
      birth_date: form.birth_date || null,
      residence: form.residence || null,
    }

    const res = await authApi.register(payload)

    // if backend returned access_token, use it and fetch /me
    if (res?.access_token) {
      auth.setAccessToken(res.access_token)
      await auth.fetchMe()
      router.replace('/home')
      return
    }

    // fallback: try login then fetchMe
    await authApi.login({ username: form.username, password: form.password })
    await auth.fetchMe()
    router.replace('/home')
  } catch (e) {
    errors.common = e?.response?.data?.error || e?.message || 'Ошибка регистрации'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.container {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  display: flex;
  justify-content: center;
  align-items: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 20px;
  margin: 0;
  box-sizing: border-box;
  overflow: auto;
}

.auth_form {
  background: white;
  padding: 1.75rem;
  border-radius: 12px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.12);
  width: 100%;
  max-width: 480px;
  max-height: calc(100vh - 80px);
  overflow: auto;
  backdrop-filter: blur(6px);
  border: 1px solid rgba(0, 0, 0, 0.06);
}

.form_title {
  text-align: center;
  margin-bottom: 2rem;
  color: #333;
  font-size: 1.8rem;
  font-weight: 600;
  background: linear-gradient(135deg,#324ece 0%, #2563eb 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.data {
  margin-bottom: 1.5rem;
}

label {
  display: block;
  margin-bottom: 0.5rem;
  color: #555;
  font-weight: 500;
  font-size: 0.9rem;
}

.form_input {
  width: 100%;
  padding: 0.6rem 0.9rem;
  border: 1.5px solid #e1e5e9;
  border-radius: 8px;
  font-size: 0.98rem;
  transition: border-color 0.18s ease, box-shadow 0.18s ease;
  background: #fbfdff;
  box-sizing: border-box;
}

.form_input:focus {
  outline: none;
  border-color: #667eea;
  background: white;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
  transform: translateY(-2px);
}

.form_input::placeholder {
  color: #a0a4a8;
}

.submit_btn {
  width: 100%;
  padding: 0.75rem;
  background: linear-gradient(135deg, #324ece 0%, #2563eb 100%);;
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 1rem;
  font-weight: 600;

  margin-top: 0.5rem;
  box-shadow: 0 4px 15px rgba(102, 126, 234, 0.3);
}

.submit_btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(102, 126, 234, 0.4);
}

.submit_btn:active {
  transform: translateY(0);
}

.submit_btn:focus {
  outline: none;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.3);
}

.form_footer {
  margin-top: 1.5rem;
  text-align: center;
}

.forgot_link {
  display: block;
  color: #667eea;
  text-decoration: none;
  font-size: 0.9rem;
  margin-bottom: 1rem;
  transition: color 0.3s ease;
}

.forgot_link:hover {
  color: #764ba2;
  text-decoration: underline;
}

.register_text {
  color: #666;
  font-size: 0.9rem;
  margin: 0;
  padding-top: 1rem;
  border-top: 1px solid #e1e5e9;
}

.register_link {
  color: #667eea;
  text-decoration: none;
  font-weight: 500;
  transition: color 0.3s ease;
}

.register_link:hover {
  color: #764ba2;
  text-decoration: underline;
}

/* Анимации */
.auth_form {
  animation: slideUp 0.5s ease-out;
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* Адаптивность */
@media (max-width: 480px) {
  .container { padding: 8px; }
  .auth_form { padding: 1rem; max-width: 100%; border-radius: 10px; }
  .form_title { font-size: 1.4rem; }
  .form_input { font-size: 0.95rem; padding: 0.55rem 0.8rem }
}

/* Дополнительные эффекты при валидации */
.form_input:invalid:not(:focus):not(:placeholder-shown) {
  border-color: #e74c3c;
}

.form_input:valid:not(:focus):not(:placeholder-shown) {
  border-color: #2ecc71;
}
</style>
