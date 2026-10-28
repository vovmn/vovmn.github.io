<template>
  <div class="page">
    <Header />

    <main class="about">
      <section class="hero">
        <div>
          <p class="eyebrow">О лаборатории</p>
          <h1>Иммунолаб помогает врачу видеть картину пациента целиком</h1>
          <p>
            Мы собираем анкетные данные, историю обращений и записи на прием в одном
            кабинете, чтобы подготовка к консультации была понятной и спокойной.
          </p>
        </div>

        <div class="accent-panel">
          <span>Фокус</span>
          <strong>Иммунология, диагностика и сопровождение пациента</strong>
          <p>Анкеты помогают заранее отметить симптомы, жалобы и важные детали.</p>
        </div>
      </section>

      <section class="content-grid">
        <article class="panel">
          <div class="section-heading">
            <h2>Чем занимается Иммунолаб</h2>
            <p>
              Личный кабинет связывает пациента и администратора: администратор назначает
              опросники и время приема, пациент заполняет ответы до визита.
            </p>
          </div>

          <div class="principles">
            <div v-for="item in principles" :key="item.title" class="principle">
              <span>{{ item.number }}</span>
              <div>
                <h3>{{ item.title }}</h3>
                <p>{{ item.text }}</p>
              </div>
            </div>
          </div>
        </article>

        <aside class="panel side-panel">
          <h2>Что уже есть в кабинете</h2>
          <ul>
            <li>Назначение опросников по системам организма</li>
            <li>Сохранение ответов пациента в базе</li>
            <li>Записи на прием и история назначений</li>
            <li>Админка для управления пациентами</li>
          </ul>
        </aside>
      </section>

      <section class="metrics">
        <div v-for="metric in metrics" :key="metric.label" class="metric">
          <strong>{{ metric.value }}</strong>
          <span>{{ metric.label }}</span>
        </div>
      </section>
    </main>
  </div>
</template>

<script setup>
import Header from '@/components/Header.vue'

const principles = [
  {
    number: '01',
    title: 'Понятный сбор данных',
    text: 'Пациент отвечает на вопросы заранее, а врач получает структурированную информацию.',
  },
  {
    number: '02',
    title: 'Контроль назначений',
    text: 'Администратор видит пользователей, опросники и записи на прием в одном интерфейсе.',
  },
  {
    number: '03',
    title: 'Аккуратная подготовка к визиту',
    text: 'Ответы не теряются и остаются привязанными к нужной системе организма.',
  },
]

const metrics = [
  { value: '24/7', label: 'доступ к личному кабинету' },
  { value: '12', label: 'систем организма в опросниках' },
  { value: '1', label: 'единая база данных пациента' },
]
</script>

<style scoped>
.page {
  min-height: 100vh;
}

.about {
  width: min(1180px, calc(100% - 2rem));
  margin: 0 auto;
  padding: 2rem 0 3rem;
}

.hero,
.panel,
.metrics {
  border: 1px solid #dce8ea;
  border-radius: 8px;
  background: #fff;
  box-shadow: 0 16px 40px rgba(25, 45, 65, 0.07);
}

.hero {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 340px;
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
  max-width: 780px;
  margin-bottom: 0.8rem;
  color: #152236;
  font-size: clamp(2rem, 4vw, 3.1rem);
  line-height: 1.08;
}

.hero p,
.section-heading p,
.principle p,
.accent-panel p,
.side-panel li {
  color: #637284;
  line-height: 1.6;
}

.hero p {
  max-width: 680px;
  margin-bottom: 0;
}

.accent-panel {
  padding: 1.2rem;
  border: 1px solid #dce8ea;
  border-radius: 8px;
  background: #f8fbfb;
}

.accent-panel span {
  display: block;
  margin-bottom: 0.45rem;
  color: #6b7c8d;
  font-size: 0.86rem;
  font-weight: 800;
}

.accent-panel strong {
  display: block;
  color: #12635f;
  font-size: 1.15rem;
  line-height: 1.35;
}

.accent-panel p {
  margin: 0.7rem 0 0;
}

.content-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 360px;
  gap: 1rem;
  margin-top: 1rem;
}

.panel {
  padding: 1.4rem;
}

.section-heading {
  margin-bottom: 1rem;
}

.section-heading h2,
.side-panel h2 {
  margin-bottom: 0.35rem;
  color: #1d2b40;
  font-size: 1.16rem;
}

.section-heading p {
  margin-bottom: 0;
}

.principles {
  display: grid;
  gap: 0.75rem;
}

.principle {
  display: grid;
  grid-template-columns: 48px minmax(0, 1fr);
  gap: 1rem;
  padding: 1rem;
  border: 1px solid #e1ecee;
  border-radius: 8px;
  background: #fbfdfd;
}

.principle span {
  display: grid;
  width: 42px;
  height: 42px;
  place-items: center;
  border-radius: 8px;
  background: #edf7f6;
  color: #12635f;
  font-weight: 900;
}

.principle h3 {
  margin-bottom: 0.25rem;
  color: #213248;
  font-size: 1rem;
}

.principle p {
  margin-bottom: 0;
}

.side-panel ul {
  display: grid;
  gap: 0.8rem;
  margin: 1rem 0 0;
  padding-left: 1.1rem;
}

.side-panel li::marker {
  color: #1f847f;
}

.metrics {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 1rem;
  margin-top: 1rem;
  padding: 1.4rem;
}

.metric {
  padding: 1rem;
  border: 1px solid #e1ecee;
  border-radius: 8px;
  background: #fbfdfd;
}

.metric strong {
  display: block;
  margin-bottom: 0.35rem;
  color: #12635f;
  font-size: 1.7rem;
}

.metric span {
  color: #637284;
  font-weight: 700;
}

@media (max-width: 860px) {
  .hero,
  .content-grid,
  .metrics {
    grid-template-columns: 1fr;
  }
}
</style>
