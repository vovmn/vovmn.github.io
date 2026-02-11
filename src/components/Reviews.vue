<template>
  <section class="reviews-section">
    <h2>Отзывы клиентов</h2>

    <div class="reviews-wrapper" ref="wrapper">
      <ul
        class="reviews"
        :style="trackStyle"
        ref="track"
      >
        <li
          v-for="r in list"
          :key="r.uid"
          class="review"
        >
          <div class="stars">{{ r.stars }}</div>
          <div class="name">{{ r.name }}</div>
          <div class="text">{{ r.text }}</div>
          <div class="date">{{ r.date }}</div>
        </li>
      </ul>
    </div>

   <div class="nav">
  <button class="nav-btn" @click="scroll(-1)" aria-label="Назад">
    <svg viewBox="0 0 24 24">
      <path d="M15 18l-6-6 6-6" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
    </svg>
  </button>

  <button class="nav-btn" @click="scroll(1)" aria-label="Вперёд">
    <svg viewBox="0 0 24 24">
      <path d="M9 6l6 6-6 6" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
    </svg>
  </button>
</div>

  </section>
</template>


<script setup>
import { ref, computed, onMounted, nextTick } from 'vue'

const raw = [
  { stars: '★★★★★', name: 'Дарья Долбина', text: 'Отличное обслуживание, спасибо Всем!!! Приезжайте не пожалеете, раскрой всегда на высшем уровне! 🤗', date: '28.10.25' },
  { stars: '★★★★★', name: 'Анна Серябрякова', text: 'Заказывали в данной компании шкаф в прихожую. Всё очень качественно. Расчет и раскрой занимает 3-4 дн, подсказывают как сделать более практично и лучше. Обращались уже не первый раз и данная работа была не последней. Сотрудничаем по мебели во всех комнатах(стол, кухню и т.д', date: '24.10.25' },
  { stars: '★★★★★', name: 'Евгения Севастьянова', text: 'Заказывала здесь раскрой для шкафа и стола в детскую комнату. Собрали мебель сами, и получилось просто отлично, ребенок счастлив. Отдельно взяли столешницу, тоже качественная. Очень довольны!', date: '16.02.25' },
  { stars: '★★★★★', name: 'Елена Миронова', text: 'Заказывали распил ЛДСП и столешницы.Все отлично,выбор цветов большой.Ребята сделали все быстро.Присмотрели фасады,приедем еще.', date: '17.03.23' },
  { stars: '★★★★★', name: 'Дмитрий Волков', text: 'Заказали распил деталей, и столешницы все хорошо, цены доступные! Столешница 57 номер глянец, Лдсп Венге.Единственный минус ,что не зависимо от вашего чертежа, вы покупаете полный лист, что не очень бюджетно, но придётся куда нибудь его приспособить', date: '3.01.22' },
  { stars: '★★★★', name: 'Яр Гл', text: 'Было то , что мне надо)Далековато)', date: '24.11.19' }
]

// даём uid, чтобы Vue не путался
const list = ref(
  raw.map((r, i) => ({ ...r, uid: i + '-' + Math.random() }))
)

const wrapper = ref(null)
const track = ref(null)

const step = ref(0)
const offset = ref(0)
const animating = ref(false)

const trackStyle = computed(() => ({
  transform: `translateX(${offset.value}px)`,
  transition: animating.value ? 'transform 300ms ease' : 'none'
}))

function measure() {
  const card = track.value?.querySelector('.review')
  if (!card) return

  const rect = card.getBoundingClientRect()
  const gap = parseFloat(getComputedStyle(track.value).gap || 0)
  step.value = rect.width + gap
}

function scroll(dir) {
  if (animating.value || !step.value) return
  animating.value = true

  if (dir > 0) {
    // 👉 вправо
    offset.value = -step.value

    setTimeout(() => {
      const first = list.value.shift()
      list.value.push(first)

      offset.value = 0
      animating.value = false
    }, 300)

  } else {
    // 👈 влево — ТЕПЕРЬ С АНИМАЦИЕЙ
    offset.value = step.value

    setTimeout(() => {
      const last = list.value.pop()
      list.value.unshift(last)

      offset.value = 0
      animating.value = false
    }, 300)
  }
}
onMounted(async () => {
  await nextTick()
  measure()
  window.addEventListener('resize', measure)
})

</script>

<style scoped>
.reviews-section {
  padding: 3rem 2rem;
  background: #fafafa;
  text-align: center;
  overflow: hidden;
}

.reviews-wrapper {
  overflow: hidden;
  margin: 1.5rem 0;
}

.reviews {
  display: flex;
  gap: 1rem;
  width: max-content;
}

.review {
  flex: 0 0 260px;
  background: #fff;
  border-radius: 8px;
  padding: 1.2rem;
  box-shadow: 0 2px 8px rgba(0,0,0,.08);
}

.stars {
  color: #f9a602;
}

.name {
  font-weight: 600;
  margin: .4rem 0;
}

.text {
  font-size: .9rem;
  line-height: 1.3;
}

.date {
  font-size: .75rem;
  opacity: .7;
  text-align: right;
  margin-top: .5rem;
}

.nav {
  display: flex;
  justify-content: center;
  gap: 0.75rem;
  margin-top: 1rem;
}

.nav-btn {
  width: 52px;
  height: 52px;
  border-radius: 50%;
  border: 1px solid #d1d5db;
  background: white;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.25s ease;
  box-shadow: 0 4px 12px rgba(0,0,0,0.05);
}

.nav-btn svg {
  width: 20px;
  height: 20px;
  color: #374151;
}

.nav-btn:hover {

  transform: translateY(-2px);
}



.nav-btn:active {
  transform: scale(0.95);
}


@media (max-width: 768px) {
  .review {
    flex-basis: 230px;
  }
}

@media (max-width: 480px) {
  .review {
    flex-basis: 200px;
  }
}
@media (max-width: 768px) {
  .nav-btn {
    width: 46px;
    height: 46px;
  }

  .nav-btn svg {
    width: 18px;
    height: 18px;
  }
}

@media (max-width: 480px) {
  .nav-btn {
    width: 42px;
    height: 42px;
  }

  .nav-btn svg {
    width: 16px;
    height: 16px;
  }
}
</style>
