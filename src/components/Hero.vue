<template>
  <section class="hero">
    <h1>
      Все для мебели:<br />
      материалы, услуги, изделия
    </h1>

    <p class="subtitle">
      ЛДСП, столешницы, фасады из массива и фурнитура для мебельного производства в Саратове. <br />
      Ваш постоянный партнёр в решении любых задач
    </p>

    <div class="buttons">
      <RouterLink class="btn primary" to="/catalog">Посмотреть каталог</RouterLink>
      <RouterLink class="btn secondary" to="/contact">Связаться с нами</RouterLink>
    </div>
  </section>
</template>

<style scoped>


.hero {
  background: url(@/img/main.webp) center / cover no-repeat;
  min-height: 60vh;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  position: relative;
  color: #e6e6e3;
  text-align: center;
  padding: 2rem;

  -webkit-mask-image: linear-gradient(
    to bottom,
    rgba(0, 0, 0, 1) 0%,
    rgba(0, 0, 0, 1) 88%,
    rgba(0, 0, 0, 0.85) 100%
  );
  mask-image: linear-gradient(
    to bottom,
    rgba(0, 0, 0, 1) 0%,
    rgba(0, 0, 0, 1) 88%,
    rgba(0, 0, 0, 0.85) 100%
  );
}



/* затемнение ТОЛЬКО для читабельности текста */
.hero::before {
  content: '';
  position: absolute;
  inset: 0;
  background: rgba(0, 0, 0, 0.35);
  z-index: 0;
}
.hero::after {
  content: '';
  position: absolute;
  left: 0;
  right: 0;
  bottom: -1px;
  height: 260px; /* ключевой момент */
  background: linear-gradient(
    to bottom,
    rgba(248, 249, 250, 0) 0%,
    rgba(248, 249, 250, 0.15) 25%,
    rgba(248, 249, 250, 0.35) 50%,
    rgba(248, 249, 250, 0.6) 75%,
    #f8f9fa 100%
  );
  z-index: 2;
  pointer-events: none;
}



/* контент поверх */
.hero > * {
  position: relative;
  z-index: 1;
}

.subtitle {
  font-size: 130%;
  font-weight: bold;
  max-width: 900px;
    color: rgba(255, 255, 255, 0.75);
}

.buttons {
  margin: 2rem 0;
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
  justify-content: center;
}

.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: .75rem 1.5rem;
  border-radius: 6px;
  cursor: pointer;
  font-size: 1rem;
  text-decoration: none;
  transition: 0.2s;
}

.primary {
  background: #fff;
  color: #222;
}
.primary:hover {
  background: #f2f2f2;
  transform: translateY(-1px);
}

.secondary {
  background: transparent;
  border: 1px solid #fff;
  color: #fff;
}
.secondary:hover {
  background: #fff;
  color: #222;
  transform: translateY(-1px);
}



</style>
