<script setup>
import { ref, computed } from 'vue'

const props = defineProps({
  filters: { type: Array, default: () => [] }
})

const emit = defineEmits(['apply', 'clear', 'close'])

const criteria = ref({})
const sortKey = ref('')
const sortDir = ref('asc')

// Filter out any undefined/null values from filters array
const validFilters = computed(() => {
  return (props.filters || []).filter(f => f != null)
})

// Get filters that can be sorted
const sortableFilters = computed(() => {
  return validFilters.value.filter(f => f.sortable !== false)
})

// initialize criteria keys
function init() {
  criteria.value = {}
  validFilters.value.forEach(f => { criteria.value[f.key] = f.default ?? '' })
  sortKey.value = ''
  sortDir.value = 'asc'
}

init()

function applyFilters() {
  const payload = { ...criteria.value }
  if (sortKey.value) payload.sort = { key: sortKey.value, dir: sortDir.value }
  emit('apply', payload)
  emit('close')
}

function clearFilters() {
  init()
  emit('clear')
}
</script>

<template>
  <div class="base-filter p-4 border rounded bg-white shadow-sm mb-3">
    <!-- Sort Section -->
    <div class="row mb-3">
      <div class="col-12">
        <div class="d-flex align-items-end gap-3 flex-wrap">
          <div>
            <label class="form-label small mb-2 fw-semibold">Sort by</label>
            <select v-model="sortKey" class="form-select form-select-sm" style="min-width: 180px">
              <option value="">None</option>
              <option v-for="f in sortableFilters" :key="f.key" :value="f.key">{{ f.label }}</option>
            </select>
          </div>
          <div v-if="sortKey">
            <label class="form-label small mb-2 fw-semibold">Direction</label>
            <select v-model="sortDir" class="form-select form-select-sm" style="min-width: 150px">
              <option value="asc">Ascending</option>
              <option value="desc">Descending</option>
            </select>
          </div>
        </div>
      </div>
    </div>

    <!-- Divider -->
    <hr class="my-3" v-if="validFilters.length > 0" />

    <!-- Filter Fields Section -->
    <div class="row g-3 mb-3" v-if="validFilters.length > 0">
      <div v-for="f in validFilters" :key="f.key" class="col-12 col-sm-6 col-lg-3">
        <label class="form-label small mb-2 fw-semibold">{{ f.label }}</label>
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

    <!-- Action Buttons -->
    <div class="d-flex justify-content-end gap-2 mt-4">
      <button class="btn btn-sm btn-outline-secondary px-4" type="button" @click="clearFilters">Clear</button>
      <button class="btn btn-sm btn-primary px-4" type="button" @click="applyFilters">Apply</button>
    </div>
  </div>
</template>

<style scoped>
.base-filter {
  background-color: #f8f9fa;
  border: 1px solid #dee2e6;
}

.form-label.fw-semibold {
  color: #495057;
}

.btn {
  min-width: 80px;
}
</style>
