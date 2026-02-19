<template>
  <div class="modal-overlay" @click.self="closeModal" role="dialog" aria-modal="true">
    <div class="modal-content">

      <div class="modal-header">
        <h2 class="modal-title">{{ product?.name }}</h2>
        <button class="close-btn" @click="closeModal" type="button" aria-label="Закрыть">×</button>
      </div>


      <div class="modal-body">

        <div class="product-preview">
          <div class="image-wrap">
            <img
              v-if="product?.image"
              :src="product.image"
              :alt="product?.name || 'Товар'"
              class="preview-image"
            />
          </div>

          <div class="product-info">
            <p v-if="product?.description" class="product-description">
              {{ product.description }}
            </p>

            <div v-if="product?.price" class="price-range">
              <span class="price-label">Цена:</span>
              <span class="price-value">{{ product.price }}</span>
            </div>
          </div>
        </div>


        <div class="sizes-prices-section">
          <div class="section-head">
            <h3 class="section-title">
              <span class="icon-size">📏</span> Размеры и цены
            </h3>

            <div class="unit-control">
              <label for="sizeUnit">Единица:</label>
              <select id="sizeUnit" v-model="sizeUnit" class="unit-select">
                <option value="mm">мм</option>
                <option value="cm">см</option>
                <option value="m">м</option>
              </select>
            </div>
          </div>

          <div class="table-container">
            <table class="sizes-table">
              <thead>
                <tr>
                  <th @click="sortSizes('width')" role="button" tabindex="0">
                    Ширина
                    <span v-if="sortColumn === 'width'" class="sort-arrow">{{ sortDirection === 'asc' ? '↑' : '↓' }}</span>
                  </th>
                  <th @click="sortSizes('height')" role="button" tabindex="0">
                    Высота
                    <span v-if="sortColumn === 'height'" class="sort-arrow">{{ sortDirection === 'asc' ? '↑' : '↓' }}</span>
                  </th>
                  <th @click="sortSizes('depth')" role="button" tabindex="0">
                    Глубина/Толщина
                    <span v-if="sortColumn === 'depth'" class="sort-arrow">{{ sortDirection === 'asc' ? '↑' : '↓' }}</span>
                  </th>
                  <th @click="sortSizes('price')" role="button" tabindex="0" class="th-price">
                    Цена
                    <span v-if="sortColumn === 'price'" class="sort-arrow">{{ sortDirection === 'asc' ? '↑' : '↓' }}</span>
                  </th>
                </tr>
              </thead>

              <tbody v-if="sortedSizes.length">
                <tr v-for="item in sortedSizes" :key="item.id">
                  <td>{{ formatSize(item.width) }}</td>
                  <td>{{ formatSize(item.height) }}</td>
                  <td>{{ formatSize(item.depth) }}</td>
                  <td class="price-cell">{{ formatPrice(item.price) }}</td>
                </tr>
              </tbody>

              <tbody v-else>
                <tr>
                  <td colspan="4" class="empty-row">
                    Размеры пока не добавлены
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>


      <div class="modal-footer">
        <button class="btn-close" @click="closeModal" type="button">
          <span class="icon-close">←</span> Вернуться в каталог
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue'

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
      sizes: []
    })
  }
})

const emit = defineEmits(['close'])

const sortColumn = ref('width')
const sortDirection = ref('asc')
const sizeUnit = ref('mm')

const closeModal = () => emit('close')

const sortSizes = (column) => {
  if (sortColumn.value === column) {
    sortDirection.value = sortDirection.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortColumn.value = column
    sortDirection.value = 'asc'
  }
}

const sortedSizes = computed(() => {
  const sizes = props.product?.sizes || []
  if (!sizes.length) return []

  return [...sizes].sort((a, b) => {
    const aVal = a?.[sortColumn.value]
    const bVal = b?.[sortColumn.value]

    if (sortColumn.value === 'price') {
      return sortDirection.value === 'asc' ? (aVal || 0) - (bVal || 0) : (bVal || 0) - (aVal || 0)
    }

    const cmp = (aVal || 0) - (bVal || 0)
    return sortDirection.value === 'asc' ? cmp : -cmp
  })
})

const formatPrice = (price) => {
  if (price === null || price === undefined || price === '') return '-'
  return new Intl.NumberFormat('ru-RU', {
    style: 'currency',
    currency: 'RUB',
    minimumFractionDigits: 0
  }).format(Number(price))
}

const formatSize = (size) => {
  if (!size || size === 0) return '-'
  const sizeInMm = Number(size)

  switch (sizeUnit.value) {
    case 'cm':
      return `${(sizeInMm / 10).toFixed(1)} см`
    case 'm':
      return `${(sizeInMm / 1000).toFixed(3)} м`
    default:
      return `${sizeInMm} мм`
  }
}
</script>

<style scoped>
/* overlay */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.7);
  display: grid;
  place-items: center;
  z-index: 1000;
  padding: 16px;
}

/* container */
.modal-content {
  width: min(960px, 100%);
  max-height: min(92vh, 920px);
  background: #fff;
  border-radius: 14px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.30);
  display: grid;
  grid-template-rows: auto 1fr auto;
  overflow: hidden;
}

/* header */
.modal-header {
  display: grid;
  grid-template-columns: 1fr auto;
  gap: 12px;
  align-items: center;
  padding: 16px 20px;
  background: #f8f9fa;
  border-bottom: 1px solid #e9ecef;
}

.modal-title {
  margin: 0;
  font-size: 1.35rem;
  color: #333;
  line-height: 1.2;
}

.close-btn {
  width: 40px;
  height: 40px;
  border: none;
  background: transparent;
  font-size: 28px;
  line-height: 1;
  border-radius: 50%;
  cursor: pointer;
  color: #666;
  display: grid;
  place-items: center;
  transition: background 0.2s, color 0.2s, transform 0.2s;
}
.close-btn:hover { background: #e9ecef; color: #333; }
.close-btn:active { transform: scale(0.95); }


.modal-body {
  padding: 18px 20px;
  overflow: auto;
}


.product-preview {
  display: grid;
  grid-template-columns: 1fr 1.2fr;
  gap: 18px;
  padding-bottom: 18px;
  margin-bottom: 18px;
  border-bottom: 2px solid #e9ecef;
}

.image-wrap {
  background: #f8f9fa;
  border-radius: 10px;
  padding: 12px;
  display: grid;
  place-items: center;
  min-height: 220px;
}

.preview-image {
  max-width: 100%;
  max-height: 320px;
  object-fit: contain;
}

.product-info {
  display: grid;
  gap: 12px;
  align-content: start;
}

.product-description {
  margin: 0;
  color: #666;
  line-height: 1.55;
}

.price-range {
  display: inline-flex;
  gap: 10px;
  align-items: center;
  padding: 12px;
  background: #f8f9fa;
  border-radius: 10px;
  border-left: 4px solid #2b6cb0;
}

.price-label {
  font-weight: 600;
  color: #333;
}

.price-value {
  font-weight: 800;
  color: #2b6cb0;
  font-size: 1.2rem;
}

/* sizes section */
.sizes-prices-section {
  display: grid;
  gap: 12px;
}

.section-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  flex-wrap: wrap;
}

.section-title {
  margin: 0;
  font-size: 1.2rem;
  color: #333;
  display: inline-flex;
  align-items: center;
  gap: 8px;
}

.unit-control {
  display: inline-flex;
  align-items: center;
  gap: 10px;
  padding: 10px 12px;
  background: #f8f9fa;
  border-radius: 10px;
}

.unit-control label {
  font-size: 0.9rem;
  color: #666;
  font-weight: 600;
}

.unit-select {
  padding: 8px 10px;
  border: 1px solid #ddd;
  border-radius: 8px;
  background: #fff;
  font-size: 0.9rem;
  min-width: 90px;
}

/* table */
.table-container {
  border: 1px solid #e9ecef;
  border-radius: 10px;
  overflow: auto; 
  -webkit-overflow-scrolling: touch;
}

.sizes-table {
  width: 100%;
  border-collapse: collapse;
  min-width: 640px; 
}

.sizes-table th {
  background: #f8f9fa;
  padding: 14px;
  text-align: left;
  font-weight: 700;
  color: #333;
  border-bottom: 2px solid #dee2e6;
  cursor: pointer;
  user-select: none;
  white-space: nowrap;
}

.sizes-table td {
  padding: 14px;
  border-bottom: 1px solid #e9ecef;
  vertical-align: middle;
  white-space: nowrap;
}

.sizes-table tr:hover { background: #f8f9fa; }

.sort-arrow {
  margin-left: 8px;
  font-weight: 800;
  color: #2b6cb0;
}

.price-cell {
  font-weight: 800;
  color: #2b6cb0;
}

.empty-row {
  text-align: center;
  color: #666;
  padding: 18px;
}

/* footer */
.modal-footer {
  padding: 14px 20px;
  background: #f8f9fa;
  border-top: 1px solid #e9ecef;
  display: grid;
  place-items: center;
}

.btn-close {
  padding: 12px 18px;
  background: #2b6cb0;
  color: #fff;
  border: none;
  border-radius: 10px;
  cursor: pointer;
  transition: transform 0.2s, background 0.2s;
  display: inline-flex;
  align-items: center;
  gap: 10px;
  font-size: 1rem;
}
.btn-close:hover { background: #2c5282; transform: translateY(-1px); }
.btn-close:active { transform: translateY(0); }

.icon-close { font-size: 1.2em; }

@media (max-width: 900px) {
  .modal-content {
    width: min(900px, 100%);
    max-height: 92vh;
  }
  .product-preview {
    grid-template-columns: 1fr;
  }
  .image-wrap {
    min-height: 180px;
  }
  .preview-image {
    max-height: 260px;
  }
}

@media (max-width: 480px) {
  .modal-overlay {
    padding: 10px;
  }

  .modal-header {
    padding: 12px 14px;
  }
  .modal-body {
    padding: 12px 14px;
  }
  .modal-footer {
    padding: 12px 14px;
  }

  .modal-title {
    font-size: 1.15rem;
  }

  .unit-control {
    width: 100%;
    justify-content: space-between;
  }


  .sizes-table {
    min-width: 620px;
  }
}
</style>
