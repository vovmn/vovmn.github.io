<!-- components/DocumentCard.vue -->
<template>
  <div class="document-card">
    <div class="document-header">
      <span class="document-type" :class="typeClass">{{ typeLabel }}</span>
      <span class="document-date">{{ formattedDate }}</span>
    </div>
    
    <h3 class="document-title">{{ document.title }}</h3>
    
    <div class="document-info">
      <div class="info-item">
        <span class="info-label">Врач:</span>
        <span class="info-value">{{ document.doctor }}</span>
      </div>
    </div>
    
    <div class="document-actions">
      <button class="action-btn view-btn">Просмотреть</button>
      <button class="action-btn download-btn">Скачать</button>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  document: {
    type: Object,
    required: true
  }
})

const typeLabels = {
  analysis: 'Анализ',
  extract: 'Выписка',
  result: 'Результат'
}

const typeLabel = computed(() => typeLabels[props.document.type] || 'Документ')

const typeClass = computed(() => `type-${props.document.type}`)

const formattedDate = computed(() => {
  return new Date(props.document.date).toLocaleDateString('ru-RU')
})
</script>

<style scoped>
.document-card {
  background: white;
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  transition: transform 0.3s, box-shadow 0.3s;
}

.document-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.15);
}

.document-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.document-type {
  padding: 0.25rem 0.75rem;
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 500;
}

.type-analysis {
  background-color: #e8f4fd;
  color: #3498db;
}

.type-extract {
  background-color: #f0f8f0;
  color: #27ae60;
}

.type-result {
  background-color: #fef5e7;
  color: #f39c12;
}

.document-date {
  font-size: 0.9rem;
  color: #7f8c8d;
}

.document-title {
  font-size: 1.1rem;
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 1rem 0;
  line-height: 1.4;
}

.document-info {
  margin-bottom: 1.5rem;
}

.info-item {
  display: flex;
  justify-content: space-between;
  margin-bottom: 0.5rem;
}

.info-label {
  font-weight: 500;
  color: #555;
}

.info-value {
  color: #333;
}

.document-actions {
  display: flex;
  gap: 0.75rem;
}

.action-btn {
  flex: 1;
  padding: 0.6rem;
  border: none;
  border-radius: 6px;
  font-size: 0.9rem;
  font-weight: 500;
  cursor: pointer;
  transition: background-color 0.3s;
}

.view-btn {
  background-color: #3498db;
  color: white;
}

.view-btn:hover {
  background-color: #2980b9;
}

.download-btn {
  background-color: #ecf0f1;
  color: #2c3e50;
}

.download-btn:hover {
  background-color: #d5dbdb;
}
</style>