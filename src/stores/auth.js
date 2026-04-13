import { defineStore } from 'pinia'
import { authApi } from '@/services/authApi'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    accessToken: '',
    user: null,
  }),
  actions: {
    setAccessToken(token) {
      this.accessToken = token
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
    },
  },
})
