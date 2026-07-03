<template>
  <header class="header">
    <router-link class="brand" to="/info" aria-label="Имулаб">
      <span class="brand-mark">IM</span>
      <span class="brand-text">Имулаб</span>
    </router-link>

    <nav class="nav" aria-label="Основная навигация">
      <router-link to="/info">Опросники</router-link>
      <router-link to="/contacts">Контакты</router-link>
      <router-link to="/documents">Документы</router-link>
      <router-link to="/record">Запись</router-link>
    </nav>

    <div class="user-menu">
      <router-link v-if="!auth.user" class="login-link" to="/login">Войти</router-link>
      <template v-else>
        <span class="user-chip">{{ auth.user.username }}</span>
        <button class="logout-btn" type="button" @click="logout">Выйти</button>
      </template>
    </div>
  </header>
</template>

<script setup>
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const router = useRouter()

async function logout() {
  await auth.logout()
  router.replace('/login')
}
</script>

<style scoped>
.header {
  position: sticky;
  top: 0;
  z-index: 20;
  display: grid;
  grid-template-columns: auto 1fr auto;
  align-items: center;
  gap: 1.5rem;
  min-height: 72px;
  padding: 0 2rem;
  background: rgba(255, 255, 255, 0.92);
  border-bottom: 1px solid #dfe8ea;
  backdrop-filter: blur(14px);
}

.brand {
  display: inline-flex;
  align-items: center;
  gap: 0.75rem;
  font-weight: 700;
  color: #162337;
}

.brand-mark {
  display: inline-grid;
  width: 40px;
  height: 40px;
  place-items: center;
  border-radius: 8px;
  background: #1f847f;
  color: #fff;
  font-size: 0.82rem;
  letter-spacing: 0;
}

.brand-text {
  font-size: 1.08rem;
}

.nav {
  display: flex;
  justify-content: center;
  gap: 0.4rem;
}

.nav a {
  padding: 0.65rem 0.85rem;
  border-radius: 8px;
  color: #5f6f7f;
  font-size: 0.95rem;
  font-weight: 600;
}

.nav a:hover,
.nav .router-link-active {
  background: #edf7f6;
  color: #12635f;
}

.user-menu {
  display: inline-flex;
  align-items: center;
  justify-content: flex-end;
  gap: 0.75rem;
}

.user-chip {
  max-width: 180px;
  overflow: hidden;
  color: #243244;
  font-size: 0.92rem;
  font-weight: 600;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.login-link,
.logout-btn {
  display: inline-flex;
  min-height: 38px;
  align-items: center;
  padding: 0 0.95rem;
  border: 1px solid #c9d7da;
  border-radius: 8px;
  background: #fff;
  color: #203042;
  font-weight: 700;
}

.logout-btn:hover,
.login-link:hover {
  border-color: #1f847f;
  color: #12635f;
}

@media (max-width: 820px) {
  .header {
    grid-template-columns: 1fr;
    gap: 0.8rem;
    padding: 1rem;
  }

  .nav {
    justify-content: flex-start;
    overflow-x: auto;
    padding-bottom: 0.1rem;
  }

  .user-menu {
    justify-content: flex-start;
  }
}
</style>
