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
  { path: '/', redirect: '/home' },
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

app.mount('#app')
