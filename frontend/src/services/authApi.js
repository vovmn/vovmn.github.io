import { http } from './http'

export const authApi = {
  async login(payload) {
    const { data } = await http.post('/auth/login', payload)
    return data
  },
  async me() {
    try {
      const { data } = await http.get('/auth/me')
      return data
    } catch (error) {
      console.error('❌ Me request failed:', error.response?.status, error.message)
      throw error
    }
  },
  async logout() {
    console.log('🚪 Logging out')
    const { data } = await http.post('/auth/logout')
    return data
  },
}
