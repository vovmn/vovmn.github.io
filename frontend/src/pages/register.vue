<template>
  <main class="auth-page">
    <section class="auth-shell">
      <div class="auth-intro">
        <span class="brand-mark">IM</span>
        <h1>Иммунолаб</h1>
        <p>Создайте профиль пациента, чтобы проходить медицинские анкеты и сохранять ответы.</p>
      </div>

      <form class="auth-form" @submit.prevent="onSubmit" novalidate>
        <div class="form-head">
          <p>Новый профиль</p>
          <h2>Регистрация</h2>
        </div>

        <div class="fields-grid">
          <label class="field" for="username">
            <span>Имя пользователя</span>
            <input id="username" v-model.trim="form.username" type="text" autocomplete="username" />
            <small v-if="errors.username">{{ errors.username }}</small>
          </label>

          <label class="field" for="email">
            <span>Почта</span>
            <input id="email" v-model.trim="form.email" type="email" autocomplete="email" />
            <small v-if="errors.email">{{ errors.email }}</small>
          </label>

          <label class="field" for="phone">
            <span>Телефон</span>
            <input
              id="phone"
              v-model="form.phone"
              type="tel"
              autocomplete="tel"
              placeholder="+7 (999) 999-99-99"
              @input="onPhoneInput"
            />
            <small v-if="errors.phone">{{ errors.phone }}</small>
          </label>

          <label class="field" for="birth">
            <span>Дата рождения</span>
            <input id="birth" v-model="form.birth_date" type="date" />
            <small v-if="errors.birth_date">{{ errors.birth_date }}</small>
          </label>

          <label class="field" for="gender">
            <span>Пол</span>
            <select id="gender" v-model="form.gender">
              <option value="">Не указан</option>
              <option value="F">Женский</option>
              <option value="M">Мужской</option>
            </select>
            <small v-if="errors.gender">{{ errors.gender }}</small>
          </label>

          <label class="field" for="residence">
            <span>Место проживания</span>
            <input id="residence" v-model.trim="form.residence" type="text" placeholder="Город" />
          </label>

          <label class="field" for="password">
            <span>Пароль</span>
            <input id="password" v-model="form.password" type="password" autocomplete="new-password" />
            <small v-if="errors.password">{{ errors.password }}</small>
          </label>

          <label class="field" for="repassword">
            <span>Повторите пароль</span>
            <input id="repassword" v-model="form.repassword" type="password" autocomplete="new-password" />
            <small v-if="errors.repassword">{{ errors.repassword }}</small>
          </label>
        </div>

        <p v-if="errors.common" class="common-error">{{ errors.common }}</p>

        <button type="submit" class="submit-btn" :disabled="loading">
          {{ loading ? 'Создаем профиль...' : 'Зарегистрироваться' }}
        </button>

        <p class="switch-link">
          Уже есть аккаунт?
          <router-link to="/login">Войти</router-link>
        </p>
      </form>
    </section>
  </main>
</template>

<script setup>
import { reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { authApi } from '@/services/authApi'
import { useAuthStore } from '@/stores/auth'
import {
  validateBirthDate,
  validateEmail,
  validatePassword,
  validatePhone,
  validateUsername,
} from '@/utils/validators'

const router = useRouter()
const route = useRoute()
const auth = useAuthStore()

const form = reactive({
  username: '',
  email: '',
  phone: '',
  birth_date: '',
  gender: '',
  residence: '',
  password: '',
  repassword: '',
})

const errors = reactive({
  username: '',
  email: '',
  phone: '',
  birth_date: '',
  gender: '',
  password: '',
  repassword: '',
  common: '',
})

const loading = ref(false)

function formatPhone(digits) {
  if (!digits) return ''
  if (digits[0] === '8') digits = '7' + digits.slice(1)

  let rest = digits
  let out = '+'
  if (rest.length > 0) {
    out += rest.slice(0, 1)
    rest = rest.slice(1)
  }
  if (rest.length > 0) {
    out += ` (${rest.slice(0, 3)}`
    rest = rest.slice(3)
    if (out.includes('(') && out.length >= 6) out += ')'
  }
  if (rest.length > 0) {
    out += ` ${rest.slice(0, 3)}`
    rest = rest.slice(3)
  }
  if (rest.length > 0) {
    out += `-${rest.slice(0, 2)}`
    rest = rest.slice(2)
  }
  if (rest.length > 0) {
    out += `-${rest.slice(0, 2)}`
  }
  return out.trim()
}

function onPhoneInput(e) {
  const digits = (e.target.value || '').replace(/[^0-9]/g, '')
  form.phone = formatPhone(digits)
}

function validate() {
  errors.common = ''
  errors.username = validateUsername(form.username)
  errors.email = validateEmail(form.email)
  errors.password = validatePassword(form.password)
  errors.repassword = form.password !== form.repassword ? 'Пароли не совпадают' : ''
  errors.phone = validatePhone(form.phone)
  errors.birth_date = validateBirthDate(form.birth_date)
  errors.gender = form.gender && !['M', 'F'].includes(form.gender) ? 'Выберите пол из списка' : ''

  if (
    errors.username ||
    errors.email ||
    errors.password ||
    errors.repassword ||
    errors.phone ||
    errors.birth_date ||
    errors.gender
  ) {
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
      gender: form.gender || null,
    }

    const res = await authApi.register(payload)
    if (res?.access_token) {
      auth.setAccessToken(res.access_token)
      await auth.fetchMe()
      router.replace(typeof route.query.redirect === 'string' ? route.query.redirect : '/info')
      return
    }

    const login = await authApi.login({ username: form.username, password: form.password })
    auth.setAccessToken(login.access_token)
    await auth.fetchMe()
    router.replace(typeof route.query.redirect === 'string' ? route.query.redirect : '/info')
  } catch (e) {
    errors.common = e?.response?.data?.error || e?.message || 'Ошибка регистрации'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.auth-page {
  display: grid;
  min-height: 100vh;
  place-items: center;
  padding: 1rem;
}

.auth-shell {
  display: grid;
  grid-template-columns: minmax(280px, 0.78fr) minmax(360px, 1.22fr);
  width: min(1080px, 100%);
  overflow: hidden;
  border: 1px solid #dce8ea;
  border-radius: 8px;
  background: #fff;
  box-shadow: 0 24px 60px rgba(25, 45, 65, 0.12);
}

.auth-intro {
  display: flex;
  min-height: 620px;
  flex-direction: column;
  justify-content: flex-end;
  padding: 2rem;
  background:
    linear-gradient(180deg, rgba(31, 132, 127, 0.9), rgba(18, 99, 95, 0.96)),
    #1f847f;
  color: #fff;
}

.brand-mark {
  display: grid;
  width: 48px;
  height: 48px;
  place-items: center;
  margin-bottom: 1rem;
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.18);
  font-weight: 900;
}

.auth-intro h1 {
  margin: 0 0 0.7rem;
  font-size: 2.4rem;
}

.auth-intro p {
  max-width: 360px;
  margin: 0;
  color: rgba(255, 255, 255, 0.82);
  line-height: 1.6;
}

.auth-form {
  padding: 2rem;
}

.form-head p {
  margin: 0 0 0.35rem;
  color: #1f847f;
  font-size: 0.82rem;
  font-weight: 800;
  text-transform: uppercase;
}

.form-head h2 {
  margin: 0 0 1.5rem;
  color: #172338;
  font-size: 1.8rem;
}

.fields-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1rem;
}

.field {
  display: grid;
  gap: 0.45rem;
}

.field span {
  color: #34475c;
  font-size: 0.92rem;
  font-weight: 700;
}

.field input,
.field select {
  width: 100%;
  min-height: 46px;
  padding: 0 0.85rem;
  border: 1px solid #d4e0e3;
  border-radius: 8px;
  background: #fbfdfd;
  color: #172338;
  outline: none;
}

.field input:focus,
.field select:focus {
  border-color: #1f847f;
  box-shadow: 0 0 0 3px rgba(31, 132, 127, 0.12);
}

.field small,
.common-error {
  color: #b42318;
  font-size: 0.84rem;
}

.common-error {
  margin: 1rem 0 0;
}

.submit-btn {
  width: 100%;
  min-height: 46px;
  margin-top: 1.1rem;
  border: 0;
  border-radius: 8px;
  background: #1f847f;
  color: #fff;
  font-weight: 800;
}

.submit-btn:hover {
  background: #176b67;
}

.submit-btn:disabled {
  background: #9ab6b4;
}

.switch-link {
  margin: 1.2rem 0 0;
  color: #607184;
  text-align: center;
}

.switch-link a {
  color: #12635f;
  font-weight: 800;
}

@media (max-width: 820px) {
  .auth-shell {
    grid-template-columns: 1fr;
  }

  .auth-intro {
    min-height: auto;
  }
}

@media (max-width: 560px) {
  .fields-grid {
    grid-template-columns: 1fr;
  }

  .auth-form,
  .auth-intro {
    padding: 1.25rem;
  }
}
</style>
