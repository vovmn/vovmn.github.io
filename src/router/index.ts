import { createRouter, createWebHashHistory } from 'vue-router'

const routes = [
  {
    path: '/',
    name: 'Home',
    component: () => import('@/pages/main.vue'),
    meta: { title: 'Аквилон — Главная' }
  },
  {
    path: '/catalog',
    name: 'Catalog',
    component: () => import('@/pages/Catalog.vue'),
    meta: { title: 'Каталог продукции — Аквилон' }
  },
  {
    path: '/about',
    name: 'About',
    component: () => import('@/pages/about.vue'),
    meta: { title: 'О компании — Аквилон' }
  },
  {
    path: '/services',
    name: 'Services',
    component: () => import('@/pages/services.vue'),
    meta: { title: 'Услуги — Аквилон' }
  },
  {
    path: '/contact',
    name: 'Contact',
    component: () => import('@/pages/contact.vue'),
    meta: { title: 'Контакты — Аквилон' }
  }
]

const router = createRouter({
  history: createWebHashHistory(),
  routes,
  scrollBehavior() {
    return { top: 0 }
  }
})

router.afterEach((to) => {
  document.title = typeof to.meta.title === 'string'
    ? to.meta.title
    : 'Аквилон'
})

export default router
