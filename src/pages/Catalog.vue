<template>
  <div class="catalog-page">
    <Header />

    <main class="catalog-main">
      <div class="container">
        <h1 class="page-title">Каталог продукции</h1>

        <button
          v-if="activeFilter !== 'all' || searchQuery || currentPage !== 1"
          @click="resetAllFilters"
          class="reset-filters-btn"
        >
          Сбросить все фильтры
        </button>

        <!-- Поле поиска -->
        <div class="search-container">
          <div class="search-box">
            <svg class="search-icon" viewBox="0 0 24 24" width="20" height="20">
              <path
                fill="currentColor"
                d="M15.5 14h-.79l-.28-.27A6.471 6.471 0 0 0 16 9.5 6.5 6.5 0 1 0 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14z"
              />
            </svg>

            <input
              v-model="searchQuery"
              type="text"
              class="search-input"
              placeholder="Поиск товаров..."
              @input="handleSearch"
            />

            <button
              v-if="searchQuery"
              @click="clearSearch"
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

          <div v-if="searchQuery" class="search-info">
            <span>Найдено товаров: {{ filteredProducts.length }}</span>
            <button @click="clearSearch" class="clear-all-btn">
              Очистить поиск
            </button>
          </div>
        </div>

        <!-- Загрузка данных -->
        <div v-if="loading" class="loading">
          <div class="spinner"></div>
          <p>Загрузка каталога...</p>
        </div>

        <!-- Ошибка загрузки -->
        <div v-else-if="error" class="error">
          <p>Ошибка загрузки каталога: {{ error }}</p>
          <button @click="reloadProducts" class="retry-btn">Попробовать снова</button>
        </div>

        <!-- Основной контент -->
        <div v-else>
          <!-- Фильтры по категориям -->
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

          <!-- Сообщение при отсутствии результатов поиска -->
          <div v-if="searchQuery && filteredProducts.length === 0" class="no-results">
            <svg class="no-results-icon" viewBox="0 0 24 24" width="48" height="48">
              <path
                fill="#6b7280"
                d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-2h2v2zm0-4h-2V7h2v6z"
              />
            </svg>
            <p class="no-results-title">Товары не найдены</p>
            <p class="no-results-text">
              По запросу "{{ searchQuery }}" ничего не найдено.
              <br />Попробуйте изменить поисковый запрос или выберите другую категорию.
            </p>
            <button @click="clearSearch" class="back-to-catalog-btn">
              Вернуться к каталогу
            </button>
          </div>

          <template v-else-if="filteredProducts.length > 0">
            <div class="pagination-info">
              <div class="pagination-stats">
                Показано {{ startItem }}-{{ endItem }} из {{ filteredProducts.length }} товаров
              </div>
            </div>

            <!-- ✅ ВАЖНО: передаем index -->
            <div class="products-grid">
              <CardCatalog
                v-for="(product, i) in paginatedProducts"
                :key="product.id"
                :product="product"
                :index="pageStartIndex + i"
                :eager-count="8"
                :high-priority-count="2"
                @details-click="openModal"
                @card-click="openModal"
              />
            </div>

            <div v-if="totalPages > 1" class="pagination">
              <button
                :disabled="currentPage === 1"
                @click="changePage(currentPage - 1)"
                class="pagination-btn pagination-prev"
              >
                <svg viewBox="0 0 24 24" width="16" height="16">
                  <path fill="currentColor" d="M15.41 7.41L14 6l-6 6 6 6 1.41-1.41L10.83 12z" />
                </svg>
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
                <span v-if="showEllipsis" class="pagination-ellipsis">...</span>
              </div>

              <button
                :disabled="currentPage === totalPages"
                @click="changePage(currentPage + 1)"
                class="pagination-btn pagination-next"
              >
                Вперед
                <svg viewBox="0 0 24 24" width="16" height="16">
                  <path fill="currentColor" d="M10 6L8.59 7.41 13.17 12l-4.58 4.59L10 18l6-6z" />
                </svg>
              </button>
            </div>
          </template>

          <div v-else class="no-products">
            <p>Товары в этой категории скоро появятся</p>
          </div>
        </div>
      </div>
    </main>

    <ModalPriceSizes
      v-if="showModal"
      :product="selectedProduct"
      @close="closeModal"
    />

    <Footer />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Header from '@/components/Header.vue'
import Footer from '@/components/Footer.vue'
import CardCatalog from '@/components/CardCatalog.vue'
import ModalPriceSizes from '@/components/ModalPriceSizes.vue'
import { useProducts } from '@/composables/useProducts'

const ITEMS_PER_PAGE = 21

const route = useRoute()
const router = useRouter()

const {
  productsData,
  loading,
  error,
  getFilteredProducts: getFilteredProductsFromComposable,
  getAllCategories,
  reloadProducts
} = useProducts()

const categories = getAllCategories()

const parseUrlParams = () => {
  const params = route.query
  return {
    filter: params.filter && categories.some(c => c.id === params.filter) ? params.filter : 'all',
    page: params.page && parseInt(params.page) > 0 ? parseInt(params.page) : 1,
    search: params.search || ''
  }
}

const urlParams = parseUrlParams()

const activeFilter = ref(urlParams.filter)
const searchQuery = ref(urlParams.search)
const currentPage = ref(urlParams.page)

const showModal = ref(false)
const selectedProduct = ref(null)
const isChangingPage = ref(false)

/** ✅ Индекс начала текущей страницы, чтобы приоритеты картинок работали нормально */
const pageStartIndex = computed(() => (currentPage.value - 1) * ITEMS_PER_PAGE)

const updateUrl = () => {
  const query = {}
  if (activeFilter.value !== 'all') query.filter = activeFilter.value
  if (currentPage.value !== 1) query.page = String(currentPage.value)
  if (searchQuery.value) query.search = searchQuery.value
  router.replace({ query, hash: route.hash })
}

watch(activeFilter, updateUrl)
watch(currentPage, updateUrl)
watch(searchQuery, updateUrl)

watch([activeFilter, searchQuery], () => {
  currentPage.value = 1
})

const safeSearch = (products, query) => {
  if (!query || query.trim() === '') return products
  const normalizedQuery = query.toLowerCase().trim()

  return products.filter(product => {
    if (!product || typeof product !== 'object') return false
    const name = String(product.name || '').toLowerCase()
    const description = String(product.description || '').toLowerCase()
    const category = String(product.category || '').toLowerCase()

    return name.includes(normalizedQuery) ||
      description.includes(normalizedQuery) ||
      category.includes(normalizedQuery)
  })
}

const filteredProducts = computed(() => {
  let products = getFilteredProductsFromComposable(activeFilter.value)
  if (searchQuery.value.trim()) products = safeSearch(products, searchQuery.value)
  return products
})

const paginatedProducts = computed(() => {
  const startIndex = (currentPage.value - 1) * ITEMS_PER_PAGE
  return filteredProducts.value.slice(startIndex, startIndex + ITEMS_PER_PAGE)
})

const totalPages = computed(() => Math.ceil(filteredProducts.value.length / ITEMS_PER_PAGE))

const startItem = computed(() => (currentPage.value - 1) * ITEMS_PER_PAGE + 1)
const endItem = computed(() => {
  const end = currentPage.value * ITEMS_PER_PAGE
  return end > filteredProducts.value.length ? filteredProducts.value.length : end
})

watch(totalPages, (newTotal) => {
  if (currentPage.value > newTotal && newTotal > 0) currentPage.value = newTotal
})

const visiblePages = computed(() => {
  const maxVisible = 5
  const pages = []
  if (totalPages.value <= maxVisible) {
    for (let i = 1; i <= totalPages.value; i++) pages.push(i)
    return pages
  }

  let start = Math.max(1, currentPage.value - 2)
  let end = Math.min(totalPages.value, start + maxVisible - 1)
  if (end - start + 1 < maxVisible) start = Math.max(1, end - maxVisible + 1)

  for (let i = start; i <= end; i++) pages.push(i)
  return pages
})

const showEllipsis = computed(() => totalPages.value > visiblePages.value.length)

let searchTimeout = null
const handleSearch = () => {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    if (searchQuery.value.trim() && activeFilter.value !== 'all') {
      activeFilter.value = 'all'
    }
  }, 300)
}

const setActiveFilter = (categoryId) => {
  activeFilter.value = categoryId
  searchQuery.value = ''
}

const clearSearch = () => {
  searchQuery.value = ''
}

const changePage = (page) => {
  if (isChangingPage.value) return
  if (page < 1 || page > totalPages.value) return

  isChangingPage.value = true
  currentPage.value = page

  requestAnimationFrame(() => {
    window.scrollTo({ top: 0, behavior: 'smooth' })
    setTimeout(() => (isChangingPage.value = false), 400)
  })
}

const openModal = (product) => {
  selectedProduct.value = product
  showModal.value = true
  document.body.style.overflow = 'hidden'
}

const closeModal = () => {
  showModal.value = false
  selectedProduct.value = null
  document.body.style.overflow = 'auto'
}

const handleKeyDown = (event) => {
  if (event.key === 'Escape' && showModal.value) closeModal()
}

const resetAllFilters = () => {
  activeFilter.value = 'all'
  searchQuery.value = ''
  currentPage.value = 1
  router.replace({ name: route.name })
}

watch(() => route.query, () => {
  const newParams = parseUrlParams()
  activeFilter.value = newParams.filter
  currentPage.value = newParams.page
  searchQuery.value = newParams.search
})

onMounted(() => {
  document.addEventListener('keydown', handleKeyDown)
})

onUnmounted(() => {
  document.removeEventListener('keydown', handleKeyDown)
  clearTimeout(searchTimeout)
})
</script>

<!-- style оставь как у тебя -->


<style scoped>
/* ТВОЙ CSS ОСТАВЛЯЮ 1в1 — без изменений */
  
/* ТВОЙ CSS (как был) + sticky + улучшенный адаптив */

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

/* ======================================================
   STICKY ВАРИАНТ (поиск + категории)
   Работает без изменений template, но идеал — обернуть
   search + category-filters в один контейнер.
   Тут делаем sticky на оба блока по очереди.
====================================================== */

/* sticky-поведение для поиска */
.search-container {
  position: sticky;
  top: 0;
  z-index: 30;
  background: #f8f9fa;
  padding-top: 0.75rem;
  padding-bottom: 0.75rem;
  margin-bottom: 1.5rem;
  /* лёгкое отделение от контента */
  box-shadow: 0 6px 14px rgba(0, 0, 0, 0.04);
}

/* sticky-поведение для категорий: липнут сразу под поиском */
.category-filters {
  position: sticky;
  top: 96px; /* подгони под высоту search-container, если нужно */
  z-index: 25;
  background: #f8f9fa;
  padding: 0.5rem 0.25rem 0.75rem;
  margin-bottom: 2rem;
}

/* Важно: чтобы sticky не выглядел как “плашка от госуслуг” */
@media (max-width: 768px) {
  .search-container {
    padding-top: 0.5rem;
    padding-bottom: 0.5rem;
    margin-bottom: 1rem;
  }

  .category-filters {
    top: 88px; /* обычно чуть меньше */
    padding-bottom: 0.6rem;
    margin-bottom: 1.25rem;
  }
}

@media (max-width: 480px) {
  .category-filters {
    top: 84px;
  }
}

/* Если есть модалка/скролл и нужно отключить тень — можно убрать */

/* ======================================================
   Улучшенный адаптив (чтобы было реально удобно)
====================================================== */

/* 1) Контейнер и отступы */
.catalog-main {
  padding: 1.5rem 0;
}

.container {
  padding: 0 1rem;
}

@media (max-width: 768px) {
  .catalog-main {
    padding: 1rem 0;
  }

  .container {
    padding: 0 0.75rem;
  }
}

@media (max-width: 480px) {
  .container {
    padding: 0 0.6rem;
  }
}

/* 2) Заголовок */
@media (max-width: 768px) {
  .page-title {
    font-size: 2rem;
    margin-bottom: 1.25rem;
  }
}
@media (max-width: 480px) {
  .page-title {
    font-size: 1.6rem;
    margin-bottom: 1rem;
  }
}

/* 3) Поиск компактнее */
@media (max-width: 768px) {
  .search-container {
    margin-bottom: 1.25rem;
  }

  .search-input {
    font-size: 0.95rem;
  }
}
@media (max-width: 480px) {
  .search-box {
    margin-bottom: 0.75rem;
  }

  .search-input {
    padding: 0.8rem 2.5rem 0.8rem 2.4rem;
    border-radius: 10px;
  }

  .search-icon {
    left: 0.8rem;
  }

  .clear-search-btn {
    right: 0.5rem;
  }

  .search-info {
    flex-direction: column;
    gap: 0.5rem;
    text-align: center;
  }

  .clear-all-btn {
    width: 100%;
    max-width: 320px;
  }
}

/* 4) Категории: на мобиле горизонтальный скролл вместо колонки */
@media (max-width: 480px) {
  .category-filters {
    flex-direction: row;
    justify-content: flex-start;
    flex-wrap: nowrap;
    overflow-x: auto;
    -webkit-overflow-scrolling: touch;
    gap: 0.5rem;
    padding: 0.25rem 0.25rem 0.6rem;
  }

  .category-filters::-webkit-scrollbar {
    height: 6px;
  }
  .category-filters::-webkit-scrollbar-thumb {
    background: #e5e7eb;
    border-radius: 999px;
  }

  .filter-btn {
    width: auto;
    flex: 0 0 auto;
    padding: 0.55rem 1rem;
    font-size: 0.9rem;
    border-radius: 999px;
  }
}

/* 5) Сетка карточек: предсказуемые колонки */
.products-grid {
  grid-template-columns: repeat(4, minmax(0, 1fr));
}

@media (max-width: 1200px) {
  .products-grid {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}

@media (max-width: 900px) {
  .products-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 1.25rem;
  }
}

@media (max-width: 560px) {
  .products-grid {
    grid-template-columns: 1fr;
    gap: 1rem;
  }
}

/* 6) Пагинация */
@media (max-width: 768px) {
  .pagination {
    gap: 0.5rem;
    padding-top: 0.75rem;
  }

  .pagination-btn {
    min-width: 110px;
    padding: 0.45rem 0.75rem;
  }

  .pagination-number {
    min-width: 36px;
    height: 36px;
  }
}

@media (max-width: 480px) {
  .pagination {
    flex-wrap: wrap;
  }

  .pagination-numbers {
    width: 100%;
    justify-content: center;
    margin: 0.5rem 0;
  }

  .pagination-btn {
    flex: 1 1 140px;
    min-width: unset;
  }
}

/* 7) Нет результатов */
@media (max-width: 480px) {
  .no-results {
    padding: 2rem 1rem;
  }

  .no-results-title {
    font-size: 1.25rem;
  }

  .no-results-text {
    margin-bottom: 1.25rem;
  }

  .back-to-catalog-btn {
    width: 100%;
  }
}

/* 8) Сброс фильтров */
@media (max-width: 480px) {
  .reset-filters-btn {
    width: 100%;
  }
}

/* 9) Твой старый адаптив (оставляю, но он теперь частично перекрыт сверху) */
@media (max-width: 768px) {
  .search-box {
    max-width: 100%;
  }

  .search-input {
    padding: 0.875rem 2.5rem 0.875rem 2.5rem;
  }

  .category-filters {
    gap: 0.5rem;
  }

  .filter-btn {
    padding: 0.6rem 1.2rem;
    font-size: 0.9rem;
  }

  .products-grid {
    gap: 1.5rem;
  }

  .pagination {
    flex-wrap: wrap;
    gap: 0.5rem;
  }

  .pagination-numbers {
    order: 1;
    width: 100%;
    justify-content: center;
    margin: 0.5rem 0;
  }

  .pagination-btn {
    min-width: 120px;
  }
}

@media (max-width: 480px) {
  .products-grid {
    grid-template-columns: 1fr;
  }

  .pagination-number {
    min-width: 35px;
    height: 35px;
    font-size: 0.85rem;
  }
}
/* === FIX: инпут/поиск не вылезает за экран === */

/* 1) Глобально для этой страницы: чтобы padding/border не раздували ширину */
.catalog-page,
.catalog-page * {
  box-sizing: border-box;
}

/* 2) Контейнер точно не шире экрана */
.container {
  width: 100%;
  max-width: 1200px;
  overflow-x: hidden; /* убирает случайный горизонтальный скролл */
}

/* 3) Поиск: ограничиваем ширину и позволяем сжиматься */
.search-container {
  width: 100%;
}

.search-box {
  width: 100%;
  max-width: 600px;
  margin-left: auto;
  margin-right: auto;
}

/* 4) Инпут: главный фикс — min-width: 0 и box-sizing */
.search-input {
  width: 100%;
  min-width: 0;
  box-sizing: border-box;
}

/* 5) На всякий случай: чтобы иконки/кнопки не ломали ширину */
.search-icon,
.clear-search-btn {
  flex: 0 0 auto;
}


</style>
