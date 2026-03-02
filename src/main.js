import { createApp } from 'vue'
import { createRouter, createWebHashHistory } from 'vue-router'
import App from './App.vue'

// Импорт компонентов
import Info from './pages/Info.vue'
import Home from './pages/Home.vue'
import Autorization from './pages/Autorization.vue'
import Register from './pages/Register.vue'
import Documents from './pages/Documents.vue'
import Record from './pages/Record.vue'

const routes = [
    { path: '/', redirect: '/home' },
    { path: '/documents', component: Documents, name: 'Documents' },
    { path: '/record', component: Record, name: 'Record' },
    { path: '/home', component: Home, name: 'Home' },
    { path: '/info', component: Info, name: 'Info' },
    { path: '/autorization', component: Autorization, name: 'Autorization' },
    { path: '/register', component: Register, name: 'Register' }
]

const router = createRouter({
    history: createWebHashHistory(),
    routes
})

const app = createApp(App)
app.use(router)
app.mount('#app')