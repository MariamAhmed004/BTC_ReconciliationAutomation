<script setup>
const props = defineProps({
  title: { type: String, required: true },
  subtitle: { type: String, default: '' },
  instruction: { type: String, default: '' },
  iconSrc: { type: String, default: null }
})
</script>

<template>
  <div class="page-header mb-4">
    <div class="d-flex align-items-center mb-3">
      <div class="icon-box me-3">
        <slot name="icon">
          <img v-if="iconSrc" :src="iconSrc" alt="icon" />
          <!-- default inline icon fallback -->
          <svg v-else width="36" height="36" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" aria-hidden>
            <rect x="3" y="3" width="18" height="18" rx="3" fill="#6c757d" />
            <path d="M8 12h8M8 16h8M8 8h8" stroke="#fff" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" />
          </svg>
        </slot>
      </div>
      <h2 class="mb-0">{{ title }}</h2>
    </div>

    <hr />

    <div class="text-start mt-3">
      <div class="subtitle">{{ subtitle }}</div>
      <div class="instruction">{{ instruction }}</div>
    </div>
  </div>
</template>

<style scoped>
.page-header h2 {
  font-weight: 700;
  font-size: 1.6rem;
  color: #333;
}
.icon-box {
  width: 56px;
  height: 56px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: #f1f3f5;
  border-radius: 8px;
}
.icon-box img {
  width: 36px;
  height: 36px;
}
.page-header hr {
  border-top: 2px solid rgba(0,0,0,0.12);
  margin-top: 0.75rem;
}
.subtitle {
  font-weight: 700;
  font-size: 1.1rem;
  color: #333;
}
.instruction {
  font-style: italic;
  color: #666;
  margin-top: 0.25rem;
}
</style>
