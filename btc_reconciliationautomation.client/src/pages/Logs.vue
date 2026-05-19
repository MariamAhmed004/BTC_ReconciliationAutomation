<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import PageHeader from '@/components/common/PageHeader.vue'
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

const levelOptions = ref([{ value: '', label: 'Any' }])

// Confirmation modal
const showConfirmModal = ref(false)
const confirmModalMessage = ref('')
const pendingAction = ref(null)

// Feedback modal
const showFeedbackModal = ref(false)
const feedbackModalTitle = ref('')
const feedbackModalMessage = ref('')
const feedbackModalVariant = ref('success') // 'success' or 'danger'

function showConfirm(message, action) {
  confirmModalMessage.value = message
  pendingAction.value = action
  showConfirmModal.value = true
}

function handleConfirm() {
  showConfirmModal.value = false
  if (pendingAction.value) pendingAction.value()
  pendingAction.value = null
}

function showFeedback(title, message, variant = 'success') {
  feedbackModalTitle.value = title
  feedbackModalMessage.value = message
  feedbackModalVariant.value = variant
  showFeedbackModal.value = true
}

function formatDate(d) {
  if (!d) return ''
  try { return new Date(d).toLocaleString() } catch { return String(d) }
}

async function loadLevels() {
  try {
    const res = await fetch('/api/Log/levels')
    if (!res.ok) return
    const data = await res.json()
    levelOptions.value = [
      { value: '', label: 'Any' },
      ...data.map(l => ({ value: l.loG_LEVEL1 ?? l.LOG_LEVEL1, label: l.loG_LEVEL1 ?? l.LOG_LEVEL1 }))
    ]
  } catch (err) {
    console.error('Error loading log levels', err)
  }
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

onMounted(() => { loadLevels(); loadLogs() })

const router = useRouter()

const filters = computed(() => [
  { key: 'level', label: 'Level', type: 'select', options: levelOptions.value, default: '' },
  { key: 'runTriggeredBy', label: 'Run', type: 'text', placeholder: 'Triggered by' },
  { key: 'createdAt', label: 'Date', type: 'date' }
])

function onRowClick(item) {
  // replace with navigation or details panel as needed
  console.log('row clicked', item)
}

async function performDeleteRange() {
  const payload = {}
  if (rangeMode.value === 'days' && deleteDays.value) {
    payload.days = Number(deleteDays.value)
  } else if (rangeMode.value === 'dates') {
    if (deleteFrom.value) payload.from = new Date(deleteFrom.value).toISOString()
    if (deleteTo.value) payload.to = new Date(deleteTo.value).toISOString()
  }

  const message = rangeMode.value === 'days'
    ? `Are you sure you want to delete all logs older than ${deleteDays.value} day(s)? This action cannot be undone.`
    : `Are you sure you want to delete all logs within the selected date range? This action cannot be undone.`

  showConfirm(message, async () => {
    try {
      const res = await fetch('/api/Log/delete/range', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) })
      if (!res.ok) throw new Error('Delete request failed')
      const data = await res.json()
      showDeleteModal.value = false
      showFeedback('Logs Cleared', `Successfully deleted ${data.deleted} log record(s).`, 'success')
      await loadLogs()
    } catch (err) {
      console.error('Delete range failed', err)
      showFeedback('Error', 'Failed to delete logs. Please try again.', 'danger')
    }
  })
}

async function performDeleteAll() {
  showConfirm('Are you sure you want to delete ALL log records? This action cannot be undone.', async () => {
    try {
      const res = await fetch('/api/Log/delete/all', { method: 'POST' })
      if (!res.ok) throw new Error('Delete all request failed')
      const data = await res.json()
      showDeleteModal.value = false
      showFeedback('Logs Cleared', `Successfully deleted all ${data.deleted} log record(s).`, 'success')
      await loadLogs()
    } catch (err) {
      console.error('Delete all failed', err)
      showFeedback('Error', 'Failed to delete logs. Please try again.', 'danger')
    }
  })
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
    <PageHeader
      title="Audit Logs"
      subtitle="Following are the system audit logs recorded :"
      instruction="Use Request Log Clearance to permanently remove audit log records (by date range or all). This helps manage log volume and storage."
    >
      <template #icon>
        <i class="bi bi-file-earmark-text" style="font-size: 2rem; color: #6c757d;"></i>
      </template>
    </PageHeader>

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
          Choose how you want to clear log records. Deleted records are permanently removed and cannot be recovered.
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
                Specify a date range 
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
                Older than X days
              </label>
            </div>
          </div>

          <div v-if="rangeMode === 'dates'" class="date-inputs">
            <div class="mb-2">
              <label class="form-label">From</label>
              <input type="date" v-model="deleteFrom" class="form-control" />
            </div>
            <div class="mb-2">
              <label class="form-label">To</label>
              <input type="date" v-model="deleteTo" class="form-control" />
            </div>
            <p class="text-muted small mt-1">All logs created between these two dates will be permanently deleted.</p>
          </div>

          <div v-if="rangeMode === 'days'" class="days-input">
            <label class="form-label">Delete logs older than (days)</label>
            <input 
              type="number" 
              v-model.number="deleteDays" 
              class="form-control" 
              min="1" 
              placeholder="Enter number of days"
            />
            <p class="text-muted small mt-1">
              Any log record created more than <strong>{{ deleteDays || '?' }}</strong> day(s) ago will be permanently deleted.
            </p>
          </div>
        </div>

        <div v-if="deleteMode === 'all'" class="alert alert-danger" role="alert">
          <i class="bi bi-exclamation-triangle-fill me-2"></i>
          <strong>Warning:</strong> This will permanently delete <strong>every single log record</strong> from the database, regardless of age or type. This action cannot be undone.
        </div>
      </div>
    </BaseModal>

    <!-- Confirmation modal -->
    <BaseModal
      :show="showConfirmModal"
      title="Confirm Deletion"
      :buttons="[
        { text: 'Cancel', variant: 'secondary', action: 'close' },
        { text: 'Yes, Delete', variant: 'danger', action: 'confirm' }
      ]"
      @close="showConfirmModal = false"
      @action="(a) => { if (a === 'confirm') handleConfirm() }"
    >
      <div class="d-flex align-items-start gap-3">
        <i class="bi bi-exclamation-triangle-fill text-danger fs-4 flex-shrink-0"></i>
        <p class="mb-0">{{ confirmModalMessage }}</p>
      </div>
    </BaseModal>

    <!-- Feedback modal -->
    <BaseModal
      :show="showFeedbackModal"
      :title="feedbackModalTitle"
      :buttons="[{ text: 'Close', variant: 'secondary', action: 'close' }]"
      @close="showFeedbackModal = false"
    >
      <div class="d-flex align-items-start gap-3">
        <i
          :class="[
            'bi fs-4 flex-shrink-0',
            feedbackModalVariant === 'success' ? 'bi-check-circle-fill text-success' : 'bi-x-circle-fill text-danger'
          ]"
        ></i>
        <p class="mb-0">{{ feedbackModalMessage }}</p>
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
