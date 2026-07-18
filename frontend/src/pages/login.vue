<template>
  <main class="auth-page">
    <section class="auth-shell">
      <div class="auth-intro">
        <span class="brand-mark">IM</span>
        <h1>Иммунолаб</h1>
        <p>Личный кабинет для медицинских анкет и данных пациента.</p>
      </div>

      <form class="auth-form" @submit.prevent="onSubmit" novalidate>
        <div class="form-head">
          <p>Добро пожаловать</p>
          <h2>Вход в аккаунт</h2>
        </div>

        <label class="field" for="username">
          <span>Имя пользователя</span>
          <input
            id="username"
            v-model.trim="form.username"
            type="text"
            autocomplete="username"
            :aria-invalid="!!errors.username"
            required
          />
          <small v-if="errors.username">{{ errors.username }}</small>
        </label>

        <label class="field" for="password">
          <span>Пароль</span>
          <input
            id="password"
            v-model="form.password"
            type="password"
            autocomplete="current-password"
            :aria-invalid="!!errors.password"
            required
          />
          <small v-if="errors.password">{{ errors.password }}</small>
        </label>

        <p v-if="errors.common" class="common-error">{{ errors.common }}</p>

        <button class="submit-btn" type="submit" :disabled="loading">
          {{ loading ? 'Входим...' : 'Войти' }}
        </button>

        <p class="switch-link">
          Нет аккаунта?
          <router-link to="/register">Зарегистрироваться</router-link>
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

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()

const form = reactive({
  username: '',
  password: '',
})

const errors = reactive({
  username: '',
  password: '',
  common: '',
})

const loading = ref(false)

function validate() {
  errors.username = ''
  errors.password = ''
  errors.common = ''

  if (!form.username) errors.username = 'Введите имя пользователя'
  if (!form.password) errors.password = 'Введите пароль'
  if (form.password && form.password.length < 6) errors.password = 'Минимум 6 символов'

  return !(errors.username || errors.password)
}

async function onSubmit() {
  if (!validate()) return

  loading.value = true
  try {
    const res = await authApi.login({
      username: form.username,
      password: form.password,
    })

    auth.setAccessToken(res.access_token)
    await auth.fetchMe()
    router.replace(typeof route.query.redirect === 'string' ? route.query.redirect : '/info')
  } catch (e) {
    errors.common =
      e?.response?.data?.error ||
      e?.response?.data?.message ||
      e?.message ||
      'Не удалось войти. Проверьте данные и попробуйте еще раз.'
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
  grid-template-columns: minmax(280px, 0.9fr) minmax(320px, 1fr);
  width: min(960px, 100%);
  overflow: hidden;
  border: 1px solid #dce8ea;
  border-radius: 8px;
  background: #fff;
  box-shadow: 0 24px 60px rgba(25, 45, 65, 0.12);
}

.auth-intro {
  display: flex;
  min-height: 520px;
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

.field {
  display: grid;
  gap: 0.45rem;
  margin-bottom: 1rem;
}

.field span {
  color: #34475c;
  font-size: 0.92rem;
  font-weight: 700;
}

.field input {
  width: 100%;
  min-height: 46px;
  padding: 0 0.85rem;
  border: 1px solid #d4e0e3;
  border-radius: 8px;
  background: #fbfdfd;
  outline: none;
}

.field input:focus {
  border-color: #1f847f;
  box-shadow: 0 0 0 3px rgba(31, 132, 127, 0.12);
}

.field small,
.common-error {
  color: #b42318;
  font-size: 0.84rem;
}

.common-error {
  margin: 0.2rem 0 1rem;
}

.submit-btn {
  width: 100%;
  min-height: 46px;
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

@media (max-width: 760px) {
  .auth-shell {
    grid-template-columns: 1fr;
  }

  .auth-intro {
    min-height: auto;
  }
}
</style>
