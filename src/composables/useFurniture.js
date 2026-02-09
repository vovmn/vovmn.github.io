    // src/composables/useFurniture.js
import { ref, onMounted } from 'vue'

export function useFurniture() {
  const furniture = ref([])
  const loading = ref(false)
  const error = ref(null)

  const loadFurniture = async () => {
    try {
      loading.value = true
      const url = new URL('../data/furniture.json', import.meta.url)
      const res = await fetch(url)
      if (!res.ok) throw new Error(res.status)
      furniture.value = await res.json()
    } catch (e) {
      error.value = String(e)
    } finally {
      loading.value = false
    }
  }

  onMounted(loadFurniture)

  return {
    furniture,
    loading,
    error
  }
}
