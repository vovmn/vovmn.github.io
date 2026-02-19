<template>
  <div class="product-card" @click="$emit('cardClick', product)">
    <div class="product-image">
      <img
        :src="product.image"
        :alt="product.name"
        :loading="index < eagerCount ? 'eager' : 'lazy'"
        :fetchpriority="index < highPriorityCount ? 'high' : 'auto'"
        decoding="async"
        @error="onImgError"
      />
      <div class="product-category-badge">{{ getCategoryName(product.category) }}</div>
    </div>

    <div class="product-info">
      <h3 class="product-name">{{ product.name }}</h3>
      <p class="product-description">{{ product.description }}</p>

      <div class="product-footer">
        <p class="product-price">{{ product.price }}</p>
        <button class="details-btn" @click.stop="$emit('detailsClick', product)">
          Подробнее
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>


const props = defineProps({
  product: {
    type: Object,
    required: true,
    default: () => ({
      id: null,
      name: '',
      description: '',
      price: '',
      image: '',
      category: ''
    })
  },
  index: {
    type: Number,
    default: 9999
  },

  eagerCount: {
    type: Number,
    default: 6
  },

  highPriorityCount: {
    type: Number,
    default: 2
  },

  placeholderSrc: {
    type: String,
    default: '/placeholder.webp'
  }
})

defineEmits(['cardClick', 'detailsClick'])

const getCategoryName = (categoryId) => {
  const categories = {
    ldsp: 'ЛДСП',
    countertops: 'Столешницы',
    facades: 'Фасады',
    furniture: 'Фурнитура'
  }
  return categories[categoryId] || categoryId
}

const onImgError = (e) => {
  const img = e?.target
  if (!img) return
  if (img.dataset.fallbackApplied) return
  img.dataset.fallbackApplied = '1'
  img.src = props.placeholderSrc
}
</script>

<style scoped>
.product-card {
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
}

.product-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 5px 20px rgba(0, 0, 0, 0.15);
}

.product-image {
  flex-shrink: 0;
  margin-bottom: 1rem;
  display: flex;
  align-items: center;
  justify-content: center;
  height: 180px;
  position: relative;


  width: 100%;
}

.product-image img {
  max-width: 100%;
  max-height: 160px;

  width: 100%;
  height: 100%;
  object-fit: contain;
}

.product-category-badge {
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

.product-info {
  flex-grow: 1;
  display: flex;
  flex-direction: column;
}

.product-name {
  font-size: 1.2rem;
  font-weight: 600;
  color: #333;
  margin-bottom: 0.5rem;
  line-height: 1.3;
}

.product-description {
  color: #666;
  font-size: 0.9rem;
  margin-bottom: 1rem;
  line-height: 1.4;
  flex-grow: 1;
}

.product-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: auto;
}

.product-price {
  color: #007bff;
  font-weight: bold;
  font-size: 1.3rem;
  margin: 0;
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
}

.details-btn:hover {
  background: #2c5282;
}

@media (max-width: 768px) {
  .product-card {
    padding: 1.2rem;
  }

  .product-image {
    height: 150px;
  }

  .product-image img {
    max-height: 140px;
  }

  .product-name {
    font-size: 1.1rem;
  }

  .product-price {
    font-size: 1.2rem;
  }

  .product-footer {
    flex-direction: column;
    gap: 0.75rem;
    align-items: stretch;
  }

  .details-btn {
    width: 100%;
  }
}

@media (max-width: 480px) {
  .product-image {
    height: 130px;
  }

  .product-image img {
    max-height: 120px;
  }

  .product-category-badge {
    font-size: 0.7rem;
    padding: 0.2rem 0.5rem;
  }
}
</style>
