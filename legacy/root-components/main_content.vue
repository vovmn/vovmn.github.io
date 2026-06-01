<script setup>
    defineProps({
        title: String,
        info: String
    })
</script>


<template>
    <div class="content">
        <h2>тут будет название карточки: {{ title }}</h2>
        <h3>тут будет конет карточки: {{ info }}</h3>
    </div>

</template>

<style scoped>
    .content{
    background: white;
    display: grid;
    padding: 2rem;
    border-radius: 12px;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
    }
</style>