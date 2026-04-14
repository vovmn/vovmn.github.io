import { defineStore } from 'pinia'
import { authApi } from '@/services/authApi'

const TOKEN_KEY = 'access_token'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    accessToken: localStorage.getItem(TOKEN_KEY) || '',
    user: null,
  }),
  actions: {
    setAccessToken(token) {
      this.accessToken = token
      if (token) {
        localStorage.setItem(TOKEN_KEY, token)
      } else {
        localStorage.removeItem(TOKEN_KEY)
      }
    },
    async fetchMe() {
      this.user = await authApi.me()
    },
    async logout() {
      try {
        await authApi.logout()
      } catch {}
      this.accessToken = ''
      this.user = null
      localStorage.removeItem(TOKEN_KEY)
    },
  },
})
