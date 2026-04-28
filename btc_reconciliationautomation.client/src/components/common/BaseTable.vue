<script setup>
import { ref, computed, watch } from 'vue'
import BaseFilter from './BaseFilter.vue'

const props = defineProps({
  columns: { type: Array, default: () => [] },
  items: { type: Array, default: () => [] },
  tableClass: { type: String, default: '' },
  showSearch: { type: Boolean, default: false },
  showPagination: { type: Boolean, default: false },
  filters: { type: Array, default: () => [] },
  pageSizeOptions: { type: Array, default: () => [10, 25, 50] },
  rowClickable: { type: Boolean, default: false }
})

const emit = defineEmits(['row-click'])

const searchQuery = ref('')
const pageSize = ref(props.pageSizeOptions[0] || 10)
const currentPage = ref(1)
const showFilter = ref(false)
const activeFilters = ref({})
const sortKey = ref('')
const sortDir = ref('asc')

// Helper function to get badge class based on status
function getStatusBadgeClass(status) {
  if (!status) return 'badge bg-secondary'

  const statusUpper = status.toUpperCase()
  if (statusUpper === 'COMPLETED') return 'badge bg-success'
  if (statusUpper === 'FAILED') return 'badge bg-danger'
  return 'badge bg-secondary'
}

function initFilters() {
  activeFilters.value = {}
  props.filters.forEach(f => { activeFilters.value[f.key] = f.default ?? '' })
  sortKey.value = ''
  sortDir.value = 'asc'
}

watch(() => props.filters, initFilters, { immediate: true })

watch(() => props.items, () => {
  currentPage.value = 1
})

const filteredItems = computed(() => {
  let result = props.items || []

  // text search across columns
  if (props.showSearch && searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    result = result.filter(item =>
      props.columns.some(col => {
        const v = item[col.key]
        return v != null && String(v).toLowerCase().includes(q)
      })
    )
  }

  // apply active filters
  if (props.filters && props.filters.length) {
    result = result.filter(item => {
      return props.filters.every(f => {
        const val = activeFilters.value[f.key]
        if (val === undefined || val === null || val === '') return true
        const itemVal = item[f.key]

        if (f.type === 'select') {
          // Case-insensitive comparison for select filters
          return String(itemVal).toUpperCase() === String(val).toUpperCase()
        }

        if (f.type === 'date') {
          // For date filtering, extract date portion from formatted timestamp
          if (!itemVal) return false
          try {
            // Parse the item value as a date
            const itemDate = new Date(itemVal)
            // Parse the filter value (input date format: YYYY-MM-DD)
            const filterDate = new Date(val)
            // Compare only the date parts (ignore time)
            return itemDate.toDateString() === filterDate.toDateString()
          } catch (e) {
            // Fallback: try string contains for malformed dates
            return String(itemVal).includes(String(val))
          }
        }

        // default: text contains
        return itemVal != null && String(itemVal).toLowerCase().includes(String(val).toLowerCase())
      })
    })
  }

  // apply sorting if requested
  if (sortKey.value) {
    const key = sortKey.value
    const dir = sortDir.value === 'desc' ? -1 : 1
    const compare = (a, b) => {
      const va = a[key]
      const vb = b[key]
      if (va == null && vb == null) return 0
      if (va == null) return -1
      if (vb == null) return 1
      const na = Number(va)
      const nb = Number(vb)
      if (!isNaN(na) && !isNaN(nb)) return (na - nb) * dir
      const da = Date.parse(va)
      const db = Date.parse(vb)
      if (!isNaN(da) && !isNaN(db)) return (da - db) * dir
      return String(va).localeCompare(String(vb)) * dir
    }
    // create a shallow copy before sorting to avoid mutating props.items
    result = [...result].sort(compare)
  }

  return result
})

const totalItems = computed(() => filteredItems.value.length)
const totalPages = computed(() => Math.max(1, Math.ceil(totalItems.value / pageSize.value)))

const paginatedItems = computed(() => {
  if (!props.showPagination) return filteredItems.value
  const start = (currentPage.value - 1) * pageSize.value
  return filteredItems.value.slice(start, start + pageSize.value)
})

function handleRowClick(item) {
  if (!props.rowClickable) return
  emit('row-click', item)
}

function goToPage(n) {
  if (n < 1) n = 1
  if (n > totalPages.value) n = totalPages.value
  currentPage.value = n
}

function prevPage() { goToPage(currentPage.value - 1) }
function nextPage() { goToPage(currentPage.value + 1) }
function firstPage() { goToPage(1) }
function lastPage() { goToPage(totalPages.value) }

// reset page when pageSize changes
watch(pageSize, () => { currentPage.value = 1 })

function handleApplyFilters(payload) {
  const p = { ...payload }
  if (p.sort) {
    sortKey.value = p.sort.key || ''
    sortDir.value = p.sort.dir || 'asc'
    delete p.sort
  }
  activeFilters.value = p
  showFilter.value = false
}

function handleClearFilters() {
  initFilters()
}

function handleCloseFilter() {
  showFilter.value = false
}
</script>

<template>
  <div class="base-table">
    <div class="d-flex justify-content-between align-items-center mb-2" v-if="props.showSearch || props.showPagination">
      <div class="d-flex align-items-center">
        <label class="me-2 mb-0">Show</label>
        <select v-if="props.showPagination" v-model.number="pageSize" class="form-select form-select-sm me-2" style="width:80px">
          <option v-for="opt in props.pageSizeOptions" :key="opt" :value="opt">{{ opt }}</option>
        </select>
        <label class="mb-0">entries</label>
      </div>

      <div v-if="props.showSearch" class="ms-auto d-flex align-items-center">
        <div class="input-group" style="width:260px">
          <span class="input-group-text bg-white"><i class="bi bi-search"></i></span>
          <input v-model="searchQuery" type="search" class="form-control form-control-sm" placeholder="Search..." />
        </div>

        <div v-if="props.filters && props.filters.length" class="ms-2">
          <button class="btn btn-sm btn-outline-secondary d-flex align-items-center" type="button" @click="showFilter = !showFilter">
            <i class="bi bi-funnel-fill me-1"></i>
            Filter
          </button>
        </div>
      </div>
    </div>

    <div v-if="showFilter" class="mt-2">
      <BaseFilter 
        :filters="props.filters" 
        @apply="handleApplyFilters" 
        @clear="handleClearFilters" 
        @close="handleCloseFilter"
      />
    </div>

    <div class="table-responsive">
      <table :class="['table align-middle mb-0', props.tableClass, { 'table-hover': props.rowClickable }]">
        <thead>
          <tr class="table-header table-dark">
            <th v-for="col in props.columns" :key="col.key" :style="{ width: col.width || 'auto' }">
              {{ col.title }}
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(item, rowIndex) in paginatedItems" :key="item.id ?? rowIndex" :class="{ 'table-row-clickable': props.rowClickable }" :tabindex="props.rowClickable ? 0 : null" @click="handleRowClick(item)" @keydown.enter.prevent="handleRowClick(item)">
            <td v-for="col in props.columns" :key="col.key">
              <slot :name="col.key" :item="item">
                <template v-if="col.render === 'timestamp'">
                  <div class="timestamp">
                    <div class="time">{{ item[col.key] }}</div>
                  </div>
                </template>
                <template v-else-if="col.render === 'status'">
                  <span v-if="item[col.key] === 'ok' || item[col.key] === true" class="status-badge success">✓</span>
                  <span v-else class="status-badge danger">✕</span>
                </template>
                <template v-else-if="col.render === 'statusBadge'">
                  <span :class="getStatusBadgeClass(item[col.key])">
                    {{ item[col.key] || 'N/A' }}
                  </span>
                </template>
                <template v-else>
                  {{ item[col.key] }}
                </template>
              </slot>
            </td>
          </tr>
          <tr v-if="paginatedItems.length === 0">
            <td :colspan="props.columns.length" class="text-center text-muted">No records found</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="d-flex justify-content-between align-items-center mt-3" v-if="props.showPagination">
      <div>
        <small>Showing {{ ((currentPage-1)*pageSize)+1 }} to {{ Math.min(currentPage*pageSize, totalItems) }} of {{ totalItems }} entries</small>
      </div>

      <nav aria-label="Table pagination">
        <ul class="pagination mb-0">
          <li class="page-item" :class="{ disabled: currentPage===1 }"><button class="page-link" @click="firstPage">««</button></li>
          <li class="page-item" :class="{ disabled: currentPage===1 }"><button class="page-link" @click="prevPage">«</button></li>

          <li v-for="p in Array.from({ length: totalPages }, (_, i) => i + 1)" :key="p" class="page-item" :class="{ active: p===currentPage }">
            <button class="page-link" @click="goToPage(p)">{{ p }}</button>
          </li>

          <li class="page-item" :class="{ disabled: currentPage===totalPages }"><button class="page-link" @click="nextPage">»</button></li>
          <li class="page-item" :class="{ disabled: currentPage===totalPages }"><button class="page-link" @click="lastPage">»»</button></li>
        </ul>
      </nav>
    </div>
  </div>
</template>

<style scoped>
.table-header {
  font-weight: 700;
}

.status-badge {
  display: inline-flex;
  width: 28px;
  height: 28px;
  border-radius: 50%;
  align-items: center;
  justify-content: center;
  color: #fff;
  font-weight: 700;
}
.status-badge.success { background: #28a745; }
.status-badge.danger { background: #dc3545; }

.timestamp .time {
  text-decoration: underline;
  color: #000;
}

.base-table table {
  border: 1px solid #cfcfcf;
}

.base-table td, .base-table th {
  vertical-align: middle;
  padding: 0.9rem 1rem;
}

.pagination .page-link { cursor: pointer }
.pagination .page-item.disabled .page-link { pointer-events: none; opacity: 0.6 }
.pagination .page-item.active .page-link { background-color: #adb5bd; border-color: #adb5bd }

.table-row-clickable { cursor: pointer }
.table-row-clickable:hover { background: rgba(0,0,0,0.02) }
</style>
