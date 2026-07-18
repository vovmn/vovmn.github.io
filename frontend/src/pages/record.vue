<template>
  <div class="page">
    <Header />

    <main class="appointments">
      <section class="hero">
        <div>
          <p class="eyebrow">Запись на прием</p>
          <h1>Назначенные приемы</h1>
          <p>Здесь отображается время приема, которое назначил администратор.</p>
        </div>
      </section>

      <div v-if="loading" class="state">Загрузка записей...</div>
      <div v-else-if="error" class="state state-error">{{ error }}</div>

      <section v-else-if="appointments.length === 0" class="empty">
        <h2>Пока нет назначенных приемов</h2>
        <p>Когда администратор назначит дату и время, запись появится здесь.</p>
      </section>

      <section v-else class="appointments-list">
        <article v-for="appointment in appointments" :key="appointment.id" class="appointment-card">
          <div>
            <span class="status">{{ statusLabel(appointment.status) }}</span>
            <h2>{{ formatDateTime(appointment.scheduled_at) }}</h2>
            <p>{{ appointment.specialist || 'Прием' }}</p>
          </div>

          <dl>
            <div>
              <dt>Врач</dt>
              <dd>{{ appointment.doctor || 'Не указан' }}</dd>
            </div>
            <div>
              <dt>Комментарий</dt>
              <dd>{{ appointment.comment || 'Нет комментария' }}</dd>
            </div>
          </dl>
        </article>
      </section>
    </main>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import api from '@/api/axios'
import Header from '../components/Header.vue'

const appointments = ref([])
const loading = ref(true)
const error = ref('')

onMounted(async () => {
  try {
    const { data } = await api.get('/api/appointments/my')
    appointments.value = data
  } catch (e) {
    error.value = e?.response?.data?.error || 'Не удалось загрузить записи'
  } finally {
    loading.value = false
  }
})

function statusLabel(status) {
  if (status === 'completed') return 'Завершен'
  if (status === 'cancelled') return 'Отменен'
  return 'Запланирован'
}

function formatDateTime(value) {
  if (!value) return 'Нет даты'
  return new Intl.DateTimeFormat('ru-RU', {
    day: '2-digit',
    month: 'long',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(new Date(value))
}
</script>

<style scoped>
.page {
  min-height: 100vh;
}

.appointments {
  width: min(980px, calc(100% - 2rem));
  margin: 0 auto;
  padding: 2rem 0 3rem;
}

.hero,
.appointment-card,
.empty,
.state {
  border: 1px solid #dce8ea;
  border-radius: 8px;
  background: #fff;
  box-shadow: 0 16px 40px rgba(25, 45, 65, 0.07);
}

.hero {
  margin-bottom: 1rem;
  padding: 2rem;
}

.eyebrow {
  margin: 0 0 0.7rem;
  color: #1f847f;
  font-size: 0.82rem;
  font-weight: 800;
  text-transform: uppercase;
}

h1,
h2,
p {
  margin-top: 0;
}

h1 {
  margin-bottom: 0.7rem;
  color: #152236;
  font-size: clamp(2rem, 4vw, 3rem);
}

.hero p,
.empty p,
.appointment-card p,
dt {
  color: #637284;
}

.appointments-list {
  display: grid;
  gap: 1rem;
}

.appointment-card {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(260px, 0.6fr);
  gap: 1rem;
  padding: 1.3rem;
}

.status {
  display: inline-flex;
  margin-bottom: 0.8rem;
  padding: 0.35rem 0.6rem;
  border-radius: 999px;
  background: #edf7f6;
  color: #12635f;
  font-size: 0.78rem;
  font-weight: 900;
}

.appointment-card h2 {
  margin-bottom: 0.35rem;
  color: #1d2b40;
}

.appointment-card p {
  margin-bottom: 0;
}

dl {
  display: grid;
  gap: 0.8rem;
  margin: 0;
}

dt {
  margin-bottom: 0.2rem;
  font-size: 0.84rem;
  font-weight: 800;
}

dd {
  margin: 0;
  color: #203044;
}

.empty,
.state {
  padding: 2rem;
  text-align: center;
}

.state-error {
  color: #b42318;
}

@media (max-width: 720px) {
  .appointment-card {
    grid-template-columns: 1fr;
  }
}
</style>
