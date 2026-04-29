<script setup>
import { ref } from 'vue'

const props = defineProps({
  label: { type: String, required: true },
  icon: { type: String, required: true },
  modelValue: { type: String, default: '' },
  placeholder: { type: String, default: '' },
  type: { type: String, default: 'text' },
  infoText: { type: String, default: '' }
})

const emit = defineEmits(['update:modelValue'])

const showTooltip = ref(false)

function updateValue(event) {
  emit('update:modelValue', event.target.value)
}
</script>

<template>
  <div class="config-field-wrapper mb-3">
    <div class="d-flex align-items-center gap-2">
      <div class="field-icon">
        <i :class="`bi ${icon}`"></i>
      </div>
      <div class="field-input flex-grow-1">
        <input
          :type="type"
          :value="modelValue"
          :placeholder="placeholder"
          @input="updateValue"
          class="form-control"
        />
      </div>
      <div class="field-info position-relative">
        <button
          type="button"
          class="btn btn-link p-0 info-btn"
          @mouseenter="showTooltip = true"
          @mouseleave="showTooltip = false"
          @focus="showTooltip = true"
          @blur="showTooltip = false"
        >
          <i class="bi bi-info-circle"></i>
        </button>
        <div v-if="showTooltip && infoText" class="info-tooltip">
          {{ infoText }}
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.config-field-wrapper {
  max-width: 100%;
}

.field-icon {
  width: 48px;
  height: 48px;
  background: #e9ecef;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
  color: #495057;
  flex-shrink: 0;
}

.field-input {
  flex: 1;
}

.field-input .form-control {
  height: 48px;
  background: #e9ecef;
  border: 1px solid #dee2e6;
  font-size: 0.95rem;
  color: #495057;
}

.field-input .form-control:focus {
  background: #fff;
  border-color: #0d6efd;
  box-shadow: 0 0 0 0.2rem rgba(13, 110, 253, 0.15);
}

.field-info {
  flex-shrink: 0;
}

.info-btn {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #6c757d;
  text-decoration: none;
  font-size: 1.25rem;
  transition: color 0.2s;
}

.info-btn:hover,
.info-btn:focus {
  color: #0d6efd;
}

.info-tooltip {
  position: absolute;
  right: 0;
  top: 100%;
  margin-top: 8px;
  width: 250px;
  padding: 12px;
  background: #343a40;
  color: #fff;
  border-radius: 6px;
  font-size: 0.85rem;
  line-height: 1.4;
  z-index: 1000;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.info-tooltip::before {
  content: '';
  position: absolute;
  bottom: 100%;
  right: 8px;
  border: 6px solid transparent;
  border-bottom-color: #343a40;
}
</style>
