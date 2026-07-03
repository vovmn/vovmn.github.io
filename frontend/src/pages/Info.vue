<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import api from '@/api/axios'
import Header from '@/components/Header.vue'

const router = useRouter()
const assignments = ref([])
const loading = ref(true)
const error = ref(null)

const completedCount = computed(() =>
  assignments.value.filter((assignment) => assignment.status === 'completed').length
)

const inProgressCount = computed(() =>
  assignments.value.filter((assignment) => assignment.status === 'in_progress').length
)

const completionPercent = computed(() => {
  if (!assignments.value.length) return 0
  return Math.round((completedCount.value / assignments.value.length) * 100)
})

onMounted(async () => {
  try {
    const { data } = await api.get('/api/questionnaire/my-assignments')
    assignments.value = data
  } catch (e) {
    error.value = 'Не удалось загрузить опросники'
    console.error(e)
  } finally {
    loading.value = false
  }
})

function openQuestionnaire(code) {
  router.push({ name: 'Questionnaire', params: { systemCode: code } })
}

function statusLabel(status) {
  if (status === 'completed') return 'Завершен'
  if (status === 'in_progress') return 'В процессе'
  return 'Ожидает'
}

function formatDate(value) {
  if (!value) return 'Нет даты'
  return new Intl.DateTimeFormat('ru-RU', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  }).format(new Date(value))
}
</script>

<template>
  <div class="page">
    <Header />

    <main class="workspace">
      <section class="summary">
        <div>
          <p class="eyebrow">Личный кабинет</p>
          <h1>Опросники по системам организма</h1>
          <p class="summary-copy">
            Здесь собраны назначенные анкеты, их статус и дата последнего прохождения.
          </p>
        </div>

        <div class="stats">
          <div class="stat">
            <span>{{ assignments.length }}</span>
            <p>Назначено</p>
          </div>
          <div class="stat">
            <span>{{ completedCount }}</span>
            <p>Завершено</p>
          </div>
          <div class="stat">
            <span>{{ completionPercent }}%</span>
            <p>Прогресс</p>
          </div>
        </div>
      </section>

      <div v-if="loading" class="state">Загрузка опросников...</div>
      <div v-else-if="error" class="state state-error">{{ error }}</div>

      <section v-else-if="assignments.length === 0" class="empty">
        <h2>Пока нет назначенных опросников</h2>
        <p>Когда врач или система назначит анкету, она появится на этой странице.</p>
      </section>

      <section v-else class="content">
        <div class="section-heading">
          <div>
            <h2>Назначенные анкеты</h2>
            <p>{{ inProgressCount }} в процессе, {{ completedCount }} завершено</p>
          </div>
        </div>

        <div class="systems-grid">
          <article
            v-for="assign in assignments"
            :key="assign.id"
            class="system-card"
          >
            <div class="card-top">
              <span class="system-mark">{{ assign.name.slice(0, 1) }}</span>
              <span class="status" :class="`status-${assign.status}`">
                {{ statusLabel(assign.status) }}
              </span>
            </div>

            <h3>{{ assign.name }}</h3>
            <p class="system-code">{{ assign.code }}</p>

            <div class="meta">
              <span>Назначен</span>
              <strong>{{ formatDate(assign.assigned_at) }}</strong>
            </div>
            <div class="meta">
              <span>Завершен</span>
              <strong>{{ formatDate(assign.completed_at) }}</strong>
            </div>

            <button type="button" @click="openQuestionnaire(assign.code)">
              {{ assign.status === 'completed' ? 'Открыть снова' : 'Начать' }}
            </button>
          </article>
        </div>
      </section>
    </main>
  </div>
</template>

<style scoped>
.page {
  min-height: 100vh;
}

.workspace {
  width: min(1180px, calc(100% - 2rem));
  margin: 0 auto;
  padding: 2rem 0 3rem;
}

.summary {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  gap: 1.5rem;
  align-items: end;
  padding: 2rem;
  border: 1px solid #dce8ea;
  border-radius: 8px;
  background: #fff;
  box-shadow: 0 16px 40px rgba(25, 45, 65, 0.07);
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
h3,
p {
  margin-top: 0;
}

h1 {
  max-width: 760px;
  margin-bottom: 0.8rem;
  color: #152236;
  font-size: clamp(2rem, 4vw, 3.1rem);
  line-height: 1.08;
}

.summary-copy {
  max-width: 640px;
  margin-bottom: 0;
  color: #637284;
  font-size: 1rem;
  line-height: 1.6;
}

.stats {
  display: grid;
  grid-template-columns: repeat(3, minmax(108px, 1fr));
  gap: 0.75rem;
}

.stat {
  min-width: 108px;
  padding: 1rem;
  border: 1px solid #e2ecee;
  border-radius: 8px;
  background: #f8fbfb;
}

.stat span {
  display: block;
  color: #162337;
  font-size: 1.55rem;
  font-weight: 800;
}

.stat p {
  margin: 0.25rem 0 0;
  color: #667788;
  font-size: 0.86rem;
}

.content {
  margin-top: 1.5rem;
}

.section-heading {
  display: flex;
  justify-content: space-between;
  margin-bottom: 1rem;
}

.section-heading h2 {
  margin-bottom: 0.25rem;
  color: #19283c;
  font-size: 1.2rem;
}

.section-heading p {
  margin-bottom: 0;
  color: #6b7c8d;
}

.systems-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1rem;
}

.system-card {
  display: flex;
  min-height: 260px;
  flex-direction: column;
  padding: 1.25rem;
  border: 1px solid #dce8ea;
  border-radius: 8px;
  background: #fff;
  box-shadow: 0 10px 26px rgba(25, 45, 65, 0.05);
}

.card-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.8rem;
  margin-bottom: 1rem;
}

.system-mark {
  display: grid;
  width: 42px;
  height: 42px;
  place-items: center;
  border-radius: 8px;
  background: #edf7f6;
  color: #12635f;
  font-weight: 800;
}

.status {
  padding: 0.35rem 0.6rem;
  border-radius: 999px;
  font-size: 0.76rem;
  font-weight: 800;
}

.status-completed {
  background: #e8f6ee;
  color: #1f6c45;
}

.status-in_progress {
  background: #fff6df;
  color: #946713;
}

.status-pending {
  background: #eef3f7;
  color: #526575;
}

.system-card h3 {
  margin-bottom: 0.35rem;
  color: #1d2b40;
  font-size: 1.08rem;
  line-height: 1.35;
}

.system-code {
  margin-bottom: 1.2rem;
  color: #7b8b9a;
  font-size: 0.82rem;
  text-transform: uppercase;
}

.meta {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  padding: 0.55rem 0;
  border-top: 1px solid #edf2f4;
  color: #718291;
  font-size: 0.88rem;
}

.meta strong {
  color: #28384a;
  font-weight: 700;
}

.system-card button {
  min-height: 42px;
  margin-top: auto;
  border: 0;
  border-radius: 8px;
  background: #1f847f;
  color: #fff;
  font-weight: 800;
}

.system-card button:hover {
  background: #176b67;
}

.state,
.empty {
  margin-top: 1.5rem;
  padding: 2rem;
  border: 1px solid #dce8ea;
  border-radius: 8px;
  background: #fff;
  color: #637284;
  text-align: center;
}

.state-error {
  color: #b42318;
}

.empty h2 {
  margin-bottom: 0.5rem;
  color: #1d2b40;
}

@media (max-width: 860px) {
  .summary {
    grid-template-columns: 1fr;
    padding: 1.4rem;
  }

  .stats {
    grid-template-columns: repeat(3, 1fr);
  }
}

@media (max-width: 560px) {
  .stats {
    grid-template-columns: 1fr;
  }
}
</style>
