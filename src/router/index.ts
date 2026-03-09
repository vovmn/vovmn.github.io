import { createRouter, createWebHistory, type RouteLocationNormalizedLoaded } from 'vue-router'

const DEFAULT_TITLE = 'Аквилон'
const DEFAULT_DESCRIPTION = 'ЛДСП, столешницы и мебельная фурнитура в Саратове.'
const DEFAULT_ROBOTS = 'index,follow'

// Лучше задать в .env.production:
// VITE_SITE_URL=https://your-domain.ru
const SITE_URL = (import.meta.env.VITE_SITE_URL || window.location.origin).replace(/\/$/, '')

// Храним позиции только для тех страниц, где это реально нужно.
// Сейчас включено для каталога.
const scrollPositions = new Map<string, number>()

const routes = [
  {
    path: '/',
    name: 'Home',
    component: () => import('@/pages/main.vue'),
    meta: {
      title: 'Аквилон | ЛДСП, столешницы, мебельная фурнитура купить Саратов',
      description: 'Аквилон – ваш постоянный партнёр в решении любых задач. Продажа лдсп, столешниц, мебельной фурнитуры в Саратове.',
      keywords: 'продажа лдсп саратов, продажа столешниц саратов, купить столешницу в саратове, мебельная фурнитура купить саратов, продажа мебельной фурнитуры в саратове, купить лдсп в саратове',
      robots: 'index,follow'
    }
  },
  {
    path: '/catalog',
    name: 'Catalog',
    component: () => import('@/pages/Catalog.vue'),
    meta: {
      title: 'Каталог | ЛДВП, ЛДСП, столешницы скиф, мебельная фурнитура Саратов',
      description: 'Широкий выбор ЛДВП, ЛДСП, столешниц для кухни, а так же мебельная фурнитура. Выгодная цена. Саратов.',
      keywords: 'лдвп саратов, лдсп саратов, столешницы скиф саратов, столешницы для кухни саратов, мебельная фурнитура саратов, фурнитура для мебели саратов, мебельная фурнитура цена саратов',
      robots: 'index,follow',
      preserveScroll: true
    }
  },
  {
    path: '/services',
    name: 'Services',
    component: () => import('@/pages/services.vue'),
    meta: {
      title: 'Услуги | раскрой лдсп, кромление пвх, ламинирование дсп Саратов',
      description: 'Компания Аквилон в Саратове осуществляет раскрой лдсп, кромление пвх, ламинирование дсп и др.',
      keywords: 'раскрой лдсп в саратове, раскрой лдсп саратов, кромление пвх, ламинирование дсп саратов',
      robots: 'index,follow'
    }
  },
  {
    path: '/contact',
    name: 'Contact',
    component: () => import('@/pages/contact.vue'),
    meta: {
      title: 'Контакты | Аквилон в Саратове',
      description: 'Аквилон в Саратове. Свяжитесь с нами удобным для вас способом. Все контакты указаны на нашем сайте.',
      keywords: 'Аквилон саратов телефон, аквилон в саратове контакты',
      robots: 'index,follow'
    }
  },
  {
    path: '/about',
    name: 'About',
    component: () => import('@/pages/about.vue'),
    meta: {
      title: 'О Нас | Аквилон в Саратове',
      description: 'Компания Аквилон - более 20 лет на рынке материалов для корпусной мебели в Саратове!',
      keywords: 'ип солопова, аквилон саратов, аквилон в саратове',
      robots: 'index,follow'
    }
  },
  {
    path: '/:pathMatch(.*)*',
    redirect: '/'
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior(to, _from, savedPosition) {
    return new Promise((resolve) => {
      // Даём странице дорисоваться, иначе восстановление скролла
      // иногда срабатывает криво на длинных каталогах
      setTimeout(() => {
        // Назад / вперёд в браузере
        if (savedPosition) {
          resolve(savedPosition)
          return
        }

        // Ручное восстановление для страниц, где preserveScroll=true
        if (to.meta.preserveScroll) {
          const savedTop = scrollPositions.get(to.fullPath)

          if (typeof savedTop === 'number') {
            resolve({ left: 0, top: savedTop })
            return
          }
        }

        // По умолчанию — наверх
        resolve({ left: 0, top: 0 })
      }, 50)
    })
  }
})

router.beforeEach((to, from) => {
  if (from.meta.preserveScroll) {
    scrollPositions.set(from.fullPath, window.scrollY)
  }

  // Если идём на совсем другую страницу, а не возвращаемся на ту же,
  // старую позицию можно убрать, чтобы не было странных сюрпризов
  if (to.fullPath !== from.fullPath && !to.meta.preserveScroll) {
    scrollPositions.delete(to.fullPath)
  }
})

router.afterEach((to) => {
  applyHead(to)
})

function applyHead(to: RouteLocationNormalizedLoaded) {
  const title = String(to.meta.title || DEFAULT_TITLE)
  const description = String(to.meta.description || DEFAULT_DESCRIPTION)
  const keywords = to.meta.keywords ? String(to.meta.keywords) : ''
  const robots = String(to.meta.robots || DEFAULT_ROBOTS)
  const canonical = buildCanonicalUrl(to)

  document.title = title

  setMetaByName('description', description)
  setMetaByName('robots', robots)

  if (keywords) {
    setMetaByName('keywords', keywords)
  } else {
    removeMetaByName('keywords')
  }

  setLinkTag('canonical', canonical)

  // Open Graph
  setMetaByProperty('og:title', title)
  setMetaByProperty('og:description', description)
  setMetaByProperty('og:type', 'website')
  setMetaByProperty('og:url', canonical)
  setMetaByProperty('og:site_name', 'Аквилон')

  // Twitter
  setMetaByName('twitter:card', 'summary_large_image')
  setMetaByName('twitter:title', title)
  setMetaByName('twitter:description', description)
}

function buildCanonicalUrl(to: RouteLocationNormalizedLoaded) {
  // По умолчанию canonical без query и hash, чтобы не плодить дубли.
  // Если однажды понадобятся индексируемые query-страницы,
  // лучше задать это явно.
  const canonicalPath = String(to.meta.canonicalPath || to.path)
  return new URL(canonicalPath, SITE_URL).toString()
}

function setMetaByName(name: string, content: string) {
  let tag = document.querySelector(`meta[name="${name}"]`) as HTMLMetaElement | null

  if (!tag) {
    tag = document.createElement('meta')
    tag.setAttribute('name', name)
    document.head.appendChild(tag)
  }

  tag.setAttribute('content', content)
}

function setMetaByProperty(property: string, content: string) {
  let tag = document.querySelector(`meta[property="${property}"]`) as HTMLMetaElement | null

  if (!tag) {
    tag = document.createElement('meta')
    tag.setAttribute('property', property)
    document.head.appendChild(tag)
  }

  tag.setAttribute('content', content)
}

function removeMetaByName(name: string) {
  const tag = document.querySelector(`meta[name="${name}"]`)
  if (tag) {
    tag.remove()
  }
}

function setLinkTag(rel: string, href: string) {
  let tag = document.querySelector(`link[rel="${rel}"]`) as HTMLLinkElement | null

  if (!tag) {
    tag = document.createElement('link')
    tag.setAttribute('rel', rel)
    document.head.appendChild(tag)
  }

  tag.setAttribute('href', href)
}

export default router