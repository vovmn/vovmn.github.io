//Библиотека для создания соеденения по http(GET, POST)
import axios from 'axios'
import { useAuthStore } from '@/stores/auth'

// A production build always uses same-origin paths handled by nginx. This also
// prevents a developer's local .env from embedding localhost into the image.
const baseURL = import.meta.env.DEV
  ? (import.meta.env.VITE_API_URL || 'http://localhost:5000')
  : ''

export const http = axios.create({
  baseURL, // например https://api.site.com
  withCredentials: true, // важно для refresh-cookie варианта
  timeout: 15000,
})

//запросы идят вместе с токеном
http.interceptors.request.use((config) => {
  try {
    const auth = useAuthStore()
    
    if (!config.headers) {
      config.headers = {}
    }
    
    if (auth.accessToken) {
      config.headers.Authorization = `Bearer ${auth.accessToken}`
    }
  } catch (err) {
    console.error('❌ Error in request interceptor:', err)
  }
  return config
})

// Авто-рефреш при 401 (если используешь refresh-cookie)
let isRefreshing = false
let queue = []

function resolveQueue(error, token = null) {
  queue.forEach(({ resolve, reject }) => {
    if (error) reject(error)
    else resolve(token)
  })
  queue = []
}

http.interceptors.response.use(
  (r) => r,
  async (error) => {
    const auth = useAuthStore()
    const original = error.config

    if (error.response?.status === 401 && !original._retry) {
      original._retry = true

      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          queue.push({ resolve, reject })
        }).then(() => http(original))
      }

      isRefreshing = true
      try {
        const { data } = await axios.post(`${baseURL}/auth/refresh`, {}, { withCredentials: true })
        auth.setAccessToken(data.access_token)
        resolveQueue(null, data.access_token)
        return http(original)
      } catch (e) {
        resolveQueue(e, null)
        auth.logout()
        throw e
      } finally {
        isRefreshing = false
      }
    }

    throw error
  },
)
