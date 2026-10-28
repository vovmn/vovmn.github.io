<template>
  <div class="page">
    <Header />

    <main class="questionnaire">
      <button type="button" class="back-btn" @click="goBack">Назад к опросникам</button>

      <div v-if="loading" class="state">Загрузка опросника...</div>
      <div v-else-if="error" class="state state-error">{{ error }}</div>

      <section v-else-if="submitted" class="result">
        <span class="result-mark">OK</span>
        <h1>Ответы сохранены</h1>
        <p>Опросник успешно завершен. Данные уже записаны в базу.</p>
        <button type="button" @click="goBack">Вернуться к опросникам</button>
      </section>

      <template v-else>
        <section class="hero">
          <div>
            <p class="eyebrow">Медицинская анкета</p>
            <h1>{{ systemName }}</h1>
            <p>Отвечайте последовательно. Обязательные вопросы учитываются перед отправкой.</p>
          </div>

          <div class="progress-box">
            <span>{{ answeredCount }} / {{ visibleQuestions.length }}</span>
            <p>заполнено</p>
            <div class="progress-track">
              <div class="progress-fill" :style="{ width: `${progressPercent}%` }"></div>
            </div>
          </div>
        </section>

        <aside class="clinical-notice">
          Анкета помогает структурировать сведения о симптомах, но не ставит диагноз и не заменяет консультацию врача.
        </aside>

        <aside v-if="activeAlerts.length" class="urgent-alert" role="alert">
          <strong>Обратите внимание</strong>
          <p v-for="message in activeAlerts" :key="message">{{ message }}</p>
        </aside>

        <section class="questions-list">
          <QuestionWidget
            v-for="q in visibleQuestions"
            :key="q.id"
            :question="q"
            v-model="answers[q.id]"
          />
        </section>

        <div class="submit-panel">
          <div>
            <strong>{{ progressPercent }}%</strong>
            <span>готовность анкеты</span>
          </div>
          <button
            class="submit-btn"
            :disabled="isSubmitting || hasMissingRequired"
            type="button"
            @click="handleSubmit"
          >
            {{ isSubmitting ? 'Сохраняем...' : 'Отправить ответы' }}
          </button>
        </div>
      </template>
    </main>
  </div>
</template>

<script setup>
import { computed, onMounted, reactive, ref, watchEffect } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import api from '@/api/axios'
import Header from '@/components/Header.vue'
import QuestionWidget from '@/components/QuestionWidget.vue'

const route = useRoute()
const router = useRouter()
const assignmentIdFromRoute = route.params.assignmentId

const questions = ref([])
const systemName = ref('')
const assignmentId = ref(null)
const loading = ref(true)
const error = ref(null)
const submitted = ref(false)
const isSubmitting = ref(false)
const answers = reactive({})

const visibleQuestions = computed(() =>
  questions.value.filter((question) => conditionMatches(question.display_condition))
)

const answeredCount = computed(() =>
  visibleQuestions.value.filter((question) => hasAnswer(question)).length
)

const progressPercent = computed(() => {
  if (!visibleQuestions.value.length) return 0
  return Math.round((answeredCount.value / visibleQuestions.value.length) * 100)
})

const hasMissingRequired = computed(() =>
  visibleQuestions.value.some((question) => question.required && !hasAnswer(question))
)

const activeAlerts = computed(() => {
  const messages = visibleQuestions.value
    .filter((question) => question.alert && alertMatches(question.alert, answers[question.id]))
    .map((question) => question.alert.message)
  return [...new Set(messages)]
})

watchEffect(() => {
  const visibleIds = new Set(visibleQuestions.value.map((question) => String(question.id)))
  for (const questionId of Object.keys(answers)) {
    if (!visibleIds.has(String(questionId))) delete answers[questionId]
  }
})

onMounted(async () => {
  try {
    const { data } = await api.post(`/api/questionnaire/assignments/${assignmentIdFromRoute}/start`)
    assignmentId.value = data.assignment_id
    systemName.value = data.system_name
    questions.value = data.questions
  } catch (e) {
    error.value = 'Не удалось загрузить опросник'
    console.error(e)
  } finally {
    loading.value = false
  }
})

async function handleSubmit() {
  isSubmitting.value = true
  try {
    const missingRequired = visibleQuestions.value.find((question) => question.required && !hasAnswer(question))
    if (missingRequired) {
      alert('Заполните все обязательные вопросы')
      return
    }

    const payload = visibleQuestions.value
      .filter((question) => hasAnswer(question))
      .map((question) => ({
        question_id: question.id,
        ...answers[question.id],
      }))

    await api.post(`/api/questionnaire/submit/${assignmentId.value}`, { answers: payload })
    submitted.value = true
  } catch (e) {
    alert('Ошибка при сохранении ответов')
    console.error(e)
  } finally {
    isSubmitting.value = false
  }
}

function hasAnswer(question) {
  const answer = answers[question.id]
  if (!answer) return false
  if (answer.skipped === true) return true
  if (answer.value_unknown === true) return question.allow_unknown === true

  switch (question.question_type) {
    case 'boolean':
      return Object.prototype.hasOwnProperty.call(answer, 'value_boolean')
    case 'numeric':
      return answer.value_numeric !== null && answer.value_numeric !== '' && !Number.isNaN(answer.value_numeric)
    case 'single_choice':
      return Boolean(answer.selected_option_id)
    case 'multiple_choice':
      return Array.isArray(answer.selected_option_ids) && answer.selected_option_ids.length > 0
    case 'text':
      return Boolean(answer.value_text?.trim())
    default:
      return false
  }
}

function answerValue(questionCode) {
  const question = questions.value.find((item) => item.code === questionCode)
  if (!question) return undefined
  const answer = answers[question.id]
  if (!answer) return undefined
  if (answer.skipped === true) return 'skipped'
  if (answer.value_unknown === true) return 'unknown'
  if (Object.prototype.hasOwnProperty.call(answer, 'value_boolean')) return answer.value_boolean
  if (Object.prototype.hasOwnProperty.call(answer, 'value_numeric')) return answer.value_numeric
  if (Object.prototype.hasOwnProperty.call(answer, 'value_text')) return answer.value_text
  if (Object.prototype.hasOwnProperty.call(answer, 'selected_option_id')) return answer.selected_option_id
  if (Array.isArray(answer.selected_option_ids)) return answer.selected_option_ids
  return undefined
}

function conditionMatches(condition) {
  if (!condition) return true
  if (Array.isArray(condition.all)) return condition.all.every(conditionMatches)
  if (Array.isArray(condition.any)) return condition.any.some(conditionMatches)
  const actual = answerValue(condition.question_code)
  if (condition.operator === 'equals') return actual === condition.value
  if (condition.operator === 'not_equals') return actual !== undefined && actual !== condition.value
  if (condition.operator === 'in') return Array.isArray(condition.values) && condition.values.includes(actual)
  return false
}

function alertMatches(alert, answer) {
  if (!answer || answer.value_unknown === true || answer.skipped === true) return false
  const trigger = alert.trigger || { operator: 'equals', value: true }
  const actual = Object.prototype.hasOwnProperty.call(answer, 'value_boolean')
    ? answer.value_boolean
    : answer.selected_option_id
  return trigger.operator === 'equals' && actual === trigger.value
}

function goBack() {
  router.push('/info')
}
</script>

<style scoped>
.page {
  min-height: 100vh;
}

.questionnaire {
  width: min(920px, calc(100% - 2rem));
  margin: 0 auto;
  padding: 2rem 0 7rem;
}

.back-btn {
  min-height: 40px;
  margin-bottom: 1rem;
  padding: 0 0.9rem;
  border: 1px solid #cfdcdf;
  border-radius: 8px;
  background: #fff;
  color: #26384c;
  font-weight: 800;
}

.back-btn:hover {
  border-color: #1f847f;
  color: #12635f;
}

.hero {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 220px;
  gap: 1.5rem;
  align-items: end;
  margin-bottom: 1rem;
  padding: 1.6rem;
  border: 1px solid #dce8ea;
  border-radius: 8px;
  background: #fff;
  box-shadow: 0 16px 40px rgba(25, 45, 65, 0.07);
}

.eyebrow {
  margin: 0 0 0.65rem;
  color: #1f847f;
  font-size: 0.8rem;
  font-weight: 800;
  text-transform: uppercase;
}

h1,
p {
  margin-top: 0;
}

h1 {
  margin-bottom: 0.65rem;
  color: #152236;
  font-size: clamp(1.7rem, 4vw, 2.5rem);
  line-height: 1.12;
}

.hero p {
  margin-bottom: 0;
  color: #657687;
  line-height: 1.55;
}

.progress-box {
  padding: 1rem;
  border: 1px solid #e0ebed;
  border-radius: 8px;
  background: #f8fbfb;
}

.progress-box span {
  display: block;
  color: #172338;
  font-size: 1.45rem;
  font-weight: 800;
}

.progress-box p {
  margin: 0.2rem 0 0.8rem;
  color: #6d7e8f;
  font-size: 0.88rem;
}

.progress-track {
  height: 8px;
  overflow: hidden;
  border-radius: 999px;
  background: #dce8ea;
}

.progress-fill {
  height: 100%;
  border-radius: inherit;
  background: #1f847f;
  transition: width 0.2s ease;
}

.questions-list {
  display: grid;
  gap: 0.8rem;
}

.clinical-notice,
.urgent-alert {
  margin-bottom: 1rem;
  padding: 1rem 1.1rem;
  border-radius: 8px;
  line-height: 1.5;
}

.clinical-notice {
  border: 1px solid #cfe0e3;
  background: #f5fafb;
  color: #405567;
}

.urgent-alert {
  border: 1px solid #f0a7a0;
  background: #fff3f2;
  color: #8f1d16;
}

.urgent-alert p {
  margin: 0.45rem 0 0;
}

.submit-panel {
  position: fixed;
  right: 50%;
  bottom: 1rem;
  z-index: 10;
  display: flex;
  width: min(920px, calc(100% - 2rem));
  transform: translateX(50%);
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 0.9rem 1rem;
  border: 1px solid #d5e2e5;
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.94);
  box-shadow: 0 16px 40px rgba(25, 45, 65, 0.12);
  backdrop-filter: blur(14px);
}

.submit-panel strong {
  display: block;
  color: #172338;
  font-size: 1.05rem;
}

.submit-panel span {
  color: #6d7e8f;
  font-size: 0.88rem;
}

.submit-btn,
.result button {
  min-height: 44px;
  padding: 0 1.2rem;
  border: 0;
  border-radius: 8px;
  background: #1f847f;
  color: #fff;
  font-weight: 800;
}

.submit-btn:hover,
.result button:hover {
  background: #176b67;
}

.submit-btn:disabled {
  background: #9ab6b4;
  cursor: not-allowed;
}

.state,
.result {
  padding: 2rem;
  border: 1px solid #dce8ea;
  border-radius: 8px;
  background: #fff;
  text-align: center;
}

.state {
  color: #617283;
}

.state-error {
  color: #b42318;
}

.result {
  margin-top: 1rem;
}

.result-mark {
  display: inline-grid;
  width: 52px;
  height: 52px;
  place-items: center;
  margin-bottom: 1rem;
  border-radius: 8px;
  background: #e8f6ee;
  color: #1f6c45;
  font-weight: 900;
}

.result p {
  color: #657687;
}

@media (max-width: 760px) {
  .hero {
    grid-template-columns: 1fr;
  }

  .submit-panel {
    align-items: stretch;
    flex-direction: column;
  }

  .submit-btn {
    width: 100%;
  }
}
</style>
