<script setup>
import { ref, watch, computed } from 'vue'

const props = defineProps({
  filters: { type: Array, default: () => [] },
})

const emit = defineEmits(['apply', 'clear'])

const open = ref(true)
const criteria = ref({})

// initialize criteria keys
function init() {
  criteria.value = {}
  props.filters.forEach(f => { criteria.value[f.key] = f.default ?? '' })
}

init()

function applyFilters() {
  emit('apply', { ...criteria.value })
  open.value = false
}

function clearFilters() {
  init()
  emit('clear')
}
</script>

<template>
  <div class="base-filter p-2 border rounded bg-white shadow-sm">
    <div class="d-flex flex-wrap gap-2">
      <div v-for="f in props.filters" :key="f.key" class="filter-field me-2 mb-2">
        <label class="form-label small mb-1">{{ f.label }}</label>
        <div v-if="f.type === 'select'">
          <select v-model="criteria[f.key]" class="form-select form-select-sm">
            <option value="">All</option>
            <option v-for="opt in f.options" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
          </select>
        </div>
        <div v-else-if="f.type === 'date'">
          <input type="date" v-model="criteria[f.key]" class="form-control form-control-sm" />
        </div>
        <div v-else>
          <input type="text" v-model="criteria[f.key]" class="form-control form-control-sm" :placeholder="f.placeholder || ''" />
        </div>
      </div>
    </div>

    <div class="d-flex justify-content-end gap-2 mt-2">
      <button class="btn btn-sm btn-outline-secondary" type="button" @click="clearFilters">Clear</button>
      <button class="btn btn-sm btn-primary" type="button" @click="applyFilters">Apply</button>
    </div>
  </div>
</template>

<style scoped>
.base-filter { min-width: 320px }
.filter-field { min-width: 140px }
</style>
