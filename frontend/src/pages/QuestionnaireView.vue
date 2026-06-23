<template>
  <div class="questionnaire-page">
    <Header />
    <div class="container">
      <button @click="goBack" class="back-btn">← Назад к списку</button>

      <div v-if="loading" class="loader">Загрузка опросника...</div>
      <div v-else-if="error" class="error">{{ error }}</div>
      <div v-else-if="submitted" class="success">
        <h2>Спасибо!</h2>
        <p>Опросник успешно завершен.</p>
        <button @click="goBack">Вернуться к списку</button>
      </div>
      <template v-else>
        <h1 class="page-title">{{ systemName }}</h1>
        <div class="questions-list">
          <QuestionWidget
            v-for="q in questions"
            :key="q.id"
            :question="q"
            v-model="answers[q.id]"
          />
        </div>
        <button
          class="submit-btn"
          :disabled="isSubmitting"
          @click="handleSubmit"
        >
          {{ isSubmitting ? 'Отправка...' : 'Отправить ответы' }}
        </button>
      </template>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import api from '@/api/axios'
import Header from '@/components/Header.vue'
import QuestionWidget from '@/components/QuestionWidget.vue'

const route = useRoute()
const router = useRouter()
const systemCode = route.params.systemCode

const questions = ref([])
const systemName = ref('')
const assignmentId = ref(null)
const loading = ref(true)
const error = ref(null)
const submitted = ref(false)
const isSubmitting = ref(false)
const answers = reactive({})

onMounted(async () => {
  try {
    const { data } = await api.post(`/api/questionnaire/start/${systemCode}`)
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
    const missingRequired = questions.value.find((question) => question.required && !hasAnswer(question))
    if (missingRequired) {
      alert('Заполните все обязательные вопросы')
      return
    }

    const payload = questions.value
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

function goBack() {
  router.push('/info')
}
</script>

<style scoped>
.container {
  max-width: 800px;
  margin: 0 auto;
  padding: 2rem;
}
.page-title {
  margin-bottom: 2rem;
  font-size: 1.8rem;
  color: #2c3e50;
}
.questions-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
.submit-btn {
  margin-top: 2rem;
  padding: 0.75rem 2rem;
  background-color: #3498db;
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 1rem;
  cursor: pointer;
}
.submit-btn:disabled {
  background-color: #95a5a6;
  cursor: not-allowed;
}
.back-btn {
  background: none;
  border: none;
  color: #3498db;
  cursor: pointer;
  font-size: 1rem;
  margin-bottom: 1rem;
}
.loader,
.error,
.success {
  text-align: center;
  margin-top: 3rem;
}
</style>
