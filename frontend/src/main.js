import { createApp } from 'vue'
import { createRouter, createWebHashHistory } from 'vue-router'
import { createPinia } from 'pinia'
import App from './App.vue'

import Documents from './pages/documents.vue'
import Contacts from './pages/home.vue'
import Info from './pages/Info.vue'
import Login from './pages/login.vue'
import QuestionnaireView from './pages/QuestionnaireView.vue'
import Record from './pages/record.vue'
import Register from './pages/register.vue'
import { useAuthStore } from './stores/auth'

const routes = [
  { path: '/', redirect: '/info' },
  { path: '/home', redirect: '/contacts' },
  { path: '/contacts', component: Contacts, name: 'Contacts', meta: { requiresAuth: true } },
  { path: '/login', component: Login, name: 'Login', meta: { publicOnly: true } },
  { path: '/register', component: Register, name: 'Register', meta: { publicOnly: true } },
  { path: '/documents', component: Documents, name: 'Documents', meta: { requiresAuth: true } },
  { path: '/record', component: Record, name: 'Record', meta: { requiresAuth: true } },
  { path: '/info', component: Info, name: 'Info', meta: { requiresAuth: true } },
  {
    path: '/questionnaire/:systemCode',
    component: QuestionnaireView,
    name: 'Questionnaire',
    meta: { requiresAuth: true },
  },
  { path: '/:pathMatch(.*)*', redirect: '/info' },
]

const router = createRouter({
  history: createWebHashHistory(),
  routes,
})

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)

router.beforeEach(async (to) => {
  const auth = useAuthStore()

  if (to.meta.publicOnly) {
    if (!auth.accessToken) return true

    if (!auth.user) {
      const user = await auth.fetchMe()
      if (!user) return true
    }

    return { name: 'Info' }
  }

  if (to.meta.requiresAuth) {
    if (!auth.accessToken) {
      return { name: 'Login', query: { redirect: to.fullPath } }
    }

    if (!auth.user) {
      const user = await auth.fetchMe()
      if (!user) {
        return { name: 'Login', query: { redirect: to.fullPath } }
      }
    }
  }

  return true
})

app.mount('#app')
