import { defineStore } from 'pinia'
import { authApi } from '@/services/authApi'

const STORAGE_TOKEN_KEY = 'auth_token'
const STORAGE_USER_KEY = 'auth_user'

function saveToken(token) {
  try {
    if (token) localStorage.setItem(STORAGE_TOKEN_KEY, token)
    else localStorage.removeItem(STORAGE_TOKEN_KEY)
  } catch (e) {}
}

function saveUser(user) {
  try {
    if (user) localStorage.setItem(STORAGE_USER_KEY, JSON.stringify(user))
    else localStorage.removeItem(STORAGE_USER_KEY)
  } catch (e) {}
}

export const useAuthStore = defineStore('auth', {
  state: () => ({
    accessToken: (typeof localStorage !== 'undefined' && localStorage.getItem(STORAGE_TOKEN_KEY)) || '',
    user: (typeof localStorage !== 'undefined' && JSON.parse(localStorage.getItem(STORAGE_USER_KEY) || 'null')) || null,
  }),
  actions: {
    setAccessToken(token) {
      this.accessToken = token
      saveToken(token)
    },
    setUser(user) {
      this.user = user
      saveUser(user)
    },
    async fetchMe() {
      try {
        const u = await authApi.me()
        this.setUser(u)
        return u
      } catch (err) {
        this.setUser(null)
        this.setAccessToken('')
        return null
      }
    },
    async logout() {
      try {
        await authApi.logout()
      } catch {}
      this.setAccessToken('')
      this.setUser(null)
    },
  },
})
