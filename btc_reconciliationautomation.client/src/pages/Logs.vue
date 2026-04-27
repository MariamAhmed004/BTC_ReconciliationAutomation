<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import BaseTable from '../components/common/BaseTable.vue'
import BaseFilter from '../components/common/BaseFilter.vue'
import { ref as ref2 } from 'vue'

const columns = [
  { key: 'id', title: 'ID', width: '80px' },
  { key: 'message', title: 'Message' },
  { key: 'level', title: 'Level', width: '120px' },
  { key: 'run', title: 'Run', width: '140px' },
  { key: 'createdAt', title: 'Date', width: '160px', render: 'timestamp' }
]

const items = ref([])
const showDeleteModal = ref2(false)
const deleteMode = ref2('range') // 'range' or 'all'
const deleteFrom = ref2('')
const deleteTo = ref2('')
const deleteDays = ref2(30)
const confirmDeleting = ref2(false)

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
      const levelName = l.LOG_LEVEL?.loG_LEVEL1 ?? l.LOG_LEVEL?.LOG_LEVEL1 ?? l.loG_LEVEL?.LOG_LEVEL_NAME ?? l.loG_LEVEL?.log_level_name ?? (l.LOG_LEVEL_ID ?? l.loG_LEVEL_ID ?? '')
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
  if (deleteDays && deleteMode.value === 'range' && (!deleteFrom.value && !deleteTo.value)) {
    payload.days = Number(deleteDays)
  } else {
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
    await loadLogs()
  } catch (err) {
    console.error('Delete all failed', err)
    alert('Failed to delete logs')
  }
}
</script>

<template>
  <div class="container pt-4">
    <h3>Logs</h3>
    <div class="mb-3 d-flex justify-content-end">
      <button class="btn btn-sm btn-danger" @click="showDeleteModal = true">Delete Logs</button>
    </div>

    <div v-if="showDeleteModal" class="modal-backdrop d-flex align-items-center justify-content-center">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Delete Logs</h5>
            <button type="button" class="btn-close" @click="showDeleteModal = false"></button>
          </div>
          <div class="modal-body">
            <div class="mb-3">
              <label class="form-label">Mode</label>
              <select v-model="deleteMode" class="form-select">
                <option value="range">Delete by period / days</option>
                <option value="all">Delete all records</option>
              </select>
            </div>

            <div v-if="deleteMode === 'range'">
              <div class="mb-2">
                <label class="form-label">From (date)</label>
                <input type="date" v-model="deleteFrom" class="form-control" />
              </div>
              <div class="mb-2">
                <label class="form-label">To (date)</label>
                <input type="date" v-model="deleteTo" class="form-control" />
              </div>
              <div class="mb-2">
                <label class="form-label">Or delete records older than (days)</label>
                <input type="number" v-model.number="deleteDays" class="form-control" min="1" />
              </div>
            </div>

            <div v-if="deleteMode === 'all'" class="text-danger">
              This will delete all log records. A confirmation will be required.
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn btn-secondary" @click="showDeleteModal = false">Cancel</button>
            <button v-if="deleteMode === 'range'" class="btn btn-danger" @click="performDeleteRange">Delete Selected</button>
            <button v-else class="btn btn-danger" @click="performDeleteAll">Delete All</button>
          </div>
        </div>
      </div>
    </div>

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
      <template #run="{ item }">
        <button class="btn btn-sm btn-outline-primary" :disabled="!item.runId" @click.stop="router.push({ name: 'LogDetails', params: { id: item.runId } })">
          <i class="bi bi-arrow-right-circle"></i>
          <span v-if="item.runTriggeredBy" class="ms-1 small">{{ item.runTriggeredBy }}</span>
          <span v-else class="ms-1 small">View</span>
        </button>
      </template>
    </BaseTable>
  </div>
</template>

<style scoped></style>
