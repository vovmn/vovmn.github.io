import { createApp } from 'vue'
import { createRouter, createWebHashHistory } from 'vue-router'
import { createPinia } from 'pinia'
import App from './App.vue'

// pages
import Info from './pages/Info.vue'
import Home from './pages/Home.vue'
import Register from './pages/Register.vue'
import Documents from './pages/Documents.vue'
import Record from './pages/Record.vue'
import Login from './pages/Login.vue'

const routes = [
  { path: '/', redirect: '/login' },
  { path: '/home', component: Home, name: 'Home' },
  { path: '/login', component: Login, name: 'Login' },
  { path: '/register', component: Register, name: 'Register' },
  { path: '/documents', component: Documents, name: 'Documents' },
  { path: '/record', component: Record, name: 'Record' },
  { path: '/info', component: Info, name: 'Info' },
]

const router = createRouter({
  history: createWebHashHistory(),
  routes,
})

const app = createApp(App)

const pinia = createPinia()
app.use(pinia) // ✅ сначала Pinia
app.use(router) // потом router (можно и наоборот, но Pinia должна быть ДО использования store)

// Route guard: require auth for most pages, allow login/register publicly
import { useAuthStore } from './stores/auth'
router.beforeEach((to, from, next) => {
  const auth = useAuthStore()
  const publicNames = ['Login', 'Register']
  if (publicNames.includes(to.name)) {
    // if already logged in, redirect to home
    if (auth.user || auth.accessToken) return next({ name: 'Home' })
    return next()
  }

  // all other routes require auth
  if (!auth.user && !auth.accessToken) {
    return next({ name: 'Login' })
  }
  return next()
})

app.mount('#app')
