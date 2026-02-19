<template>
  <header class="header">
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


    <nav class="nav">
      <RouterLink to="/">Главная</RouterLink>
      <RouterLink to="/catalog">Каталог</RouterLink>
      <RouterLink to="/services">Услуги</RouterLink> 
      <RouterLink to="/contact">Контакты</RouterLink>
      <RouterLink to="/about">О нас</RouterLink>
      
      
      
    </nav>

    <div class="header-right">
      <a href="https://go.2gis.com/v3vLE" target="_blank" class="gis-link">
        <svg class="gis-icon" width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
          <path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5c-1.38 0-2.5-1.12-2.5-2.5s1.12-2.5 2.5-2.5 2.5 1.12 2.5 2.5-1.12 2.5-2.5 2.5z"/>
        </svg>
        2GIS
      </a>
    </div>
  </header>
</template>

<script setup>
import { RouterLink } from 'vue-router'
</script>

<style scoped>

.header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 2rem;
  background: #ffffff;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
  font-family: "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
  letter-spacing: 0.02em;
}



.logo {
  flex-shrink: 0;
}

.logo-link {
  display: flex;
  align-items: center;
  
  text-decoration: none;
  color: #1f2937;
}


.logo-image {
  height: 56px;
  width: auto;
  display: block;
  object-fit: contain;
}


.logo-text {
  position: relative;
  font-family: "Georgia", "Times New Roman", serif;
  font-size: 1.35rem;
  letter-spacing: 0.28em;
  font-weight: 500;
  white-space: nowrap;
  padding-bottom: 0.4rem;
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
.logo-link:hover {
  opacity: 0.9;
  transform: translateY(-1px);
  transition: opacity 0.2s ease, transform 0.2s ease;
}
@media (max-width: 900px) {
  .logo-image {
    height: 48px;
  }
    .logo-text {
    font-size: 1.05rem;
    letter-spacing: 0.2em;
  }

  .logo-text::after {
    height: 1.5px;
  }
}

@media (max-width: 600px) {
  .logo-image {
    height: 42px;
  }
  .logo-text {
    font-size: 1.05rem;
    letter-spacing: 0.2em;
  }
}


.nav {
  display: flex;
  gap: 1.8rem;
  position: absolute;
  left: 50%;
  transform: translateX(-50%);
}

.nav a {
  position: relative;
  font-size: 0.95rem;
  color: #444;
  text-decoration: none;
  transition: color 0.25s;
}

.nav a::after {
  content: "";
  position: absolute;
  left: 0;
  bottom: -4px;
  width: 0;
  height: 2px;
  background: #c00;
  transition: width 0.25s;
}

.nav a:hover {
  color: #c00;
}

.nav a:hover::after {
  width: 100%;
}


.nav a.router-link-active {
  color: #c00;
}

.nav a.router-link-active::after {
  width: 100%;
}


.header-right {
  flex-shrink: 0;
}

.gis-link {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  background: #2b6cb0;
  color: white;
  text-decoration: none;
  border-radius: 6px;
  font-size: 0.9rem;
  font-weight: 500;
  transition: all 0.3s ease;
  box-shadow: 0 2px 4px rgba(43, 108, 176, 0.3);
}

.gis-link:hover {
  background: #2c5282;
  transform: translateY(-1px);
  box-shadow: 0 4px 8px rgba(43, 108, 176, 0.4);
}

.gis-icon {
  flex-shrink: 0;
}


@media (max-width: 900px) {
  .header {
    flex-wrap: wrap;
    row-gap: 1rem;
  }
  .nav {
    position: static;
    transform: none;
    order: 3;
    flex-basis: 100%;
    justify-content: center;
    gap: 1.2rem;
  }
  .header-right {
    order: 2;
  }
  
  .gis-link {
    padding: 0.4rem 0.8rem;
    font-size: 0.85rem;
  }
}

@media (max-width: 600px) {
  .gis-link span {
    display: none;
  }
  .gis-link {
    padding: 0.5rem;
  }
  .gis-icon {
    margin: 0;
  }
}

</style>