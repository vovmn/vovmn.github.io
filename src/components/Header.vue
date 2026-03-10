<template>
  <header class="header">
    <div class="header-inner">
      <div class="logo">
        <RouterLink
          to="/"
          class="logo-link"
          aria-label="Аквилон — на главную"
        >
          <img
            src="@/img/logo.webp"
            alt="Аквилон"
            class="logo-image"
          />
          <span class="logo-text">АКВИЛОН</span>
        </RouterLink>
      </div>

      <nav class="nav" aria-label="Основная навигация">
        <RouterLink to="/">Главная</RouterLink>
        <RouterLink to="/catalog">Каталог</RouterLink>
        <RouterLink to="/services">Услуги</RouterLink>
        <RouterLink to="/contact">Контакты</RouterLink>
        <RouterLink to="/about">О нас</RouterLink>
      </nav>

      <div class="header-right">
        <a
          href="https://go.2gis.com/v3vLE"
          target="_blank"
          rel="noopener noreferrer"
          class="gis-link"
          aria-label="Открыть Аквилон в 2GIS"
        >
          <svg
            class="gis-icon"
            width="20"
            height="20"
            viewBox="0 0 24 24"
            fill="currentColor"
            aria-hidden="true"
          >
            <path
              d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5c-1.38 0-2.5-1.12-2.5-2.5s1.12-2.5 2.5-2.5 2.5 1.12 2.5 2.5-1.12 2.5-2.5 2.5z"
            />
          </svg>
          <span>2GIS</span>
        </a>
      </div>
    </div>
  </header>
</template>

<script setup>
import { RouterLink } from 'vue-router'
</script>

<style scoped>
.header {
  background: #ffffff;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
  font-family: "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
  letter-spacing: 0.02em;
}

.header-inner {
  max-width: 1240px;
  margin: 0 auto;
  padding: 1rem 1.5rem;

  display: grid;
  grid-template-columns: auto 1fr auto;
  align-items: center;
  column-gap: 1.25rem;
}

/* LOGO */

.logo {
  min-width: 0;
}

.logo-link {
  display: inline-flex;
  align-items: center;
  gap: 0.7rem;
  text-decoration: none;
  color: #1f2937;
  transition: opacity 0.2s ease, transform 0.2s ease;
}

.logo-link:hover {
  opacity: 0.92;
  transform: translateY(-1px);
}

.logo-image {
  height: 56px;
  width: auto;
  display: block;
  object-fit: contain;
  flex-shrink: 0;
}

.logo-text {
  position: relative;
  display: inline-block;
  padding-bottom: 0.38rem;

  font-family: "Georgia", "Times New Roman", serif;
  font-size: 1.32rem;
  font-weight: 500;
  line-height: 1;
  letter-spacing: 0.28em;
  white-space: nowrap;
}

.logo-text::after {
  content: "";
  position: absolute;
  left: 0;
  bottom: 0;
  width: 100%;
  height: 2px;
  background-color: #1f2937;
  opacity: 0.85;
}

/* NAV */

.nav {
  min-width: 0;
  display: flex;
  justify-content: center;
  align-items: center;
  gap: clamp(0.8rem, 1.8vw, 1.65rem);
}

.nav a {
  position: relative;
  padding-bottom: 0.25rem;

  color: #444;
  text-decoration: none;
  font-size: 0.95rem;
  line-height: 1;
  white-space: nowrap;
  transition: color 0.25s ease;
}

.nav a::after {
  content: "";
  position: absolute;
  left: 0;
  bottom: -0.2rem;
  width: 0;
  height: 2px;
  background: #c00;
  transition: width 0.25s ease;
}

.nav a:hover {
  color: #c00;
}

.nav a:hover::after {
  width: 100%;
}

.nav a.router-link-active,
.nav a.router-link-exact-active {
  color: #c00;
}

.nav a.router-link-active::after,
.nav a.router-link-exact-active::after {
  width: 100%;
}

/* RIGHT */

.header-right {
  display: flex;
  justify-content: flex-end;
}

.gis-link {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;

  padding: 0.55rem 0.95rem;
  border-radius: 8px;
  background: #2b6cb0;
  color: #fff;
  text-decoration: none;

  font-size: 0.9rem;
  font-weight: 500;
  line-height: 1;

  box-shadow: 0 2px 4px rgba(43, 108, 176, 0.28);
  transition: background 0.25s ease, transform 0.25s ease, box-shadow 0.25s ease;
}

.gis-link:hover {
  background: #2c5282;
  transform: translateY(-1px);
  box-shadow: 0 4px 10px rgba(43, 108, 176, 0.35);
}

.gis-icon {
  flex-shrink: 0;
}

/* <= 1024 */

@media (max-width: 1024px) {
  .header-inner {
    padding: 0.95rem 1.25rem;
    column-gap: 1rem;
  }

  .logo-image {
    height: 50px;
  }

  .logo-text {
    font-size: 1.08rem;
    letter-spacing: 0.2em;
  }

  .nav a {
    font-size: 0.9rem;
  }

  .gis-link {
    padding: 0.5rem 0.85rem;
    font-size: 0.85rem;
  }
}

/* <= 900 */

@media (max-width: 900px) {
  .header-inner {
    grid-template-columns: 1fr auto;
    grid-template-areas:
      "logo right"
      "nav nav";
    row-gap: 0.95rem;
  }

  .logo {
    grid-area: logo;
  }

  .header-right {
    grid-area: right;
  }

  .nav {
    grid-area: nav;
    justify-content: center;
    gap: 1.1rem;
    padding-top: 0.1rem;
  }
}

/* <= 600 */

@media (max-width: 600px) {
  .header-inner {
    padding: 0.85rem 1rem;
    row-gap: 0.8rem;
  }

  .logo-link {
    gap: 0.55rem;
  }

  .logo-image {
    height: 40px;
  }

  .logo-text {
    font-size: 0.98rem;
    letter-spacing: 0.16em;
    padding-bottom: 0.3rem;
  }

  .nav {
    gap: 0.9rem;
  }

  .nav a {
    font-size: 0.84rem;
  }

  .gis-link {
    padding: 0.5rem;
    border-radius: 8px;
  }

  .gis-link span {
    display: none;
  }
}

/* iPhone 14 / 390px */

@media (max-width: 420px) {
  .header-inner {
    padding: 0.75rem 0.85rem;
  }

  .logo-image {
    height: 34px;
  }

  .logo-text {
    font-size: 0.88rem;
    letter-spacing: 0.12em;
  }

  .nav {
    gap: 0.72rem;
  }

  .nav a {
    font-size: 0.78rem;
  }

  .gis-link {
    padding: 0.42rem;
  }

  .gis-icon {
    width: 18px;
    height: 18px;
  }
}



@media (max-width: 360px) {
  .logo-text {
    font-size: 0.82rem;
    letter-spacing: 0.08em;
  }

  .nav {
    gap: 0.58rem;
  }

  .nav a {
    font-size: 0.73rem;
  }
}
</style>