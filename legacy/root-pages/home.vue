
<script setup>

  import Header from '../components/header.vue'
  import Main_content from '../components/main_content.vue';

</script>

<template>
  <body class="body">
    <Header />
    
    <div class="container">
      <div class="title-content">
        <Main_content />      
      </div>
      <div class="content">
        
        <Main_content />
        <Main_content />
        <Main_content />
        <Main_content />
        <Main_content />
        
      </div>

    </div>
  </body>
     
  
   

</template>

<style>
  .main{
    flex: 1;
    padding: 2rem;
    gap: 1.5rem;
    
  }

  .content{
    display: grid;
    gap: 1.5rem;
    padding: 2rem;
    grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  }
  /* .rightbar{
    width: 256px;
    background-color: rgb(255, 255, 255);
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1); */
  .container{
    min-height: calc(100vh - 80px);
    gap: 1.5rem;
    padding: 2rem;
  }
  
  .body{

    background-color: #f5f7fa;
    color: #333;
    margin: 0;
    padding: 0;
    box-sizing: border-box;
    
  }
</style>