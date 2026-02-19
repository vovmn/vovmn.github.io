<template>
  <div class="service-card" @click="$emit('cardClick', service)">
    <div class="service-image">

      <div class="service-icon">
        <slot name="icon">
          <svg width="48" height="48" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
            <path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5"/>
          </svg>
        </slot>
      </div>

      <div v-if="service.category" class="service-category-badge">
        {{ getCategoryName(service.category) }}
      </div>
    </div>

    <div class="service-info">
      <h3 class="service-name">{{ service.name }}</h3>
      <p class="service-description">{{ service.description }}</p>

      <div class="service-footer">
        <p class="service-price" v-if="service.price">{{ service.price }}</p>

        <RouterLink to="/contact" class="contact-link">
          <button class="details-btn" @click.stop="$emit('detailsClick', service)">
            Связаться
          </button>
        </RouterLink>
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps({
  service: {
    type: Object,
    required: true,
    default: () => ({
      id: null,
      name: '',
      description: '',
      price: '',
      category: ''
    })
  }
})

defineEmits(['cardClick', 'detailsClick'])

const getCategoryName = (categoryId) => {
  const categories = {
    countertops: 'Столешницы',
    edging: 'Кромление',
    cutting: 'Раскрой',
    drilling: 'Сверление/пазы',
    sawing: 'Рез',
    custom: 'Нестандарт'
  }
  return categories[categoryId] || categoryId
}
</script>

<style scoped>

.service-card,
.service-card * {
  box-sizing: border-box;
}


.service-card {
  background: white;
  padding: 1.5rem;
  border-radius: 12px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  transition: all 0.3s ease;
  border: 1px solid #e9ecef;
  display: flex;
  flex-direction: column;
  height: 100%;
  cursor: pointer;
  position: relative;
  min-width: 0;
}

.service-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 5px 20px rgba(0, 0, 0, 0.15);
}


.service-image {
  flex-shrink: 0;
  margin-bottom: 1rem;
  display: flex;
  align-items: center;
  justify-content: center;
  height: 180px;
  position: relative;
  width: 100%;


  overflow: hidden;
  border-radius: 10px;
}


.service-icon {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;

  background: #f3f4f6;
  color: #2b6cb0;
}

.service-icon svg {
  width: 56px;
  height: 56px;
}


.service-category-badge {
  position: absolute;
  top: 0.5rem;
  right: 0.5rem;
  background: #2b6cb0;
  color: white;
  padding: 0.25rem 0.75rem;
  border-radius: 15px;
  font-size: 0.75rem;
  font-weight: 500;
}

.service-info {
  flex-grow: 1;
  display: flex;
  flex-direction: column;

  min-width: 0;
}

.service-name {
  font-size: 1.2rem;
  font-weight: 600;
  color: #333;
  margin-bottom: 0.5rem;
  line-height: 1.3;


  word-break: break-word;
}

.service-description {
  color: #666;
  font-size: 0.9rem;
  margin-bottom: 1rem;
  line-height: 1.4;
  flex-grow: 1;

  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;


  min-height: calc(1.4em * 3);
}


.service-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: auto;
  gap: 0.75rem;
  min-width: 0;
}

.service-price {
  color: #007bff;
  font-weight: bold;
  font-size: 1.3rem;
  margin: 0;
  white-space: nowrap;
}


.contact-link {
  display: inline-flex;
  min-width: 0;
}

.details-btn {
  background: #2b6cb0;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.9rem;
  transition: background 0.3s ease;


  white-space: nowrap;
}

.details-btn:hover {
  background: #2c5282;
}


@media (max-width: 768px) {
  .service-card {
    padding: 1.2rem;
  }

  .service-image {
    height: 150px;
  }

  .service-icon svg {
    width: 50px;
    height: 50px;
  }

  .service-name {
    font-size: 1.1rem;
  }

  .service-price {
    font-size: 1.2rem;
  }

  .service-footer {
    flex-direction: column;
    gap: 0.75rem;
    align-items: stretch;
  }

  .contact-link {
    width: 100%;
  }

  .details-btn {
    width: 100%;
  }
}

@media (max-width: 480px) {
  .service-image {
    height: 130px;
  }

  .service-icon svg {
    width: 44px;
    height: 44px;
  }

  .service-category-badge {
    font-size: 0.7rem;
    padding: 0.2rem 0.5rem;
  }
}
</style>
