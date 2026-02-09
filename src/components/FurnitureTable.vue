<template>
  <section class="furniture-section">
    <div
      v-for="group in items"
      :key="group.group"
      class="furniture-group"
    >
      <h3 class="group-title">{{ group.group }}</h3>

      <div class="furniture-table-wrapper">
        <table class="furniture-table">
          <thead>
            <tr>
              <th>Наименование</th>
              <th>Цена</th>
              <th>Ед.</th>
            </tr>
          </thead>

          <tbody>
            <tr
              v-for="item in group.items"
              :key="item.name"
            >
              <td>{{ item.name }}</td>
              <td>{{ formatPrice(item.price) }}</td>
              <td>{{ item.unit }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </section>
</template>

<script setup>
defineProps({
  items: {
    type: Array,
    required: true
  }
})

const formatPrice = (price) =>
  new Intl.NumberFormat('ru-RU').format(price) + ' ₽'
</script>

<style scoped>
.furniture-section {
  margin-top: 2rem;
}

.furniture-group {
  margin-bottom: 2.5rem;
}

.group-title {
  font-size: 1.2rem;
  font-weight: 700;
  margin-bottom: 0.75rem;
  color: #2b6cb0;
}

.furniture-table-wrapper {
  overflow-x: auto;
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.06);
}

.furniture-table {
  width: 100%;
  border-collapse: collapse;
}

.furniture-table th,
.furniture-table td {
  padding: 0.75rem 1rem;
  text-align: left;
  font-size: 0.9rem;
}

.furniture-table thead {
  background: #f3f4f6;
}

.furniture-table th {
  font-weight: 600;
  color: #374151;
}

.furniture-table tbody tr:not(:last-child) {
  border-bottom: 1px solid #e5e7eb;
}

.furniture-table tbody tr:hover {
  background: #f9fafb;
}
</style>
  