<template>
  <div class="page">
    <Header />

    <main class="admin">
      <section class="hero">
        <div>
          <p class="eyebrow">Панель администратора</p>
          <h1>Назначения и приемы</h1>
          <p>Выберите пациента, назначьте ему опросник или создайте время приема.</p>
        </div>
      </section>

      <div v-if="loading" class="state">Загрузка данных...</div>
      <div v-else-if="error" class="state state-error">{{ error }}</div>

      <template v-else>
        <section class="forms-grid">
          <form class="panel" @submit.prevent="assignQuestionnaire">
            <div class="section-heading">
              <h2>Назначить опросник</h2>
              <p>Назначение появится в кабинете пациента.</p>
            </div>

            <label class="field">
              <span>Пациент</span>
              <select v-model="questionnaireForm.user_id" required>
                <option value="">Выберите пациента</option>
                <option v-for="user in users" :key="user.id" :value="user.id">
                  {{ user.username }} · {{ user.email }}
                </option>
              </select>
            </label>

            <label class="field">
              <span>Опросник</span>
              <select v-model="questionnaireForm.system_id" required>
                <option value="">Выберите систему</option>
                <option v-for="system in systems" :key="system.id" :value="system.id">
                  {{ system.name }}
                </option>
              </select>
            </label>

            <button class="submit-btn" type="submit" :disabled="savingQuestionnaire">
              {{ savingQuestionnaire ? 'Назначаем...' : 'Назначить опросник' }}
            </button>
          </form>

          <form class="panel" @submit.prevent="createAppointment">
            <div class="section-heading">
              <h2>Назначить прием</h2>
              <p>Создайте дату и время приема для пациента.</p>
            </div>

            <label class="field">
              <span>Пациент</span>
              <select v-model="appointmentForm.user_id" required>
                <option value="">Выберите пациента</option>
                <option v-for="user in users" :key="user.id" :value="user.id">
                  {{ user.username }} · {{ user.email }}
                </option>
              </select>
            </label>

            <div class="row">
              <label class="field">
                <span>Дата</span>
                <input v-model="appointmentForm.date" type="date" required />
              </label>

              <label class="field">
                <span>Время</span>
                <input v-model="appointmentForm.time" type="time" required />
              </label>
            </div>

            <div class="row">
              <label class="field">
                <span>Специализация</span>
                <input v-model.trim="appointmentForm.specialist" type="text" placeholder="Терапевт" />
              </label>

              <label class="field">
                <span>Врач</span>
                <input v-model.trim="appointmentForm.doctor" type="text" placeholder="Иванов И.И." />
              </label>
            </div>

            <label class="field">
              <span>Комментарий</span>
              <textarea v-model.trim="appointmentForm.comment" rows="3" placeholder="Кабинет, детали приема"></textarea>
            </label>

            <button class="submit-btn" type="submit" :disabled="savingAppointment">
              {{ savingAppointment ? 'Создаем...' : 'Назначить прием' }}
            </button>
          </form>
        </section>

        <p v-if="success" class="success">{{ success }}</p>

        <section class="lists-grid">
          <div class="panel">
            <div class="section-heading">
              <h2>Последние опросники</h2>
              <p>{{ assignments.length }} назначений</p>
            </div>

            <div class="list">
              <article v-for="assignment in assignments" :key="assignment.id" class="list-item">
                <div>
                  <strong>{{ assignment.username }}</strong>
                  <p>{{ assignment.name }}</p>
                </div>
                <span class="status">{{ statusLabel(assignment.status) }}</span>
              </article>
            </div>
          </div>

          <div class="panel">
            <div class="section-heading">
              <h2>Последние приемы</h2>
              <p>{{ appointments.length }} записей</p>
            </div>

            <div class="list">
              <article v-for="appointment in appointments" :key="appointment.id" class="list-item">
                <div>
                  <strong>{{ appointment.username }}</strong>
                  <p>{{ formatDateTime(appointment.scheduled_at) }}</p>
                </div>
                <span class="status">{{ appointment.specialist || 'Прием' }}</span>
              </article>
            </div>
          </div>
        </section>
      </template>
    </main>
  </div>
</template>

<script setup>
import { onMounted, reactive, ref } from 'vue'
import api from '@/api/axios'
import Header from '@/components/Header.vue'

const users = ref([])
const systems = ref([])
const assignments = ref([])
const appointments = ref([])
const loading = ref(true)
const error = ref('')
const success = ref('')
const savingQuestionnaire = ref(false)
const savingAppointment = ref(false)

const questionnaireForm = reactive({
  user_id: '',
  system_id: '',
})

const appointmentForm = reactive({
  user_id: '',
  date: '',
  time: '',
  specialist: '',
  doctor: '',
  comment: '',
})

onMounted(loadAdminData)

async function loadAdminData() {
  loading.value = true
  error.value = ''

  try {
    const [usersRes, systemsRes, assignmentsRes, appointmentsRes] = await Promise.all([
      api.get('/api/admin/users'),
      api.get('/api/admin/systems'),
      api.get('/api/admin/questionnaire-assignments'),
      api.get('/api/admin/appointments'),
    ])

    users.value = usersRes.data
    systems.value = systemsRes.data
    assignments.value = assignmentsRes.data
    appointments.value = appointmentsRes.data
  } catch (e) {
    error.value = e?.response?.data?.error || 'Не удалось загрузить админку'
    console.error(e)
  } finally {
    loading.value = false
  }
}

async function assignQuestionnaire() {
  savingQuestionnaire.value = true
  success.value = ''

  try {
    await api.post('/api/admin/questionnaire-assignments', questionnaireForm)
    questionnaireForm.system_id = ''
    success.value = 'Опросник назначен'
    await refreshLists()
  } catch (e) {
    error.value = e?.response?.data?.error || 'Не удалось назначить опросник'
  } finally {
    savingQuestionnaire.value = false
  }
}

async function createAppointment() {
  savingAppointment.value = true
  success.value = ''

  try {
    await api.post('/api/admin/appointments', {
      user_id: appointmentForm.user_id,
      scheduled_at: `${appointmentForm.date}T${appointmentForm.time}:00`,
      specialist: appointmentForm.specialist || null,
      doctor: appointmentForm.doctor || null,
      comment: appointmentForm.comment || null,
    })

    appointmentForm.date = ''
    appointmentForm.time = ''
    appointmentForm.specialist = ''
    appointmentForm.doctor = ''
    appointmentForm.comment = ''
    success.value = 'Прием назначен'
    await refreshLists()
  } catch (e) {
    error.value = e?.response?.data?.error || 'Не удалось назначить прием'
  } finally {
    savingAppointment.value = false
  }
}

async function refreshLists() {
  const [assignmentsRes, appointmentsRes] = await Promise.all([
    api.get('/api/admin/questionnaire-assignments'),
    api.get('/api/admin/appointments'),
  ])

  assignments.value = assignmentsRes.data
  appointments.value = appointmentsRes.data
}

function statusLabel(status) {
  if (status === 'completed') return 'Завершен'
  if (status === 'in_progress') return 'В процессе'
  return 'Ожидает'
}

function formatDateTime(value) {
  if (!value) return 'Нет даты'
  return new Intl.DateTimeFormat('ru-RU', {
    day: '2-digit',
    month: 'short',
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

.admin {
  width: min(1180px, calc(100% - 2rem));
  margin: 0 auto;
  padding: 2rem 0 3rem;
}

.hero,
.panel {
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
.section-heading p,
.list-item p {
  color: #637284;
  line-height: 1.55;
}

.forms-grid,
.lists-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1rem;
}

.lists-grid {
  margin-top: 1rem;
}

.panel {
  padding: 1.4rem;
}

.section-heading {
  margin-bottom: 1rem;
}

.section-heading h2 {
  margin-bottom: 0.3rem;
  color: #1d2b40;
  font-size: 1.16rem;
}

.section-heading p {
  margin-bottom: 0;
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

.field input,
.field select,
.field textarea {
  width: 100%;
  min-height: 44px;
  padding: 0 0.85rem;
  border: 1px solid #d4e0e3;
  border-radius: 8px;
  background: #fbfdfd;
  color: #172338;
  outline: none;
}

.field textarea {
  min-height: 92px;
  padding-top: 0.75rem;
  resize: vertical;
}

.field input:focus,
.field select:focus,
.field textarea:focus {
  border-color: #1f847f;
  box-shadow: 0 0 0 3px rgba(31, 132, 127, 0.12);
}

.row {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.75rem;
}

.submit-btn {
  width: 100%;
  min-height: 44px;
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
  cursor: not-allowed;
}

.state,
.success {
  margin: 1rem 0;
  padding: 1rem;
  border-radius: 8px;
  background: #fff;
  color: #637284;
}

.state-error {
  color: #b42318;
}

.success {
  border: 1px solid #cde8d8;
  background: #f0fbf4;
  color: #1f6c45;
  font-weight: 800;
}

.list {
  display: grid;
  gap: 0.65rem;
}

.list-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 0.85rem;
  border: 1px solid #e1ecee;
  border-radius: 8px;
  background: #fbfdfd;
}

.list-item strong {
  color: #203044;
}

.list-item p {
  margin: 0.2rem 0 0;
  font-size: 0.92rem;
}

.status {
  flex: 0 0 auto;
  padding: 0.35rem 0.6rem;
  border-radius: 999px;
  background: #edf7f6;
  color: #12635f;
  font-size: 0.78rem;
  font-weight: 900;
}

@media (max-width: 860px) {
  .forms-grid,
  .lists-grid,
  .row {
    grid-template-columns: 1fr;
  }
}
</style>
