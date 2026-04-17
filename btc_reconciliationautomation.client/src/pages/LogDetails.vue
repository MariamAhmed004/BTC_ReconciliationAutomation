<script setup>
import { useRoute } from 'vue-router'
const route = useRoute()
const id = route.params.id
</script>

<template>
  <div class="container pt-4">
    <h3>Execution Details - {{ id }}</h3>
    <p class="text-muted">Details for execution id {{ id }} will be shown here. (Placeholder)</p>
  </div>
</template>

<style scoped></style>
