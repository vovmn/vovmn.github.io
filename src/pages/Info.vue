
<script setup>
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { watch } from 'vue'
import Header from '../components/header.vue'

const router = useRouter()
const auth = useAuthStore()

// перенаправляем, если не авторизован
watch(
  () => auth.accessToken,
  (token) => {
    if (!token) {
      router.push('/login')
    }
  },
  { immediate: true }
)
</script>

<template>
  <div class="page" v-if="auth.user">
    <Header />
    
    <div class="container">
      <div class="profile-card">
        <h2 class="title">Мой профиль</h2>
        
        <div class="info-grid">
          <div class="info-item">
            <label>Логин:</label>
            <p>{{ auth.user.username }}</p>
          </div>
          
          <div class="info-item">
            <label>Почта:</label>
            <p>{{ auth.user.email }}</p>
          </div>
          
          <div class="info-item">
            <label>Роль:</label>
            <p class="role" :class="auth.user.role">{{ auth.user.role }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.page {
  background-color: #f5f7fa;
  min-height: 100vh;
}

.container {
  max-width: 600px;
  margin: 2rem auto;
  padding: 0 2rem;
}

.profile-card {
  background: white;
  border-radius: 8px;
  padding: 2rem;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
}

.title {
  margin-top: 0;
  color: #333;
  font-size: 1.8rem;
  margin-bottom: 2rem;
  text-align: center;
}

.info-grid {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.info-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem;
  border-bottom: 1px solid #e1e5e9;
}

.info-item:last-child {
  border-bottom: none;
}

label {
  font-weight: 600;
  color: #555;
  min-width: 120px;
}

p {
  margin: 0;
  color: #333;
  font-size: 1.1rem;
}

.role {
  display: inline-block;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  font-weight: 500;
}

.role.user {
  background: #e8f0ff;
  color: #2563eb;
}

.role.admin {
  background: #ffe8e8;
  color: #dc2626;
}
</style>
