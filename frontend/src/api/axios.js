import axios from 'axios'

const api = axios.create({
  baseURL: '/',            // важно для прокси Vite
  withCredentials: true,   // для рефреш-куки
})

// Добавление токена
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('access_token')   // <-- ТВОЙ КЛЮЧ
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Автообновление при 401 (оставь как было, но ключ тоже поправь)
let isRefreshing = false
let failedQueue = []

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config
    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject })
        }).then((token) => {
          originalRequest.headers.Authorization = `Bearer ${token}`
          return api(originalRequest)
        })
      }

      originalRequest._retry = true
      isRefreshing = true

      try {
        const { data } = await axios.post(
          `${api.defaults.baseURL}/auth/refresh`,
          {},
          { withCredentials: true }
        )
        const newToken = data.access_token
        localStorage.setItem('access_token', newToken)   // <-- сохраняем под тем же ключом
        api.defaults.headers.common.Authorization = `Bearer ${newToken}`
        failedQueue.forEach((prom) => prom.resolve(newToken))
        failedQueue = []
        return api(originalRequest)
      } catch (refreshError) {
        failedQueue.forEach((prom) => prom.reject(refreshError))
        failedQueue = []
        localStorage.removeItem('access_token')   // чистим
        window.location.href = '/login'
        return Promise.reject(refreshError)
      } finally {
        isRefreshing = false
      }
    }
    return Promise.reject(error)
  }
)

export default api