<script>
import Header from '../components/header.vue'
</script>

<template>
  <body class="body">
    <Header />
    
    <div class="container">
 
      <div class="page-header">
        <h1 class="page-title">Запись на прием</h1>
      </div>

      <div class="appointment-content">
        <div class="appointment-form">
          <div class="form-section">
            <h2 class="section-title">Выбор специалиста</h2>
            <div class="form-group">
              <label class="form-label">Специальность врача</label>
              <select class="form-select">
                <option value="">Выберите специальность</option>
              </select>
            </div>

            <div class="form-group">
              <label class="form-label">Врач</label>
              <select class="form-select">
                <option value="">Выберите врача</option>
              </select>
            </div>
          </div>

          <div class="form-section">
            <h2 class="section-title">Выбор даты и времени</h2>
            <div class="form-group">
              <label class="form-label">Дата приема</label>
              <input type="date" class="form-input">
            </div>

            <div class="form-group">
              <label class="form-label">Время приема</label>
              <select class="form-select">
                <option value="">Выберите время</option>
              </select>
            </div>
          </div>

          <div class="form-section">
            <h2 class="section-title">Дополнительная информация</h2>
            <div class="form-group">
              <label class="form-label">Жалобы и симптомы</label>
              <textarea class="form-textarea" placeholder="Опишите ваши жалобы..."></textarea>
            </div>
          </div>

          <button class="submit-btn">Записаться на прием</button>
        </div>

        <div class="appointment-info">
          <div class="info-card">
            <h3 class="info-title">Информация о записи</h3>
            <div class="info-item">
              <span class="info-label">Специалист:</span>
              <span class="info-value">Не выбран</span>
            </div>
            <div class="info-item">
              <span class="info-label">Дата:</span>
              <span class="info-value">Не выбрана</span>
            </div>
            <div class="info-item">
              <span class="info-label">Время:</span>
              <span class="info-value">Не выбрано</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </body>
</template>

<script setup>
import Header from '../components/header.vue'
</script>

<style>
  .body{
    background-color: #f5f7fa;
    color: #333;
    margin: 0;
    padding: 0;
    box-sizing: border-box;
  }

  .container{
    min-height: calc(100vh - 80px);
    gap: 1.5rem;
    padding: 2rem;
  }

  .page-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 2rem;
  }

  .page-title {
    font-size: 1.8rem;
    font-weight: 600;
    color: #2c3e50;
    margin: 0;
  }

  .appointment-content {
    display: grid;
    grid-template-columns: 2fr 1fr;
    gap: 2rem;
  }

  .appointment-form {
    background: white;
    padding: 2rem;
    border-radius: 12px;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  }

  .form-section {
    margin-bottom: 2rem;
  }

  .section-title {
    font-size: 1.2rem;
    font-weight: 600;
    color: #2c3e50;
    margin-bottom: 1rem;
  }

  .form-group {
    margin-bottom: 1rem;
  }

  .form-label {
    display: block;
    font-size: 0.9rem;
    font-weight: 500;
    color: #555;
    margin-bottom: 0.5rem;
  }

  .form-select,
  .form-input,
  .form-textarea {
    width: 100%;
    padding: 0.75rem;
    border: 1px solid #ddd;
    border-radius: 8px;
    font-size: 0.9rem;
    background-color: #fafafa;
    transition: border-color 0.3s;
    box-sizing: border-box;
  }

  .form-textarea {
    min-height: 100px;
    resize: vertical;
  }

  .form-select:focus,
  .form-input:focus,
  .form-textarea:focus {
    outline: none;
    border-color: #3498db;
  }

  .submit-btn {
    background-color: #3498db;
    color: white;
    border: none;
    padding: 1rem 2rem;
    border-radius: 8px;
    font-weight: 500;
    cursor: pointer;
    transition: background-color 0.3s;
    width: 100%;
    font-size: 1rem;
  }

  .submit-btn:hover {
    background-color: #2980b9;
  }

  .appointment-info {
    background: white;
    padding: 2rem;
    border-radius: 12px;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
    height: fit-content;
  }

  .info-card {
    display: flex;
    flex-direction: column;
    gap: 1rem;
  }

  .info-title {
    font-size: 1.2rem;
    font-weight: 600;
    color: #2c3e50;
    margin: 0;
  }

  .info-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0.5rem 0;
    border-bottom: 1px solid #eee;
  }

  .info-label {
    font-weight: 500;
    color: #555;
  }

  .info-value {
    color: #333;
    font-weight: 500;
  }

  /* Адаптивность */
  @media (max-width: 768px) {
    .container {
      padding: 1rem;
    }
    
    .appointment-content {
      grid-template-columns: 1fr;
    }
  }
</style>