import { http } from './http'

export const authApi = {
  async login(payload) {
    console.log('📝 Logging in with:', payload)
    const { data } = await http.post('/auth/login', payload)
    console.log('✅ Login response:', data)
    return data
  },
  async register(payload) {
    console.log('📝 Registering with:', payload)
    const { data } = await http.post('/auth/register', payload)
    console.log('✅ Register response:', data)
    return data
  },
  async me() {
    console.log('👤 Fetching /auth/me')
    try {
      const { data } = await http.get('/auth/me')
      console.log('✅ Me response:', data)
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
