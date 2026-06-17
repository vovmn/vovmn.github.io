<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import api from '@/api/axios'
import Header from '@/components/header.vue'

const router = useRouter()

// список назначенных опросников
const assignments = ref([])
const loading = ref(true)
const error = ref(null)

onMounted(async () => {
  try {
    // Запрашиваем только назначенные опросники для текущего пользователя
    const { data } = await api.get('/api/questionnaire/my-assignments')
    assignments.value = data
  } catch (e) {
    error.value = 'Не удалось загрузить назначенные опросники'
    console.error('Ошибка загрузки назначений:', e)
  } finally {
    loading.value = false
  }
})

// Переход к прохождению опросника по коду системы (роутер использует хеш-режим)
const startSystem = (code) => {
  router.push({ name: 'Questionnaire', params: { systemCode: code } })
}
</script>

<template>
  <body class="body">
    <Header />

    <div class="container">
      <h1 class="page-title">Мои опросники</h1>

      <div v-if="loading" class="loader">Загрузка...</div>
      <div v-else-if="error" class="error">{{ error }}</div>

      <!-- Пустое состояние, если ещё нет назначенных опросников -->
      <div v-else-if="assignments.length === 0" class="empty">
        У вас пока нет назначенных опросников.
      </div>

      <div v-else class="systems-grid">
        <div
          v-for="assign in assignments"
          :key="assign.id"
          class="system-card"
          @click="startSystem(assign.code)"
        >
          <div class="card-icon">
            <span>🩺</span>
          </div>
          <h3 class="card-title">{{ assign.name }}</h3>
          <p class="card-code">{{ assign.code }}</p>
          <!-- Статус опросника -->
          <p class="card-status">
            <template v-if="assign.status === 'completed'">✅ Завершён</template>
            <template v-else-if="assign.status === 'in_progress'">⏳ В процессе</template>
            <template v-else>🆕 Ожидает</template>
          </p>
        </div>
      </div>
    </div>
  </body>
</template>

<style scoped>
.page-title {
  margin-bottom: 2rem;
  font-size: 2rem;
  color: #2c3e50;
  text-align: center;
}

.systems-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 1.5rem;
}

.system-card {
  background: white;
  border-radius: 12px;
  padding: 2rem 1.5rem;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.06);
  text-align: center;
  cursor: pointer;
  transition: transform 0.2s, box-shadow 0.2s;
  border: 1px solid #eee;
}

.system-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.1);
}

.card-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
}

.card-title {
  font-size: 1.2rem;
  color: #34495e;
  margin-bottom: 0.5rem;
}

.card-code {
  font-size: 0.85rem;
  color: #95a5a6;
  text-transform: uppercase;
}

.card-status {
  font-size: 0.85rem;
  margin-top: 0.5rem;
  color: #2c3e50;
  font-weight: 500;
}

.loader,
.error,
.empty {
  text-align: center;
  font-size: 1.2rem;
  color: #7f8c8d;
  margin-top: 2rem;
}

.error {
  color: #e74c3c;
}
</style>