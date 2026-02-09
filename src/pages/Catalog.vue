<template>
  <div class="catalog-page">
    <Header />

    <main class="catalog-main">
      <div class="container">
        <h1 class="page-title">Каталог продукции</h1>

        <button
          v-if="activeFilter !== 'all' || appliedSearch || currentPage !== 1"
          @click="resetAllFilters"
          class="reset-filters-btn"
        >
          Сбросить все фильтры
        </button>

        <!-- ПОИСК -->
        <div class="search-container">
          <div class="search-box">
            <svg class="search-icon" viewBox="0 0 24 24" width="20" height="20">
              <path
                fill="currentColor"
                d="M15.5 14h-.79l-.28-.27A6.471 6.471 0 0 0 16 9.5 6.5 6.5 0 1 0 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14z"
              />
            </svg>

            <input
              v-model="searchInput"
              type="text"
              class="search-input"
              placeholder="Поиск товаров..."
              @keydown.enter.prevent="applySearch"
            />

            <button
              v-if="searchInput"
              @click="clearSearchInput"
              class="clear-search-btn"
              type="button"
            >
              <svg viewBox="0 0 24 24" width="18" height="18">
                <path
                  fill="currentColor"
                  d="M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z"
                />
              </svg>
            </button>
          </div>

          <div class="search-info" v-if="searchInput || appliedSearch">
            <span v-if="appliedSearch">
              Найдено товаров: {{ filteredProducts.length }}
            </span>
            <span v-else>Введите запрос и нажмите Enter</span>

            <div style="display:flex; gap:.5rem;">
              <button
                class="clear-all-btn"
                @click="applySearch"
                :disabled="!searchInput.trim()"
              >
                Найти
              </button>

              <button
                v-if="appliedSearch"
                class="clear-all-btn"
                @click="clearAppliedSearch"
              >
                Очистить поиск
              </button>
            </div>
          </div>
        </div>

        <!-- КАТЕГОРИИ -->
        <div class="category-filters">
          <button
            v-for="category in categories"
            :key="category.id"
            :class="['filter-btn', { active: activeFilter === category.id }]"
            @click="setActiveFilter(category.id)"
          >
            {{ category.name }}
          </button>
        </div>

        <!-- КОНТЕНТ -->
        <div v-if="loading" class="loading">
          <div class="spinner"></div>
          <p>Загрузка каталога...</p>
        </div>

        <div v-else-if="error" class="error">
          <p>Ошибка загрузки каталога: {{ error }}</p>
        </div>

        <div v-else>
          <template v-if="filteredProducts.length > 0">
            <div class="pagination-info">
              <div class="pagination-stats">
                Показано {{ startItem }}–{{ endItem }} из {{ filteredProducts.length }} товаров
              </div>
            </div>

            <!-- ФУРНИТУРА → ТАБЛИЦА -->
            <FurnitureTable
              v-if="activeFilter === 'furniture'"
              :items="paginatedProducts"
            />

            <!-- ОСТАЛЬНЫЕ → КАРТОЧКИ -->
            <div
              v-else
              class="products-grid"
            >
              <CardCatalog
                v-for="(product, i) in paginatedProducts"
                :key="product.id"
                :product="product"
                :index="pageStartIndex + i"
                @details-click="openModal"
                @card-click="openModal"
              />
            </div>

            <div v-if="totalPages > 1" class="pagination">
              <button
                :disabled="currentPage === 1"
                @click="changePage(currentPage - 1)"
                class="pagination-btn"
              >
                Назад
              </button>

              <div class="pagination-numbers">
                <button
                  v-for="page in visiblePages"
                  :key="page"
                  @click="changePage(page)"
                  :class="['pagination-number', { active: currentPage === page }]"
                >
                  {{ page }}
                </button>
              </div>

              <button
                :disabled="currentPage === totalPages"
                @click="changePage(currentPage + 1)"
                class="pagination-btn"
              >
                Вперёд
              </button>
            </div>
          </template>

          <div v-else class="no-products">
            <p>Товары не найдены</p>
          </div>
        </div>
      </div>
    </main>

    <Footer />
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import Header from '@/components/Header.vue'
import Footer from '@/components/Footer.vue'
import CardCatalog from '@/components/CardCatalog.vue'
import FurnitureTable from '@/components/FurnitureTable.vue'
import { useProducts } from '@/composables/useProducts'
import furnitureData from '@/data/furniture.json'

const ITEMS_PER_PAGE = 21

const {
  loading,
  error,
  getFilteredProducts,
  getAllCategories
} = useProducts()

const categories = getAllCategories()

const activeFilter = ref('all')
const currentPage = ref(1)

const searchInput = ref('')
const appliedSearch = ref('')

const pageStartIndex = computed(() =>
  (currentPage.value - 1) * ITEMS_PER_PAGE
)

const filteredProducts = computed(() => {
  let items = []

  if (activeFilter.value === 'furniture') {
    items = furnitureData
  } else {
    items = getFilteredProducts(activeFilter.value)
  }

  if (appliedSearch.value) {
    const q = appliedSearch.value.toLowerCase().trim()

    items = items.filter(item =>
      String(item.name || '').toLowerCase().includes(q) ||
      String(item.description || '').toLowerCase().includes(q) ||
      String(item.group || '').toLowerCase().includes(q)
    )
  }

  return items
})

const paginatedProducts = computed(() => {
  const start = (currentPage.value - 1) * ITEMS_PER_PAGE
  return filteredProducts.value.slice(start, start + ITEMS_PER_PAGE)
})

const totalPages = computed(() =>
  Math.ceil(filteredProducts.value.length / ITEMS_PER_PAGE)
)

const startItem = computed(() =>
  (currentPage.value - 1) * ITEMS_PER_PAGE + 1
)

const endItem = computed(() =>
  Math.min(currentPage.value * ITEMS_PER_PAGE, filteredProducts.value.length)
)

const visiblePages = computed(() => {
  const pages = []
  for (let i = 1; i <= totalPages.value; i++) pages.push(i)
  return pages
})

const applySearch = () => {
  appliedSearch.value = searchInput.value.trim()
  currentPage.value = 1
}

const clearSearchInput = () => {
  searchInput.value = ''
}

const clearAppliedSearch = () => {
  searchInput.value = ''
  appliedSearch.value = ''
  currentPage.value = 1
}

const setActiveFilter = (id) => {
  activeFilter.value = id
  searchInput.value = ''
  appliedSearch.value = ''
  currentPage.value = 1
}

const changePage = (page) => {
  if (page < 1 || page > totalPages.value) return
  currentPage.value = page
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

const resetAllFilters = () => {
  activeFilter.value = 'all'
  searchInput.value = ''
  appliedSearch.value = ''
  currentPage.value = 1
}

const openModal = () => {}
</script>


<style scoped>

.furniture-table-container {
  grid-column: 1 / -1;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
  flex-wrap: wrap;
  gap: 1rem;
}

.reset-filters-btn {
  padding: 0.5rem 1rem;
  background: #f3f4f6;
  color: #6b7280;
  border: 1px solid #e5e7eb;
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.9rem;
  transition: all 0.2s;
}

.reset-filters-btn:hover {
  background: #e5e7eb;
  color: #4b5563;
}

.catalog-page {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.catalog-main {
  flex: 1;
  padding: 2rem 0;
  background: #f8f9fa;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 1rem;
}

.page-title {
  text-align: center;
  font-size: 2.5rem;
  font-weight: 700;
  color: #333;
  margin-bottom: 2rem;
}

.search-container {
  margin-bottom: 2.5rem;
}

.search-box {
  position: relative;
  max-width: 600px;
  margin: 0 auto 1rem;
}

.search-icon {
  position: absolute;
  left: 1rem;
  top: 50%;
  transform: translateY(-50%);
  color: #6b7280;
  pointer-events: none;
}

.search-input {
  width: 100%;
  padding: 1rem 3rem 1rem 2.8rem;
  border: 2px solid #e5e7eb;
  border-radius: 12px;
  font-size: 1rem;
  transition: all 0.3s ease;
  background: white;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.search-input:focus {
  outline: none;
  border-color: #2b6cb0;
  box-shadow: 0 0 0 3px rgba(43, 108, 176, 0.1);
}

.search-input::placeholder {
  color: #9ca3af;
}

.clear-search-btn {
  position: absolute;
  right: 0.75rem;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  color: #9ca3af;
  cursor: pointer;
  padding: 0.25rem;
  border-radius: 50%;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
}

.clear-search-btn:hover {
  background: #f3f4f6;
  color: #6b7280;
}

.search-info {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 1rem;
  color: #6b7280;
  font-size: 0.9rem;
}

.clear-all-btn {
  padding: 0.4rem 0.8rem;
  background: #f3f4f6;
  color: #6b7280;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.85rem;
  transition: all 0.2s;
}

.clear-all-btn:hover {
  background: #e5e7eb;
  color: #4b5563;
}

.loading {
  text-align: center;
  padding: 3rem;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
}

.spinner {
  width: 50px;
  height: 50px;
  border: 4px solid #f3f3f3;
  border-top: 4px solid #2b6cb0;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.error {
  text-align: center;
  padding: 3rem;
  color: #e53e3e;
}

.retry-btn {
  margin-top: 1rem;
  padding: 0.75rem 1.5rem;
  background: #2b6cb0;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 1rem;
  transition: all 0.2s;
}

.retry-btn:hover {
  background: #2c5282;
}

.category-filters {
  display: flex;
  justify-content: center;
  gap: 1rem;
  margin-bottom: 3rem;
  flex-wrap: wrap;
}

.filter-btn {
  padding: 0.75rem 1.5rem;
  border: 2px solid #2b6cb0;
  background: white;
  color: #2b6cb0;
  border-radius: 25px;
  cursor: pointer;
  transition: all 0.3s ease;
  font-weight: 500;
}

.filter-btn:hover {
  background: #2b6cb0;
  color: white;
  transform: translateY(-2px);
}

.filter-btn.active {
  background: #2b6cb0;
  color: white;
}

.pagination-info {
  display: flex;
  justify-content: center;
  margin-bottom: 1.5rem;
}

.pagination-stats {
  background: white;
  padding: 0.5rem 1rem;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  color: #4b5563;
  font-size: 0.9rem;
}

.products-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 2rem;
  margin-bottom: 2rem;
}

.no-results {
  text-align: center;
  padding: 4rem 2rem;
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  margin: 2rem 0;
}

.no-results-icon {
  margin-bottom: 1.5rem;
}

.no-results-title {
  font-size: 1.5rem;
  font-weight: 600;
  color: #374151;
  margin-bottom: 1rem;
}

.no-results-text {
  color: #6b7280;
  line-height: 1.6;
  margin-bottom: 2rem;
  max-width: 500px;
  margin-left: auto;
  margin-right: auto;
}

.back-to-catalog-btn {
  padding: 0.75rem 1.5rem;
  background: #2b6cb0;
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 1rem;
  font-weight: 500;
  transition: all 0.3s;
}

.back-to-catalog-btn:hover {
  background: #2c5282;
  transform: translateY(-2px);
}

.no-products {
  text-align: center;
  padding: 3rem;
  color: #666;
  font-size: 1.1rem;
}

.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 1rem;
  margin-top: 3rem;
  padding: 1rem 0;
  border-top: 1px solid #e5e7eb;
}

.pagination-btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  border: 1px solid #e5e7eb;
  background: white;
  color: #374151;
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.9rem;
  transition: all 0.2s;
  min-width: 80px;
  justify-content: center;
}

.pagination-btn:hover:not(:disabled) {
  border-color: #2b6cb0;
  color: #2b6cb0;
  transform: translateY(-1px);
}

.pagination-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.pagination-numbers {
  display: flex;
  gap: 0.25rem;
}

.pagination-number {
  min-width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px solid #e5e7eb;
  background: white;
  color: #374151;
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.9rem;
  transition: all 0.2s;
}

.pagination-number:hover {
  border-color: #2b6cb0;
  color: #2b6cb0;
}

.pagination-number.active {
  background: #2b6cb0;
  border-color: #2b6cb0;
  color: white;
}

.pagination-ellipsis {
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 40px;
  height: 40px;
  color: #6b7280;
}

/* sticky-поведение для поиска */
.search-container {
  position: sticky;
  top: 0;
  z-index: 30;
  background: #f8f9fa;
  padding-top: 0.75rem;
  padding-bottom: 0.75rem;
  margin-bottom: 1.5rem;
  box-shadow: 0 6px 14px rgba(0, 0, 0, 0.04);
}

/* sticky-поведение для категорий */
.category-filters {
  position: sticky;
  top: 96px;
  z-index: 25;
  background: #f8f9fa;
  padding: 0.5rem 0.25rem 0.75rem;
  margin-bottom: 2rem;
}

/* === FIX: инпут/поиск не вылезает за экран === */
.catalog-page,
.catalog-page * {
  box-sizing: border-box;
}

.container {
  width: 100%;
  max-width: 1200px;
  overflow-x: hidden;
}

.search-container {
  width: 100%;
}

.search-box {
  width: 100%;
  max-width: 600px;
  margin-left: auto;
  margin-right: auto;
}

.search-input {
  width: 100%;
  min-width: 0;
  box-sizing: border-box;
}

.search-icon,
.clear-search-btn {
  flex: 0 0 auto;
}
</style>
