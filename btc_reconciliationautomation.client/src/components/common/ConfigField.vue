<script setup>
import { ref } from 'vue'
import BaseModal from './BaseModal.vue'

const props = defineProps({
  label: { type: String, required: true },
  icon: { type: String, required: true },
  modelValue: { type: String, default: '' },
  placeholder: { type: String, default: '' },
  type: { type: String, default: 'text' },
  infoText: { type: String, default: '' }
})

const emit = defineEmits(['update:modelValue'])

const showInfoModal = ref(false)

function updateValue(event) {
  emit('update:modelValue', event.target.value)
}
</script>

<template>
  <div class="config-field-wrapper mb-4">
    <label class="field-label mb-1">{{ label }}</label>
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
      <div class="field-info">
        <button
          type="button"
          class="btn btn-link p-0 info-btn"
          @click="showInfoModal = true"
          :title="`About ${label}`"
        >
          <i class="bi bi-info-circle"></i>
        </button>
      </div>
    </div>

    <BaseModal
      :show="showInfoModal"
      :title="label"
      :message="infoText"
      @close="showInfoModal = false"
    />
  </div>
</template>

<style scoped>
.config-field-wrapper {
  max-width: 100%;
}

.field-label {
  display: block;
  font-size: 0.875rem;
  font-weight: 600;
  color: #495057;
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
  border: none;
  font-size: 0.95rem;
  color: #495057;
}

.field-input .form-control:focus {
  background: #fff;
  border: none;
  outline: none;
  box-shadow: none;
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
</style>
