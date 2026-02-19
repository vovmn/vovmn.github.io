<template>
  <div class="contact-page">
    <Header />
    
    <main class="contact-main">
      <div class="container">
        <h1 class="page-title">Контакты</h1>
        <p class="page-subtitle">Свяжитесь с нами удобным для вас способом</p>
        

        <div class="contact-info">
          <div class="contact-card">
            <div class="contact-icon">
              <svg class="address-icon" width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
                <path d="M20 4H4c-1.1 0-1.99.9-1.99 2L2 18c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 4l-8 5-8-5V6l8 5 8-5v2z"/>
              </svg>
            </div>
            <div class="contact-details">
              <h3>Email</h3>
              <a href="mailto:kmv.solopov@mail.ru" class="contact-link">
                kmv.solopov@mail.ru
              </a>
            </div>
          </div>

          <div class="contact-card">
            <div class="contact-icon">
              <svg class="address-icon" width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
                <path d="M6.62 10.79c1.44 2.83 3.76 5.14 6.59 6.59l2.2-2.2c.27-.27.67-.36 1.02-.24 1.12.37 2.33.57 3.57.57.55 0 1 .45 1 1V20c0 .55-.45 1-1 1-9.39 0-17-7.61-17-17 0-.55.45-1 1-1h3.5c.55 0 1 .45 1 1 0 1.25.2 2.45.57 3.57.11.35.03.74-.25 1.02l-2.2 2.2z"/>
              </svg>
            </div>
            <div class="contact-details">
              <h3>Телефоны</h3>
              <div class="phone-numbers">
                <p>8 (8452) 65-99-99</p>
                <p>8 (8452) 65-96-96</p>
                <p>8 (8452) 65-99-66</p>
                <p>8-917-214-54-84</p>
              </div>
            </div>
          </div>

          <div class="contact-card">
            <div class="contact-icon">
              <svg class="address-icon" width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
                <path d="M11.99 2C6.47 2 2 6.48 2 12s4.47 10 9.99 10C17.52 22 22 17.52 22 12S17.52 2 11.99 2zM12 20c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8zm.5-13H11v6l5.25 3.15.75-1.23-4.5-2.67z"/>
              </svg>
            </div>
            <div class="contact-details">
              <h3>Режим работы</h3>
              <p>Пн-Пт: 9:00 - 17:00</p>
              <p>Сб: 9:00 - 12:00</p>
              <p>Вс: выходной</p>
            </div>
          </div>
        </div>


        <div class="addresses-section">
          <h2>Наши адреса в Саратове</h2>
          

          <div class="address-section">
            <h3><svg class="address-icon" width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
              <path d="M18 2h-8L4.02 8 4 20c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zm0 18H6V8.83L10.83 4H18v16zM9 7h2v4H9zm3 0h2v4h-2zm3 0h2v4h-2z"/>
            </svg> Основной офис: ул. Огородная, 3д</h3>
            <div class="yandex-map">

              <div id="map-ogorodnaya" style="width: 100%; height: 400px;"></div>
              <div class="map-info">
                <p><strong>Адрес:</strong> г. Саратов, ул. Огородная, 3д</p>
                <a href="https://yandex.com/maps/-/CPA57TIN" 
                   target="_blank" 
                   class="map-link">
                  Открыть в Яндекс.Картах
                </a>
              </div>
            </div>
          </div>


          <div class="address-section">
            <h3><svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor" style="display: inline-block; margin-right: 8px; vertical-align: middle;">
              <path d="M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm0 16H5V5h14v14zM7 10h2v7H7zm4-3h2v10h-2zm4-4h2v14h-2z"/>
            </svg> Офис: ул. Техническая, д. 5</h3>
            <div class="yandex-map">

              <div id="map-tehnicheskaya" style="width: 100%; height: 400px;"></div>
              <div class="map-info">
                <p><strong>Адрес:</strong> г. Саратов, ул. Техническая, д. 5</p>
                <a href="https://yandex.com/maps/-/CPA5v-1D" 
                   target="_blank" 
                   class="map-link">
                  Открыть в Яндекс.Картах
                </a>
              </div>
            </div>
          </div>


          <div class="address-section">
            <h3><svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor" style="display: inline-block; margin-right: 8px; vertical-align: middle;">
              <path d="M20 2H4c-1 0-2 .9-2 2v3.01c0 .72.43 1.34 1 1.69V20c0 1.1 1.1 2 2 2h14c.9 0 2-.9 2-2V8.7c.57-.35 1-.97 1-1.69V4c0-1.1-1-2-2-2zm-5 12H9v-2h6v2zm5-7H4V4h16v3z"/>
            </svg> Офис: ул. Блинова, д. 52Б</h3>
            <div class="yandex-map">

              <div id="map-blinova" style="width: 100%; height: 400px;"></div>
              <div class="map-info">
                <p><strong>Адрес:</strong> г. Саратов, ул. Блинова, д. 52Б</p>
                <a href="https://yandex.com/maps/-/CPA57M81" 
                   target="_blank" 
                   class="map-link">
                  Открыть в Яндекс.Картах
                </a>
              </div>
            </div>
          </div>
        </div>
      </div>
    </main>

    <Footer />
  </div>
</template>

<script setup>
import { onMounted } from 'vue'
import Header from '@/components/Header.vue'
import Footer from '@/components/Footer.vue'


const initYandexMaps = () => {
  if (typeof ymaps === 'undefined') {
 
    const script = document.createElement('script')
    script.src = 'https://api-maps.yandex.ru/2.1/?apikey=ваш_api_ключ&lang=ru_RU'
    script.async = true
    document.head.appendChild(script)
    
    script.onload = () => {
      ymaps.ready(initMaps)
    }
  } else {
    ymaps.ready(initMaps)
  }
}

const initMaps = () => {

  const mapOgorodnaya = new ymaps.Map('map-ogorodnaya', {
    center: [51.515378, 45.977223], 
    zoom: 17
  })
  
  const placemarkOgorodnaya = new ymaps.Placemark([51.515378, 45.977223], {
    balloonContent: '<strong>ул. Огородная, 3д</strong><br>Основной офис Аквилон'
  }, {
    preset: 'islands#icon',
    iconColor: '#2b6cb0'
  })
  
  mapOgorodnaya.geoObjects.add(placemarkOgorodnaya)
  

  const mapTehnicheskaya = new ymaps.Map('map-tehnicheskaya', {
    center: [51.576461, 46.011934], 
    zoom: 17
  })
  
  const placemarkTehnicheskaya = new ymaps.Placemark([51.576461, 46.011934], {
    balloonContent: '<strong>ул. Техническая, 5</strong><br>Производство Аквилон'
  }, {
    preset: 'islands#icon',
    iconColor: '#38a169'
  })
  
  mapTehnicheskaya.geoObjects.add(placemarkTehnicheskaya)
  

  const mapBlinova = new ymaps.Map('map-blinova', {
    center: [51.598688, 46.006659],
    zoom: 17
  })
  
  const placemarkBlinova = new ymaps.Placemark([51.598688, 46.006659], {
    balloonContent: '<strong>ул. Блинова, 52Б</strong><br>Склад Аквилон'
  }, {
    preset: 'islands#icon',
    iconColor: '#e53e3e'
  })
  
  mapBlinova.geoObjects.add(placemarkBlinova)
}

onMounted(() => {
  initYandexMaps()
})
</script>

<style scoped>
.contact-page {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.contact-main {
  flex: 1;
  padding: 2rem 0;
  background: #f8f9fa;
}

.container {
  max-width: 800px;
  margin: 0 auto;
  padding: 0 1rem;
}

.page-title {
  text-align: center;
  font-size: 2.5rem;
  font-weight: 700;
  color: #333;
  margin-bottom: 1rem;
}

.page-subtitle {
  text-align: center;
  font-size: 1.2rem;
  color: #666;
  margin-bottom: 3rem;
}

/* ===== Контактные карточки ===== */
.contact-info {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  max-width: 600px;
  margin: 0 auto 4rem auto;
}

.contact-card {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
  padding: 1.5rem;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.contact-icon {
  flex: 0 0 auto;
  color: #2b6cb0;
  margin-top: 0.2rem;
}

.contact-details {
  min-width: 0;
}

.contact-details h3 {
  margin: 0 0 0.5rem 0;
  color: #333;
  font-size: 1.1rem;
}

.contact-details p {
  margin: 0.25rem 0;
  color: #666;
}

.contact-link {
  color: #2b6cb0;
  text-decoration: none;
}

.contact-link:hover {
  text-decoration: underline;
}

.phone-numbers p {
  margin: 0.25rem 0;
  color: #666;
}

/* ===== Адреса ===== */
.addresses-section {
  margin-top: 2rem;
}

.addresses-section h2 {
  text-align: center;
  margin-bottom: 2rem;
  color: #333;
}

/* 🔧 ГЛАВНЫЙ ФИКС SVG + ТЕКСТ */
.address-section {
  margin-bottom: 3rem;
}

.address-section h3 {
  display: flex;
  align-items: flex-start;
  gap: 0.5rem;

  margin-bottom: 1rem;
  color: #333;
  font-size: 1.3rem;
  line-height: 1.35;

  flex-wrap: wrap;
}

.address-section h3 svg {
  flex: 0 0 auto;
  color: #2b6cb0;
  margin-top: 0.15rem;
}

/* ===== Карта ===== */
.yandex-map {
  background: white;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.map-info {
  padding: 1rem 1.5rem;
  background: #f8f9fa;
  border-top: 1px solid #e9ecef;
}

.map-info p {
  margin: 0 0 0.5rem 0;
  color: #666;
}

.map-link {
  display: inline-block;
  color: #2b6cb0;
  text-decoration: none;
  font-size: 0.9rem;
  padding: 0.5rem 1rem;
  background: white;
  border: 1px solid #2b6cb0;
  border-radius: 4px;
  transition: all 0.2s;
}

.map-link:hover {
  background: #2b6cb0;
  color: white;
}

/* ===== Адаптив ===== */
@media (max-width: 768px) {
  .page-title {
    font-size: 2rem;
  }

  .container {
    max-width: 100%;
  }

  .contact-card {
    padding: 1.2rem;
  }

  .address-section h3 {
    font-size: 1.2rem;
  }

  .yandex-map > div {
    height: 300px !important;
  }
}

@media (max-width: 480px) {
  .page-title {
    font-size: 1.8rem;
  }

  .page-subtitle {
    font-size: 1.1rem;
  }

  .contact-info {
    gap: 1rem;
    margin-bottom: 3rem;
  }

  .address-section h3 {
    font-size: 1.1rem;
  }

  .yandex-map > div {
    height: 250px !important;
  }

  .map-info {
    padding: 0.75rem 1rem;
  }
}

</style>