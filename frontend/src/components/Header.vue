<template>
  <header class="header">
    <h1 class="logo">Имулаб</h1>

    <nav class="nav">
      <router-link to="/home">
        <ul class="side">
          <b>Главная</b>
        </ul>
      </router-link>

      <router-link to="/info">
        <ul class="side">
          <b>Мои данные</b>
        </ul>
      </router-link>

      <router-link to="/documents">
        <ul class="side">
          <b>Анализы и выписки</b>
        </ul>
      </router-link>

      <router-link to="/record">
        <ul class="side">
          <b>Запись на прием</b>
        </ul>
      </router-link>
    </nav>

    <div class="usermenu">
      <span>
        <router-link v-if="!auth.user" to="/login">Войти</router-link>
        <span v-else>{{ auth.user.username }}</span>
      </span>
      <b v-if="auth.user" @click="logout">Выйти</b>
    </div>
  </header>
</template>

<script setup>
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const router = useRouter()

function logout() {
  auth.logout()
  router.replace('/login')
}
</script>

<style scoped>
.usermenu {
  display: flex;
  gap: 1rem;
}
.nav {
  display: flex;
  gap: 2rem;
}

.logo {
  font-weight: bold;
  color: #2563eb;
  font-size: 200%;
}

.header {
  background: white;
  padding: 1rem 2rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
}

.side {
  font-size: larger;
  margin: 0;
  padding: 0;
  list-style: none;
}

.side:hover {
  color: #2563eb;
  cursor: pointer;
}

/* Стили для router-link */
a {
  text-decoration: none;
  color: inherit;
}

.router-link-active .side {
  color: #2563eb;
}
</style>
