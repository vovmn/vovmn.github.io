<template>
  <div class="container">
    <form class="auth_form" @submit.prevent="onSubmit" novalidate>
      <h2 class="form_title">Авторизация</h2>

      <div class="data">
        <label for="username">Имя пользователя</label>
        <input
          id="username"
          v-model.trim="form.username"
          class="form_input"
          type="text"
          autocomplete="username"
          :aria-invalid="!!errors.username"
          required
        />
        <p v-if="errors.username" class="error">{{ errors.username }}</p>
      </div>

      <div class="data">
        <label for="password">Пароль</label>
        <input
          id="password"
          v-model="form.password"
          class="form_input"
          type="password"
          autocomplete="current-password"
          :aria-invalid="!!errors.password"
          required
          minlength="6"
        />
        <p v-if="errors.password" class="error">{{ errors.password }}</p>
      </div>

      <button class="submit_btn" type="submit" :disabled="loading">
        {{ loading ? 'Входим…' : 'Войти' }}
      </button>

      <p v-if="errors.common" class="error common">{{ errors.common }}</p>

      <div class="form_footer">
        <router-link to="/forgot">забыли пароль?</router-link>
        <p class="register_text">
          Нет аккаунта?
          <router-link to="/register">Зарегистрироваться</router-link>
        </p>
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

    // если сервер возвращает access_token:
    auth.setAccessToken(res.access_token)
    await auth.fetchMe() // /me

    router.replace('/home')
  } catch (e) {
    // аккуратное извлечение текста ошибки:
    errors.common =
      e?.response?.data?.message ||
      e?.message ||
      'Не удалось войти. Проверь данные и попробуй ещё раз.'
  } finally {
    loading.value = false
  }
}
</script>
<style scoped>
.error {
  margin-top: 0.5rem;
  font-size: 0.85rem;
  color: #e74c3c;
}
.common {
  text-align: center;
  margin-top: 1rem;
}

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
  padding: 2.5rem;
  border-radius: 16px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
  width: 100%;
  max-width: 420px;
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.form_title {
  text-align: center;
  margin-bottom: 2rem;
  color: #333;
  font-size: 1.8rem;
  font-weight: 600;
  background: linear-gradient(135deg, #324ece 0%, #2563eb 100%);
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
  padding: 0.75rem 1rem;
  border: 2px solid #e1e5e9;
  border-radius: 8px;
  font-size: 1rem;
  transition: all 0.3s ease;
  background: #f8f9fa;
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
  background: linear-gradient(135deg, #324ece 0%, #2563eb 100%);
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
  .container {
    padding: 10px;
  }

  .auth_form {
    padding: 2rem 1.5rem;
  }

  .form_title {
    font-size: 1.5rem;
  }
}

/* Дополнительные эффекты при валидации */
.form_input:invalid:not(:focus):not(:placeholder-shown) {
  border-color: #e74c3c;
}

.form_input:valid:not(:focus):not(:placeholder-shown) {
  border-color: #2ecc71;
}
</style>
