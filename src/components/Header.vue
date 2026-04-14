<template>
  <header class="header">
    <h1 class="logo">Имулаб</h1>

    <nav class="nav">
      <router-link to="/home" class="nav-link">Главная</router-link>
      <router-link to="/info" class="nav-link">Мои данные</router-link>
      <router-link to="/documents" class="nav-link">Анализы и выписки</router-link>
      <router-link to="/record" class="nav-link">Запись на прием</router-link>
    </nav>

    <div class="usermenu">
      <router-link to="/info" v-if="auth.user" class="username">
        {{ auth.user.username }}
      </router-link>
      <button v-if="auth.accessToken" @click="handleLogout" class="logout-btn">
        Выйти
      </button>
      <router-link v-else to="/login" class="login-link">Войти</router-link>
    </div>
  </header>
</template>

<script setup>
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const auth = useAuthStore()

async function handleLogout() {
  await auth.logout()
  router.push('/login')
}
</script>

<style scoped>
.usermenu {
  display: flex;
  gap: 1rem;
  align-items: center;
}
.nav {
  display: flex;
  gap: 1.75rem;
  align-items: center;
}

.logo {
  font-weight: 700;
  color: #2563eb;
  font-size: 1.9rem;
}

.header {
  background: white;
  padding: 1rem 2rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
}

.nav-link,
.login-link,
.username {
  text-decoration: none;
  color: #39404a;
  font-weight: 600;
}

.nav-link {
  padding: 0.35rem 0.5rem;
  transition: color 0.2s ease, background 0.2s ease;
}

.nav-link:hover,
.username:hover,
.login-link:hover {
  color: #2563eb;
}

.router-link-active {
  color: #2563eb;
}

.logout-btn {
  background: none;
  border: 1px solid #2563eb;
  color: #2563eb;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  cursor: pointer;
  font-weight: 500;
  transition: all 0.2s ease;
}

.logout-btn:hover {
  background: #2563eb;
  color: white;
}

.username {
  text-decoration: none;
  color: #333;
  cursor: pointer;
}

.username:hover {
  color: #2563eb;
}

.logout-btn {
  background: none;
  border: 1px solid #2563eb;
  color: #2563eb;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  cursor: pointer;
  font-weight: 500;
  transition: all 0.3s;
}

.logout-btn:hover {
  background: #2563eb;
  color: white;
}

.login-link {
  text-decoration: none;
  color: #2563eb;
  font-weight: 500;
  cursor: pointer;
}
</style>
