<template>
  <div class="container">
    <form @submit.prevent="login" class="auth_form">
      <h2 class="form_title">Регистрация</h2>

      <div class="data">
        <label for="username">Имя пользователя:</label>
        <input
          type="text"
          id="username"
          v-model="username"
          placeholder="Введите имя пользователя"
          class="form_input"
        />
      </div>

        <div class="data">
        <label for="email">Почта:</label>
        <input
          type="text"
          id="Email"
          v-model="username"
          placeholder="Введите Почту"
          class="form_input"
        />
      </div>


      <div class="data">
        <label for="password">Пароль:</label>
        <input
          type="password"
          id="password"
          v-model="password"
          placeholder="Введите пароль"
          class="form_input"
        />
      </div>

          <div class="data">
        <label for="repassword">Пароль:</label>
        <input
          type="password"
          id="repassword"
          v-model="password"
          placeholder="Введите пароль еще раз"
          class="form_input"
        />
      </div>

      <button type="submit" class="submit_btn">Войти</button>

      <div class="form_footer">
        <router-link>забыли пароль?</router-link>
        <p class="register_text">Уже есть аккаунт?<router-link to="/Autorization">Войти</router-link></p>
      </div>
    </form>
  </div>
</template>

<script>
export default {
  data() {
    return {
      username: '',
      password: '',
      repassword: '',
      email: ''
    }
  },
  methods: {
    login() {
      console.log('Пользователь авторизован:', this.username)
      // код отправка на серв
    },
  },
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
