<template>
  <body class="body">
    <Header />
    
    <div class="container">

      <div class="page-header">
        <h1 class="page-title">Анализы и выписки</h1>
        <router-link to="/record" >
            <button class="appointment-btn">Запись на прием</button>
        </router-link>
        
      </div>


      <div class="filters-panel">
        <div class="filter-group">
          <label class="filter-label">Плюс по названию</label>
          <input 
            type="text" 
            class="filter-input" 
            placeholder="Введите название документа..."
            v-model="searchQuery"
          >
        </div>

        <div class="filter-group">
          <label class="filter-label">Тип документа</label>
          <select class="filter-select" v-model="selectedType">
            <option value="all">Все типы</option>
            <option value="analysis">Анализы</option>
            <option value="extract">Выписки</option>
            <option value="result">Результаты</option>
          </select>
        </div>

        <div class="filter-group">
          <label class="filter-label">Период</label>
          <select class="filter-select" v-model="selectedPeriod">
            <option value="all">За все время</option>
            <option value="week">За неделю</option>
            <option value="month">За месяц</option>
            <option value="year">За год</option>
          </select>
        </div>

        <button class="apply-btn">Применить</button>
      </div>

      <div class="documents-content">
        <DocumentCard 
          v-for="(doc, index) in filteredDocuments" 
          :key="index"
          :document="doc"
        />
      </div>
    </div>
  </body>
</template>

<script setup>
import { ref, computed } from 'vue'
import Header from '../components/header.vue'
import DocumentCard from '../components/DocumentCard.vue'

const searchQuery = ref('')
const selectedType = ref('all')
const selectedPeriod = ref('all')


const documents = ref([
  {
    id: 1,
    title: 'Общий анализ крови',
    type: 'analysis',
    date: '2024-01-15',
    doctor: 'Иванов А.П.'
  },
  {
    id: 2,
    title: 'Выписка из стационара',
    type: 'extract',
    date: '2024-01-10',
    doctor: 'Петрова М.В.'
  },
  {
    id: 3,
    title: 'Биохимический анализ',
    type: 'analysis',
    date: '2024-01-08',
    doctor: 'Сидоров К.Л.'
  },
  {
    id: 4,
    title: 'Результаты МРТ',
    type: 'result',
    date: '2024-01-05',
    doctor: 'Козлов Д.С.'
  },
  {
    id: 5,
    title: 'Выписка амбулаторная',
    type: 'extract',
    date: '2023-12-20',
    doctor: 'Николаева О.И.'
  }
])

const filteredDocuments = computed(() => {
  return documents.value.filter(doc => {
    const matchesSearch = doc.title.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchesType = selectedType.value === 'all' || doc.type === selectedType.value
    return matchesSearch && matchesType
  })
})
</script>

<style scoped>
.body {
  background-color: #f5f7fa;
  color: #333;
  margin: 0;
  padding: 0;
  box-sizing: border-box;
  
}

.container {
  min-height: calc(100vh - 80px);
  gap: 1.5rem;
  padding: 2rem;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.page-title {
  font-size: 1.8rem;
  font-weight: 600;
  color: #2c3e50;
  margin: 0;
}

.appointment-btn {
  background-color: #3498db;
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 8px;
  font-weight: 500;
  cursor: pointer;
  transition: background-color 0.3s;
}

.appointment-btn:hover {
  background-color: #2980b9;
}

.filters-panel {
  background: white;
  padding: 1.5rem;
  border-radius: 12px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  display: flex;
  gap: 1.5rem;
  align-items: end;
  margin-bottom: 2rem;
  flex-wrap: wrap;
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  flex: 1;
  min-width: 200px;
}

.filter-label {
  font-size: 0.9rem;
  font-weight: 500;
  color: #555;
}

.filter-input,
.filter-select {
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 8px;
  font-size: 0.9rem;
  background-color: #fafafa;
  transition: border-color 0.3s;
}

.filter-input:focus,
.filter-select:focus {
  outline: none;
  border-color: #3498db;
}

.apply-btn {
  background-color: #27ae60;
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 8px;
  font-weight: 500;
  cursor: pointer;
  transition: background-color 0.3s;
  height: fit-content;
}

.apply-btn:hover {
  background-color: #219a52;
}

.documents-content {
  display: grid;
  gap: 1.5rem;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
}

/* Адаптивность */
@media (max-width: 768px) {
  .container {
    padding: 1rem;
  }
  
  .page-header {
    flex-direction: column;
    gap: 1rem;
    align-items: flex-start;
  }
  
  .filters-panel {
    flex-direction: column;
    align-items: stretch;
  }
  
  .filter-group {
    min-width: auto;
  }
}
</style>