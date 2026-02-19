<template>
  <div class="about-page">
    <Header />

    <main class="about-main">
      <div class="container">
        <!-- Hero -->
        <header class="about-hero">
          <h1 class="page-title">О компании «Аквилон»</h1>
          <p class="page-subtitle">
            Более 20 лет мы работаем с материалами для корпусной мебели в Саратове, обеспечиваем поставку и берём на себя весь цикл подготовки — наши клиенты получают точно выверенные, полностью готовые к сборке детали.
          </p>
        </header>

        <div class="about-content">
          <!-- Кто мы -->
          <section class="section card">
            <div class="section-head">
              <h2>Кто мы</h2>
              <p class="lead">
                «Аквилон» — поставщик древесно-плитных материалов и услуг по их обработке. Мы работаем с мебельными производствами и частными мастерами: оперативно предоставляем продукцию, подготавливаем детали к сборке.
              </p>
            </div>

            <div class="grid-2">
              <div class="info-card">
                <h3>Материалы</h3>
                <ul class="list">
                  <li>
                    ЛДСП, ЛМДФ и ДВПО от заводов: «РосПлит», Череповецкий ФМК, «Монза» (Верхневохтожский ДОК),
                    «ЕВРОПЛИТ» и SWISS KRONO.
                  </li>
                  <li>
                    Все материалы — с сертификатами качества, санитарно-эпидемиологическими заключениями и соответствуют требованиям ГОСТ.
                  </li>
                </ul>
              </div>

              <div class="info-card">
                <h3>Обработка</h3>
                <ul class="list list-check">
                  <li>Раскрой ЛДСП на современном оборудовании</li>
                  <li>Кромление постформингом и ПВХ</li>
                  <li>Фрезеровка под фурнитуру (в т.ч. под петли)</li>
                  <li>Изготовление гнутых радиусов и их кромление</li>
                </ul>
              </div>
            </div>
          </section>

          <!-- Доставка -->
          <section class="section callout">
            <div class="callout-inner">
              <h2>Доставка</h2>
              <p>
                Мы доставляем товары по городу и в ближайшие окрестности. Стоимость и условия доставки рассчитываются индивидуально.
              </p>
            </div>
          </section>

          <!-- История -->
          <section class="section card">
            <div class="section-head">
              <h2>Наша история</h2>
              <p class="muted">
                Основа нашего долголетия — гибкий подход и контроль качества.
              </p>
            </div>

            <div class="prose">
              <p>
                Компания «Аквилон» уже более 20 лет на рынке материалов для корпусной мебели в Саратове. С самого начала наш приоритет в работе — удобство для клиентов и надёжность.
              </p>

              <p>
                На этапе основания мы поставляли качественные материалы и постепенно расширяли спектр услуг, ориентируясь на реальные потребности рынка. Сегодня «Аквилон» — партнёр, который занимается всеми стадиями работы с материалом: от выбора сырья и раскроя до точной обработки и доставки.
              </p>

              <p>
                Нас выбирают и крупные мебельные производства, и частные мастера: мы одинаково внимательно относимся к срокам,
                точности и качеству. Эти принципы остаются неизменными — и именно на них строится наша репутация в Саратове.
              </p>
            </div>
          </section>

          <!-- Ценности (твои блоки оставлены) -->
          <section class="values-section">
            <h2>Наши ценности</h2>

            <div class="values-grid">
              <div class="value-card">
                <div class="value-icon">
                  <svg width="48" height="48" viewBox="0 0 24 24" fill="currentColor">
                    <path
                      d="M12 1L3 5v6c0 5.55 3.84 10.74 9 12 5.16-1.26 9-6.45 9-12V5l-9-4zm0 10.99h7c-.53 4.12-3.28 7.79-7 8.94V12H5V6.3l7-3.11v8.8z"
                    />
                  </svg>
                </div>
                <h3>Качество</h3>
                <p>Работаем с сертифицированными материалами и контролируем результат на каждом этапе</p>
              </div>

              <div class="value-card">
                <div class="value-icon">
                  <svg width="48" height="48" viewBox="0 0 24 24" fill="currentColor">
                    <path
                      d="M23 12l-2.44-2.78.34-3.68-3.61-.82-1.89-3.18L12 3 8.6 1.54 6.71 4.72l-3.61.81.34 3.68L1 12l2.44 2.78-.34 3.69 3.61.82 1.89 3.18L12 21l3.4 1.46 1.89-3.18 3.61-.82-.34-3.68L23 12zm-10 5h-2v-2h2v2zm0-4h-2V7h2v6z"
                    />
                  </svg>
                </div>
                <h3>Надёжность</h3>
                <p>Стабильные сроки, понятные условия и ответственность за выполнение заказа</p>
              </div>

              <div class="value-card">
                <div class="value-icon">
                  <svg width="48" height="48" viewBox="0 0 24 24" fill="currentColor">
                    <path
                      d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"
                    />
                  </svg>
                </div>
                <h3>Забота о клиентах</h3>
                <p>Подскажем, поможем выбрать и подстроимся под задачу — без бюрократии</p>
              </div>

              <div class="value-card">
                <div class="value-icon">
                  <svg width="48" height="48" viewBox="0 0 24 24" fill="currentColor">
                    <path d="M12 3v10.55c-.59-.34-1.27-.55-2-.55-2.21 0-4 1.79-4 4s1.79 4 4 4 4-1.79 4-4V7h4V3h-6z" />
                  </svg>
                </div>
                <h3>Развитие</h3>
                <p>Расширяем ассортимент и услуги, внедряем технологии и улучшаем сервис</p>
              </div>
            </div>
          </section>

          <!-- Статистика -->
          <section class="stats-section">
            <h2>Мы в цифрах</h2>
            <div class="stats-grid">
              <div class="stat-item">
                <div class="stat-number">20+</div>
                <div class="stat-label">лет на рынке</div>
              </div>
              <div class="stat-item">
                <div class="stat-number">5000+</div>
                <div class="stat-label">клиентов</div>
              </div>
              <div class="stat-item">
                <div class="stat-number">10000+</div>
                <div class="stat-label">заказов</div>
              </div>
              <div class="stat-item">
                <div class="stat-number">100%</div>
                <div class="stat-label">контроль качества</div>
              </div>
            </div>
          </section>
        </div>
      </div>
    </main>

    <Footer />
  </div>
</template>

<script setup>
import Header from '@/components/Header.vue'
import Footer from '@/components/Footer.vue'
</script>

<style scoped>


.about-page {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.about-page,
.about-page * {
  box-sizing: border-box;
}

.about-main {
  flex: 1;
  padding: 2.5rem 0;
  background: #f8f9fa;
  overflow-x: hidden;
}

.container {
  max-width: 1200px;
  width: 100%;
  margin: 0 auto;
  padding: 0 1rem;
  overflow-x: hidden;
}



.about-hero {
  text-align: center;
  margin-bottom: 2.5rem;
}

.page-title {
  font-size: 2.6rem;
  font-weight: 800;
  color: #111827;
  margin-bottom: 0.75rem;
  letter-spacing: -0.02em;
}

.page-subtitle {
  max-width: 820px;
  margin: 0 auto;
  font-size: 1.15rem;
  color: #4b5563;
  line-height: 1.7;
}



.about-content {
  display: flex;
  flex-direction: column;
  gap: 2rem;
  width: 100%;
  max-width: 100%;
  overflow-x: hidden;
}

.section {
  width: 100%;
}


.card {
  background: #fff;
  border: 1px solid #e5e7eb;
  border-radius: 14px;
  box-shadow: 0 8px 24px rgba(17, 24, 39, 0.05);
  padding: 2rem;
  width: 100%;
  max-width: 100%;
}

.section-head {
  max-width: 900px;
  margin: 0 auto 1.5rem;
  text-align: center;
}

.section-head h2 {
  font-size: 2rem;
  margin: 0 0 0.5rem;
  color: #111827;
}

.lead {
  font-size: 1.05rem;
  line-height: 1.75;
  color: #4b5563;
  margin: 0;
}

.muted {
  margin: 0;
  color: #6b7280;
}



.grid-2 {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.25rem;
  width: 100%;
  max-width: 100%;
}

.info-card {
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  padding: 1.25rem;
  background: #ffffff;
}

.info-card h3 {
  margin: 0 0 0.75rem;
  font-size: 1.2rem;
  color: #111827;
}



.list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.list li {
  position: relative;
  padding-left: 1.4rem;
  margin: 0.6rem 0;
  color: #4b5563;
  line-height: 1.6;
}

.list li::before {
  content: "";
  position: absolute;
  left: 0;
  top: 0.75rem;
  width: 8px;
  height: 8px;
  border-radius: 999px;
  background: #2b6cb0;
}

.list-check li::before {
  content: "✓";
  background: transparent;
  color: #2b6cb0;
  font-weight: 900;
  top: 0.1rem;
}


.callout {
  padding: 0;
  background: transparent;
}

.callout-inner {
  border-radius: 14px;
  padding: 1.5rem 2rem;
  background: linear-gradient(180deg, #ffffff, #f3f7ff);
  border: 1px solid #dbeafe;
  box-shadow: 0 10px 26px rgba(37, 99, 235, 0.08);
  max-width: 980px;
  margin: 0 auto;
  text-align: center;
}


.prose {
  max-width: 900px;
  margin: 0 auto;
  color: #4b5563;
  line-height: 1.85;
  font-size: 1.05rem;
}

.prose p {
  margin-bottom: 1rem;
}



.values-section h2 {
  text-align: center;
  font-size: 2rem;
  color: #111827;
  margin-bottom: 1.75rem;
}

.values-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1.25rem;
  width: 100%;
  max-width: 100%;
}

.value-card {
  background: white;
  padding: 1.75rem;
  border-radius: 12px;
  box-shadow: 0 10px 24px rgba(17, 24, 39, 0.06);
  text-align: center;
  border: 1px solid #e5e7eb;
}

.value-icon {
  color: #2b6cb0;
  margin-bottom: 1rem;
}

.stats-section {
  background: white;
  padding: 2.5rem;
  border-radius: 14px;
  box-shadow: 0 10px 24px rgba(17, 24, 39, 0.06);
  border: 1px solid #e5e7eb;
  width: 100%;
  max-width: 100%;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1.25rem;
}



svg {
  max-width: 100%;
  height: auto;
}



@media (max-width: 900px) {
  .grid-2 {
    grid-template-columns: 1fr;
  }

  .card {
    padding: 1.5rem;
  }
}

@media (max-width: 480px) {
  .page-title {
    font-size: 2rem;
  }

  .page-subtitle {
    font-size: 1.05rem;
  }

  .callout-inner {
    padding: 1.25rem;
  }

  .stats-section {
    padding: 1.5rem;
  }
  
}

.stats-section {
  width: 100%;
  max-width: 100%;
  margin: 0 auto;
  padding: 2.5rem;
  background: #ffffff;
  border-radius: 14px;
  border: 1px solid #e5e7eb;
  box-shadow: 0 10px 24px rgba(17, 24, 39, 0.06);
  overflow: visible;
}

.stats-section h2 {
  text-align: center;
  font-size: 2rem;
  font-weight: 700;
  color: #111827;
  margin-bottom: 1.75rem;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1.25rem;
}

.stat-item {
  text-align: center;
}

.stat-number {
  font-size: 2.6rem;
  font-weight: 800;
  color: #2b6cb0;
  line-height: 1;
}

.stat-label {
  margin-top: 0.25rem;
  font-size: 0.95rem;
  color: #6b7280;
}

</style>
