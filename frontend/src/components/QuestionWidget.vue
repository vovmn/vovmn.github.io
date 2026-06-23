<template>
  <div class="question-widget">
    <p class="question-text">{{ question.question_text }}</p>

    <div v-if="question.question_type === 'boolean'" class="options">
      <label class="option">
        <input
          type="radio"
          :name="question.id"
          :checked="modelValue?.value_boolean === true"
          @change="emitValue({ value_boolean: true })"
        />
        Да
      </label>
      <label class="option">
        <input
          type="radio"
          :name="question.id"
          :checked="modelValue?.value_boolean === false"
          @change="emitValue({ value_boolean: false })"
        />
        Нет
      </label>
    </div>

    <div v-else-if="question.question_type === 'numeric'" class="numeric-input">
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
      >
        <input
          type="radio"
          :name="question.id"
          :checked="modelValue?.selected_option_id === opt.id"
          @change="emitValue({ selected_option_id: opt.id })"
        />
        {{ opt.text }}
      </label>
    </div>

    <div v-else-if="question.question_type === 'multiple_choice'" class="options">
      <label
        v-for="opt in question.options"
        :key="opt.id"
        class="option"
      >
        <input
          type="checkbox"
          :checked="selectedOptionIds.includes(opt.id)"
          @change="toggleOption(opt.id, $event.target.checked)"
        />
        {{ opt.text }}
      </label>
    </div>

    <div v-else-if="question.question_type === 'text'" class="text-input">
      <textarea
        :value="modelValue?.value_text ?? ''"
        @input="emitValue({ value_text: $event.target.value })"
        placeholder="Введите ответ"
        rows="3"
      ></textarea>
    </div>

    <div v-else>
      Тип вопроса "{{ question.question_type }}" не поддерживается
    </div>
  </div>
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
  margin-bottom: 1.5rem;
  background: #fff;
  padding: 1.25rem;
  border-radius: 8px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.04);
}
.question-text {
  font-weight: 600;
  margin-bottom: 0.75rem;
}
.options {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}
.option {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
}
.numeric-input input,
.text-input textarea {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid #ccc;
  border-radius: 4px;
}
</style>
