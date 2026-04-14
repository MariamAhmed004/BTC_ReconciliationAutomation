<script setup>
const props = defineProps({
  title: { type: String, default: '' },
  headerClass: { type: String, default: '' },
  bodyClass: { type: String, default: '' }
})
</script>

<template>
  <div class="card base-card mb-3">
    <div v-if="title" :class="['card-header text-center', props.headerClass]">
      {{ title }}
    </div>
    <div :class="['card-body d-flex align-items-center justify-content-center', props.bodyClass]">
      <slot>XXXX</slot>
    </div>
  </div>
</template>

<style scoped>
.base-card .card-header {
  background-color: #f1f3f5; /* subtle light header */
  border-bottom: 1px solid rgba(0,0,0,0.08);
  font-weight: 600;
}

.base-card .card-body {
  min-height: 120px; /* similar to the reference image */
}
</style>
