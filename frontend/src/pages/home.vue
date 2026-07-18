<template>
  <div class="page">
    <Header />

    <main class="contacts">
      <section class="hero">
        <div>
          <p class="eyebrow">Связь с клиникой</p>
          <h1>Контакты и адрес</h1>
          <p>
            Шаблоны для телефонов, почты и карты уже подготовлены. Останется заменить
            значения на реальные контакты клиники.
          </p>
        </div>

        <div class="quick-card">
          <span>Основной номер</span>
          <a href="tel:+79990000000">+7 (999) 000-00-00</a>
          <p>Ежедневно с 09:00 до 21:00</p>
        </div>
      </section>

      <section class="content-grid">
        <div class="contacts-panel">
          <div class="section-heading">
            <h2>Телефоны</h2>
            <p>Замените номера на реальные рабочие линии.</p>
          </div>

          <div class="phone-list">
            <article
              v-for="phone in phones"
              :key="phone.label"
              class="phone-card"
            >
              <div>
                <h3>{{ phone.label }}</h3>
                <p>{{ phone.description }}</p>
              </div>
              <a :href="`tel:${phone.raw}`">{{ phone.display }}</a>
            </article>
          </div>
        </div>

        <aside class="info-panel">
          <h2>Режим работы</h2>
          <dl>
            <div>
              <dt>Пн-Пт</dt>
              <dd>09:00-21:00</dd>
            </div>
            <div>
              <dt>Сб</dt>
              <dd>10:00-18:00</dd>
            </div>
            <div>
              <dt>Вс</dt>
              <dd>По записи</dd>
            </div>
          </dl>

          <h2>Адрес</h2>
          <p class="address">г. Москва, ул. Примерная, 10, офис 25</p>

          <h2>Почта</h2>
          <a class="mail-link" href="mailto:info@immunolab.ru">info@immunolab.ru</a>
        </aside>
      </section>

      <section class="map-section">
        <div class="section-heading">
          <h2>Карта</h2>
          <p>Шаблон под Яндекс.Карты: сюда можно вставить iframe из конструктора карт.</p>
        </div>

        <div class="map-placeholder">
          <div class="map-copy">
            <span>Яндекс.Карты</span>
            <strong>Место для карты клиники</strong>
            <p>
              Вставьте embed-ссылку в блок ниже, когда будет точный адрес. Сейчас
              оставлена аккуратная заглушка, чтобы страница выглядела завершенной.
            </p>
          </div>

          <!--
            Когда будет готова ссылка из Яндекс.Карт, замените заглушку на iframe:
            <iframe
              src="https://yandex.ru/map-widget/v1/?..."
              width="100%"
              height="100%"
              frameborder="0"
              allowfullscreen
            ></iframe>
          -->
        </div>
      </section>
    </main>
  </div>
</template>

<script setup>
import Header from '../components/Header.vue'

const phones = [
  {
    label: 'Регистратура',
    description: 'Запись на прием и перенос визита',
    display: '+7 (999) 000-00-00',
    raw: '+79990000000',
  },
  {
    label: 'Поддержка пациентов',
    description: 'Вопросы по личному кабинету и анкетам',
    display: '+7 (999) 111-22-33',
    raw: '+79991112233',
  },
  {
    label: 'Лаборатория',
    description: 'Уточнение по анализам и документам',
    display: '+7 (999) 444-55-66',
    raw: '+79994445566',
  },
]
</script>

<style scoped>
.page {
  min-height: 100vh;
}

.contacts {
  width: min(1180px, calc(100% - 2rem));
  margin: 0 auto;
  padding: 2rem 0 3rem;
}

.hero,
.contacts-panel,
.info-panel,
.map-section {
  border: 1px solid #dce8ea;
  border-radius: 8px;
  background: #fff;
  box-shadow: 0 16px 40px rgba(25, 45, 65, 0.07);
}

.hero {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 300px;
  gap: 1.5rem;
  align-items: end;
  padding: 2rem;
}

.eyebrow {
  margin: 0 0 0.7rem;
  color: #1f847f;
  font-size: 0.82rem;
  font-weight: 800;
  text-transform: uppercase;
}

h1,
h2,
h3,
p {
  margin-top: 0;
}

h1 {
  max-width: 720px;
  margin-bottom: 0.8rem;
  color: #152236;
  font-size: clamp(2rem, 4vw, 3.1rem);
  line-height: 1.08;
}

.hero p,
.section-heading p,
.phone-card p,
.address,
.map-copy p {
  color: #637284;
  line-height: 1.6;
}

.hero p {
  max-width: 650px;
  margin-bottom: 0;
}

.quick-card {
  padding: 1.2rem;
  border: 1px solid #dce8ea;
  border-radius: 8px;
  background: #f8fbfb;
}

.quick-card span {
  display: block;
  margin-bottom: 0.4rem;
  color: #6b7c8d;
  font-size: 0.86rem;
  font-weight: 700;
}

.quick-card a {
  display: block;
  color: #12635f;
  font-size: 1.3rem;
  font-weight: 900;
}

.quick-card p {
  margin: 0.45rem 0 0;
  font-size: 0.9rem;
}

.content-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 340px;
  gap: 1rem;
  margin-top: 1rem;
}

.contacts-panel,
.info-panel,
.map-section {
  padding: 1.4rem;
}

.section-heading {
  margin-bottom: 1rem;
}

.section-heading h2,
.info-panel h2 {
  margin-bottom: 0.35rem;
  color: #1d2b40;
  font-size: 1.16rem;
}

.section-heading p {
  margin-bottom: 0;
}

.phone-list {
  display: grid;
  gap: 0.75rem;
}

.phone-card {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  gap: 1rem;
  align-items: center;
  padding: 1rem;
  border: 1px solid #e1ecee;
  border-radius: 8px;
  background: #fbfdfd;
}

.phone-card h3 {
  margin-bottom: 0.25rem;
  color: #213248;
  font-size: 1rem;
}

.phone-card p {
  margin-bottom: 0;
  font-size: 0.92rem;
}

.phone-card a,
.mail-link {
  color: #12635f;
  font-weight: 900;
}

.info-panel dl {
  display: grid;
  gap: 0.5rem;
  margin: 0 0 1.2rem;
}

.info-panel dl div {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  padding-bottom: 0.5rem;
  border-bottom: 1px solid #edf2f4;
}

.info-panel dt {
  color: #657687;
}

.info-panel dd {
  margin: 0;
  color: #203044;
  font-weight: 800;
}

.address {
  margin-bottom: 1.2rem;
}

.map-section {
  margin-top: 1rem;
}

.map-placeholder {
  display: grid;
  min-height: 360px;
  place-items: center;
  overflow: hidden;
  border: 1px dashed #b9ccd0;
  border-radius: 8px;
  background:
    linear-gradient(90deg, rgba(31, 132, 127, 0.08) 1px, transparent 1px),
    linear-gradient(rgba(31, 132, 127, 0.08) 1px, transparent 1px),
    #f8fbfb;
  background-size: 34px 34px;
}

.map-placeholder iframe {
  width: 100%;
  height: 100%;
  min-height: 360px;
  border: 0;
}

.map-copy {
  max-width: 420px;
  padding: 1.5rem;
  border: 1px solid #dce8ea;
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.92);
  text-align: center;
}

.map-copy span {
  display: inline-flex;
  margin-bottom: 0.5rem;
  color: #1f847f;
  font-size: 0.82rem;
  font-weight: 900;
  text-transform: uppercase;
}

.map-copy strong {
  display: block;
  margin-bottom: 0.5rem;
  color: #172338;
  font-size: 1.2rem;
}

.map-copy p {
  margin-bottom: 0;
}

@media (max-width: 860px) {
  .hero,
  .content-grid {
    grid-template-columns: 1fr;
  }

  .phone-card {
    grid-template-columns: 1fr;
  }
}
</style>
