// src/composables/useProducts.js
import { ref, onMounted } from 'vue'

/**
 * Подхватываем ВСЕ картинки из src/img, чтобы Vite включил их в сборку
 * Ключи будут выглядеть примерно так: "/src/img/....jpg"
 */
const imageModules = import.meta.glob('/src/img/**/*.{png,jpg,jpeg,JPG,webp,svg}', {
  eager: true,
  import: 'default'
})

function resolveImagePath(p) {
  if (!p) return ''
  // приводим к виду "/src/img/...."
  const key = p.startsWith('/') ? p : `/${p}`

  // если картинка есть в мапе — берем собранный URL, иначе оставляем как есть
  return imageModules[key] || key
}

export function useProducts() {
  const productsData = ref([])
  const loading = ref(true)
  const error = ref(null)

  const categories = ref([
    { id: 'all', name: 'Все товары' },
    { id: 'ldsp', name: 'ЛДСП' },
    { id: 'countertops', name: 'Столешницы' },
    { id: 'facades', name: 'Фасады из массива' },
    { id: 'furniture', name: 'Фурнитура' }
  ])

  const loadProducts = async () => {
    try {
      loading.value = true
      error.value = null

      // ✅ правильная загрузка JSON для build
      const url = new URL('../data/products.json', import.meta.url)
      const response = await fetch(url)

      if (!response.ok) throw new Error(`Ошибка загрузки: ${response.status}`)

      const data = await response.json()

      const raw = data.products || []

      // ✅ резолвим картинки так, чтобы они работали в build
      productsData.value = raw.map(p => ({
        ...p,
        image: resolveImagePath(p.image)
      }))
    } catch (err) {
      error.value = err?.message || String(err)
      console.error('Ошибка при загрузке товаров:', err)
    } finally {
      loading.value = false
    }
  }

  const getFilteredProducts = (activeFilter) => {
    if (activeFilter === 'all') return productsData.value
    return productsData.value.filter(product => product.category === activeFilter)
  }

  const getProductById = (id) => {
    return productsData.value.find(product => product.id === id)
  
  }



function resolveImagePath(p) {
  if (!p) return imageModules["/src/img/placeholder.webp"] || ""

  const key = p.startsWith("/") ? p : `/${p}`
  return imageModules[key] || imageModules["/src/img/placeholder.webp"] || ""
}


  const getAllCategories = () => categories.value

  onMounted(() => {
    loadProducts()
  })

  return {
    productsData,
    loading,
    error,
    loadProducts,         
    getFilteredProducts,
    getProductById,
    getAllCategories,
    reloadProducts: loadProducts
  }
}
