import { http } from './http'

export const authApi = {
  async login(payload) {
    const { data } = await http.post('/auth/login', payload)
    return data
  },
  async me() {
    const { data } = await http.get('/auth/me')
    return data
  },
  async logout() {
    const { data } = await http.post('/auth/logout')
    return data
  },
}
