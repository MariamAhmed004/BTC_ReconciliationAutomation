<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import BaseTable from '../components/common/BaseTable.vue'
import BaseFilter from '../components/common/BaseFilter.vue'
import BaseModal from '../components/common/BaseModal.vue'

const columns = [
  { key: 'id', title: 'ID', width: '80px' },
  { key: 'message', title: 'Message' },
  { key: 'level', title: 'Level', width: '120px' },
  { key: 'run', title: 'Run', width: '140px' },
  { key: 'createdAt', title: 'Date', width: '160px', render: 'timestamp' }
]

const items = ref([])
const showDeleteModal = ref(false)
const deleteMode = ref('range') // 'range' or 'all'
const rangeMode = ref('days') // 'days' or 'dates'
const deleteFrom = ref('')
const deleteTo = ref('')
const deleteDays = ref(30)
const confirmDeleting = ref(false)

function formatDate(d) {
  if (!d) return ''
  try { return new Date(d).toLocaleString() } catch { return String(d) }
}

async function loadLogs() {
  try {
    const res = await fetch('/api/Log/latest')
    if (!res.ok) {
      console.error('Failed to fetch logs', res.status)
      return
    }
    const ct = res.headers.get('content-type') || ''
    if (!ct.includes('application/json')) {
      console.error('Expected JSON but got', ct)
      return
    }
    const data = await res.json()
    // API model uses uppercase properties; map to table shape
    items.value = (data || []).map(l => {
      // Resolve log level name from included navigation when available
      const levelName = l.loG_LEVEL?.loG_LEVEL1 ?? l.LOG_LEVEL?.LOG_LEVEL1 ?? l.loG_LEVEL?.LOG_LEVEL_NAME ?? l.loG_LEVEL?.log_level_name ?? (l.LOG_LEVEL_ID ?? l.loG_LEVEL_ID ?? '')
      // Resolve related run info (id and triggeredBy) if included
      const runObj = l.RUN ?? l.run ?? l.Run ?? null
      const runId = runObj?.RUN_ID ?? runObj?.ruN_ID ?? runObj?.id ?? null
      const runTriggeredBy = runObj?.TRIGGERED_BY ?? runObj?.triggereD_BY ?? runObj?.triggeredBy ?? ''

      return {
        id: l.loG_ID ?? l.LOG_ID ?? l.id,
        message: l.loG_MESSAGE ?? l.LOG_MESSAGE ?? l.message,
        level: levelName,
        // For the 'run' column we keep the run id so the slot can render a button
        runId: runId,
        runTriggeredBy: runTriggeredBy,
        createdAt: formatDate(l.createD_AT ?? l.CREATED_AT ?? l.createdAt)
      }
    })
  } catch (err) {
    console.error('Error loading logs', err)
  }
}

onMounted(() => { loadLogs() })

const router = useRouter()

const filters = [
  { key: 'level', label: 'Level', type: 'select', options: [
    { value: '', label: 'Any' },
    { value: 'INFO', label: 'Info' },
    { value: 'WARNING', label: 'Warning' },
    { value: 'ERROR', label: 'Error' }
  ], default: '' },
  { key: 'runTriggeredBy', label: 'Run', type: 'text', placeholder: 'Triggered by' },
  { key: 'createdAt', label: 'Date', type: 'date' }
]

function onRowClick(item) {
  // replace with navigation or details panel as needed
  console.log('row clicked', item)
}

async function performDeleteRange() {
  // build payload
  const payload = {}
  if (rangeMode.value === 'days' && deleteDays.value) {
    payload.days = Number(deleteDays.value)
  } else if (rangeMode.value === 'dates') {
    if (deleteFrom.value) payload.from = new Date(deleteFrom.value).toISOString()
    if (deleteTo.value) payload.to = new Date(deleteTo.value).toISOString()
  }

  try {
    const ok = confirm('Are you sure you want to delete the selected logs? This action cannot be undone.')
    if (!ok) return
    const res = await fetch('/api/Log/delete/range', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) })
    if (!res.ok) throw new Error('Delete request failed')
    const data = await res.json()
    alert(`Deleted ${data.deleted} log records`)
    showDeleteModal.value = false
    await loadLogs()
  } catch (err) {
    console.error('Delete range failed', err)
    alert('Failed to delete logs')
  }
}

async function performDeleteAll() {
  try {
    const ok = confirm('Are you sure you want to delete ALL log records? This action cannot be undone.')
    if (!ok) return
    const res = await fetch('/api/Log/delete/all', { method: 'POST' })
    if (!res.ok) throw new Error('Delete all request failed')
    const data = await res.json()
    alert(`Deleted ${data.deleted} log records`)
    showDeleteModal.value = false
    await loadLogs()
  } catch (err) {
    console.error('Delete all failed', err)
    alert('Failed to delete logs')
  }
}

const modalButtons = computed(() => {
  if (deleteMode.value === 'all') {
    return [
      { text: 'Cancel', variant: 'secondary', action: 'close' },
      { text: 'Delete All', variant: 'danger', action: 'deleteAll' }
    ]
  }
  return [
    { text: 'Cancel', variant: 'secondary', action: 'close' },
    { text: 'Delete Selected', variant: 'danger', action: 'deleteRange' }
  ]
})

function handleModalAction(action) {
  if (action === 'deleteAll') {
    performDeleteAll()
  } else if (action === 'deleteRange') {
    performDeleteRange()
  }
}
</script>

<template>
  <div class="container pt-4">
    <h3>Logs</h3>
    <div class="mb-3 d-flex justify-content-end">
      <button class="btn btn-sm btn-danger" @click="showDeleteModal = true">Request Log Clearance</button>
    </div>

    <BaseModal
      :show="showDeleteModal"
      title="Delete Logs"
      :buttons="modalButtons"
      size="lg"
      @close="showDeleteModal = false"
      @action="handleModalAction"
    >
      <div class="delete-modal-content">
        <p class="text-muted small mb-3">
          Choose how you want to delete log records. You can either delete all logs or delete logs by specifying a time period.
        </p>

        <div class="mb-3">
          <label class="form-label fw-semibold">Deletion Mode</label>
          <select v-model="deleteMode" class="form-select">
            <option value="range">Delete by time period</option>
            <option value="all">Delete all records</option>
          </select>
        </div>

        <div v-if="deleteMode === 'range'" class="range-options">
          <div class="mb-3">
            <label class="form-label fw-semibold">Select Period Type</label>
            <div class="form-check">
              <input 
                class="form-check-input" 
                type="radio" 
                name="rangeMode" 
                id="rangeModeDate" 
                value="dates"
                v-model="rangeMode"
              />
              <label class="form-check-label" for="rangeModeDate">
                Specify date range
              </label>
            </div>
            <div class="form-check">
              <input 
                class="form-check-input" 
                type="radio" 
                name="rangeMode" 
                id="rangeModeDays" 
                value="days"
                v-model="rangeMode"
              />
              <label class="form-check-label" for="rangeModeDays">
                Delete logs older than X days
              </label>
            </div>
          </div>

          <div v-if="rangeMode === 'dates'" class="date-inputs">
            <div class="mb-2">
              <label class="form-label">From (date)</label>
              <input type="date" v-model="deleteFrom" class="form-control" />
            </div>
            <div class="mb-2">
              <label class="form-label">To (date)</label>
              <input type="date" v-model="deleteTo" class="form-control" />
            </div>
          </div>

          <div v-if="rangeMode === 'days'" class="days-input">
            <label class="form-label">Delete records older than (days)</label>
            <input 
              type="number" 
              v-model.number="deleteDays" 
              class="form-control" 
              min="1" 
              placeholder="Enter number of days"
            />
          </div>
        </div>

        <div v-if="deleteMode === 'all'" class="alert alert-danger" role="alert">
          <i class="bi bi-exclamation-triangle-fill me-2"></i>
          <strong>Warning:</strong> This will permanently delete all log records from the database. This action cannot be undone.
        </div>
      </div>
    </BaseModal>

    <BaseTable
      :columns="columns"
      :items="items"
      :showSearch="true"
      :showPagination="true"
      :pageSizeOptions="[5,10,25]"
      :filters="filters"
      :rowClickable="true"
      @row-click="onRowClick"
    >
      <template #level="{ item }">
        <span 
          v-if="item.level" 
          :class="[
            'badge text-white',
            item.level.toUpperCase() === 'ERROR' ? 'bg-danger' :
            item.level.toUpperCase() === 'WARNING' ? 'bg-warning' :
            item.level.toUpperCase() === 'INFO' ? 'bg-info' :
            'bg-secondary'
          ]"
        >
          {{ item.level }}
        </span>
        <span v-else class="text-muted">-</span>
      </template>

      <template #run="{ item }">
        <button 
          v-if="item.runId" 
          class="btn btn-sm btn-outline-primary" 
          @click.stop="router.push({ name: 'LogDetails', params: { id: item.runId } })"
        >
          <i class="bi bi-arrow-right-circle me-1"></i>
          Run Details
        </button>
        <span v-else class="text-muted small">N/A</span>
      </template>
    </BaseTable>
  </div>
</template>

<style scoped>
.delete-modal-content {
  max-height: 60vh;
  overflow-y: auto;
}

.range-options {
  background-color: #f8f9fa;
  padding: 1rem;
  border-radius: 0.375rem;
  border: 1px solid #dee2e6;
}

.date-inputs,
.days-input {
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #dee2e6;
}

.form-check {
  padding: 0.5rem 0;
}

.form-check-input:checked {
  background-color: #0d6efd;
  border-color: #0d6efd;
}
</style>
