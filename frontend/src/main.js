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
import QuestionnaireView from './pages/QuestionnaireView.vue'   // <-- добавлен импорт

const routes = [
  { path: '/', redirect: '/login' },
  { path: '/home', component: Home, name: 'Home' },
  { path: '/login', component: Login, name: 'Login' },
  { path: '/register', component: Register, name: 'Register' },
  { path: '/documents', component: Documents, name: 'Documents' },
  { path: '/record', component: Record, name: 'Record' },
  { path: '/info', component: Info, name: 'Info' },
  // новый маршрут для опросника
  {
    path: '/questionnaire/:systemCode',
    component: QuestionnaireView,
    name: 'Questionnaire',
  },
]

const router = createRouter({
  history: createWebHashHistory(),
  routes,
})

const app = createApp(App)

const pinia = createPinia()
app.use(pinia)
app.use(router)

// Route guard
import { useAuthStore } from './stores/auth'
router.beforeEach((to, from, next) => {
  const auth = useAuthStore()
  const publicNames = ['Login', 'Register']
  if (publicNames.includes(to.name)) {
    if (auth.user || auth.accessToken) return next({ name: 'Home' })
    return next()
  }

  if (!auth.user && !auth.accessToken) {
    return next({ name: 'Login' })
  }
  return next()
})

app.mount('#app')