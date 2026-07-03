<template>
  <article class="question-widget">
    <div class="question-head">
      <p class="question-text">{{ question.question_text }}</p>
      <span v-if="question.required" class="required">Обязательный</span>
    </div>

    <div v-if="question.question_type === 'boolean'" class="segmented">
      <label :class="{ active: modelValue?.value_boolean === true }">
        <input
          type="radio"
          :name="question.id"
          :checked="modelValue?.value_boolean === true"
          @change="emitValue({ value_boolean: true })"
        />
        Да
      </label>
      <label :class="{ active: modelValue?.value_boolean === false }">
        <input
          type="radio"
          :name="question.id"
          :checked="modelValue?.value_boolean === false"
          @change="emitValue({ value_boolean: false })"
        />
        Нет
      </label>
    </div>

    <div v-else-if="question.question_type === 'numeric'" class="field">
      <input
        type="number"
        :value="modelValue?.value_numeric ?? ''"
        @input="emitNumericValue($event.target.value)"
        placeholder="Введите число"
      />
    </div>

    <div v-else-if="question.question_type === 'single_choice'" class="options">
      <label
        v-for="opt in question.options"
        :key="opt.id"
        class="option"
        :class="{ active: modelValue?.selected_option_id === opt.id }"
      >
        <input
          type="radio"
          :name="question.id"
          :checked="modelValue?.selected_option_id === opt.id"
          @change="emitValue({ selected_option_id: opt.id })"
        />
        <span>{{ opt.text }}</span>
      </label>
    </div>

    <div v-else-if="question.question_type === 'multiple_choice'" class="options">
      <label
        v-for="opt in question.options"
        :key="opt.id"
        class="option"
        :class="{ active: selectedOptionIds.includes(opt.id) }"
      >
        <input
          type="checkbox"
          :checked="selectedOptionIds.includes(opt.id)"
          @change="toggleOption(opt.id, $event.target.checked)"
        />
        <span>{{ opt.text }}</span>
      </label>
    </div>

    <div v-else-if="question.question_type === 'text'" class="field">
      <textarea
        :value="modelValue?.value_text ?? ''"
        @input="emitValue({ value_text: $event.target.value })"
        placeholder="Введите ответ"
        rows="3"
      ></textarea>
    </div>

    <div v-else class="unsupported">
      Тип вопроса "{{ question.question_type }}" не поддерживается
    </div>
  </article>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  question: {
    type: Object,
    required: true,
  },
  modelValue: {
    type: Object,
    default: () => ({}),
  },
})

const emit = defineEmits(['update:modelValue'])

const selectedOptionIds = computed(() => props.modelValue?.selected_option_ids ?? [])

function emitValue(val) {
  emit('update:modelValue', val)
}

function emitNumericValue(rawValue) {
  emitValue({
    value_numeric: rawValue === '' ? null : Number(rawValue),
  })
}

function toggleOption(optionId, checked) {
  const currentIds = selectedOptionIds.value
  const nextIds = checked
    ? [...new Set([...currentIds, optionId])]
    : currentIds.filter((id) => id !== optionId)

  emitValue({ selected_option_ids: nextIds })
}
</script>

<style scoped>
.question-widget {
  padding: 1.1rem;
  border: 1px solid #dce8ea;
  border-radius: 8px;
  background: #fff;
  box-shadow: 0 8px 20px rgba(25, 45, 65, 0.04);
}

.question-head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 0.9rem;
}

.question-text {
  margin: 0;
  color: #1d2b40;
  font-weight: 700;
  line-height: 1.45;
}

.required {
  flex: 0 0 auto;
  padding: 0.28rem 0.5rem;
  border-radius: 999px;
  background: #eef7f6;
  color: #12635f;
  font-size: 0.72rem;
  font-weight: 800;
}

.segmented {
  display: grid;
  grid-template-columns: repeat(2, minmax(96px, 1fr));
  gap: 0.5rem;
  max-width: 280px;
}

.segmented label,
.option {
  display: flex;
  align-items: center;
  gap: 0.55rem;
  min-height: 44px;
  padding: 0.65rem 0.75rem;
  border: 1px solid #d4e0e3;
  border-radius: 8px;
  background: #fbfdfd;
  color: #27374a;
  cursor: pointer;
}

.segmented label.active,
.option.active {
  border-color: #1f847f;
  background: #edf7f6;
  color: #12635f;
  font-weight: 700;
}

.segmented input,
.option input {
  flex: 0 0 auto;
  accent-color: #1f847f;
}

.options {
  display: grid;
  gap: 0.55rem;
}

.field input,
.field textarea {
  width: 100%;
  border: 1px solid #d4e0e3;
  border-radius: 8px;
  background: #fbfdfd;
  color: #1d2b40;
  outline: none;
}

.field input {
  min-height: 44px;
  padding: 0 0.85rem;
}

.field textarea {
  min-height: 110px;
  resize: vertical;
  padding: 0.8rem 0.85rem;
}

.field input:focus,
.field textarea:focus {
  border-color: #1f847f;
  box-shadow: 0 0 0 3px rgba(31, 132, 127, 0.12);
}

.unsupported {
  padding: 0.8rem;
  border-radius: 8px;
  background: #fff6df;
  color: #946713;
}

@media (max-width: 560px) {
  .question-head {
    display: block;
  }

  .required {
    display: inline-block;
    margin-top: 0.6rem;
  }
}
</style>
