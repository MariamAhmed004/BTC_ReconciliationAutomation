<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import BaseTable from '../components/common/BaseTable.vue'

const route = useRoute()
const id = route.params.id

const details = ref({
  status: 'N/A',
  runAt: null,
  relatedConfigurations: '',
  triggeredBy: '',
  recordsProcessed: 0,
  totalDiscrepancies: 0,
  addedToRowB: 0,
  deactivatedInRowB: 0,
  matchedPackages: 0,
  errorMessage: null
})

const files = ref([])
const logs = ref([])
const isLoading = ref(false)
const error = ref(null)

function formatDate(d) {
  if (!d) return 'N/A'
  try { 
    return new Date(d).toLocaleString('en-US', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    })
  } catch { 
    return String(d) 
  }
}

// Computed property for status display
const statusDisplay = computed(() => {
  const status = details.value.status?.toUpperCase()
  if (status === 'COMPLETED') return { text: 'Success', class: 'text-success' }
  if (status === 'FAILED') return { text: 'Failed', class: 'text-danger' }
  return { text: status || 'N/A', class: 'text-muted' }
})

async function loadDetails() {
  isLoading.value = true
  error.value = null

  try {
    const res = await fetch(`/api/Reconciliation/run/${id}`)
    if (!res.ok) {
      throw new Error(`HTTP error! status: ${res.status}`)
    }

    const data = await res.json()

    // Map basic details
    details.value.status = data.status || 'N/A'
    details.value.runAt = formatDate(data.runDate)
    details.value.triggeredBy = data.triggeredBy || 'Automatic'
    details.value.errorMessage = data.errorMessage

    // Map summary data
    details.value.recordsProcessed = data.recordsProcessed || 0
    details.value.totalDiscrepancies = data.totalDiscrepancies || 0
    details.value.matchedPackages = data.mismatchCount || 0
    details.value.addedToRowB = data.missingInBilling || 0
    details.value.deactivatedInRowB = 0 // Removed from backend

    // Map configuration
    if (data.configuration) {
      const config = data.configuration
      details.value.relatedConfigurations = `Schedule: ${config.scheduleExpression || 'N/A'}, Email: ${config.emailRecipients || 'N/A'}`
    } else {
      details.value.relatedConfigurations = 'No configuration'
    }

    // Map files
    files.value = (data.files || []).map(f => ({
      name: f.fileName || 'Unknown File',
      url: f.filePath || '#',
      fileId: f.fileId,
      createdAt: formatDate(f.createdAt),
      fileType: f.fileType || 'N/A'
    }))

    // Map logs
    logs.value = (data.logs || []).map(l => ({
      id: l.logId || '',
      message: l.message || 'No message',
      createdAt: formatDate(l.createdAt),
      logLevel: l.logLevel || 'INFO',
      logLevelId: l.logLevelId
    }))

  } catch (err) {
    error.value = err.message
    console.error('Failed to load run details', err)
  } finally {
    isLoading.value = false
  }
}

onMounted(() => { loadDetails() })

const logsColumns = [
  { key: 'id', title: 'Log ID', width: '100px' },
  { key: 'logLevel', title: 'Level', width: '100px' },
  { key: 'message', title: 'Message' },
  { key: 'createdAt', title: 'Created At', width: '180px' }
]
</script>

<template>
  <div class="container pt-4">
    <div class="d-flex align-items-center mb-3">
      <div class="me-2"><span class="fs-4">&#x2630;</span></div>
      <h3 class="mb-0">Reconciliation Execution Details</h3>
    </div>

    <p class="text-muted">Following are the details of the reconciliation process : <br/><small>Click on a file icon to download</small></p>

    <!-- Loading state -->
    <div v-if="isLoading" class="text-center my-5">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
      <p class="mt-2">Loading run details...</p>
    </div>

    <!-- Error state -->
    <div v-if="error" class="alert alert-danger" role="alert">
      <strong>Error:</strong> {{ error }}
    </div>

    <!-- Content -->
    <div v-if="!isLoading && !error">
      <div class="mb-3 text-center">
        <div>
          <span :class="['fw-semibold', statusDisplay.class]">{{ statusDisplay.text }}</span>
          <span v-if="details.errorMessage" class="mx-3 text-danger">→ ERROR: {{ details.errorMessage }}</span>
        </div>
      </div>

      <div class="text-center mb-3">
        <div class="fw-bold">Run at: {{ details.runAt }}</div>
        <div class="fw-semibold mt-2">Related Configurations</div>
        <div class="mb-1">{{ details.relatedConfigurations }}</div>
        <div class="fw-semibold mt-2">Triggered by</div>
        <div class="mb-3">{{ details.triggeredBy }}</div>
      </div>

      <hr />
      <h5 class="text-center">Process Summary</h5>

      <div class="row text-center my-3">
        <div class="col-6 col-md-4 mb-3">
          <div class="fw-semibold">Records Processed</div>
          <div class="fs-5">{{ details.recordsProcessed }}</div>
        </div>
        <div class="col-6 col-md-4 mb-3">
          <div class="fw-semibold">Total Discrepancies</div>
          <div class="fs-5 text-danger">{{ details.totalDiscrepancies }}</div>
        </div>
        <div class="col-6 col-md-4 mb-3">
          <div class="fw-semibold">Matched Packages</div>
          <div class="fs-5">{{ details.matchedPackages }}</div>
        </div>

        <div class="col-6 col-md-4 mb-3">
          <div class="fw-semibold">Added to ROWB</div>
          <div class="fs-5">{{ details.addedToRowB }}</div>
        </div>
        <div class="col-6 col-md-4 mb-3">
          <div class="fw-semibold">Deactivated in ROWB</div>
          <div class="fs-5">{{ details.deactivatedInRowB }}</div>
        </div>
      </div>

      <hr />
      <h5 class="text-center">Related Files</h5>
      <div v-if="files.length === 0" class="text-center text-muted my-3">
        No files generated for this run
      </div>
      <div v-else class="d-flex justify-content-center align-items-start gap-4 my-3 flex-wrap">
        <div v-for="(f, idx) in files" :key="idx" class="text-center file-block">
          <a :href="f.url" class="text-decoration-none text-dark" :title="`Created: ${f.createdAt}`">
            <div class="file-icon mb-2">XLSX</div>
            <div class="small">{{ f.name }}</div>
            <div v-if="f.deliveryMethod" class="small text-muted">{{ f.deliveryMethod }}</div>
            <div v-if="f.emailStatus" class="small text-muted">{{ f.emailStatus }}</div>
          </a>
        </div>
      </div>

      <hr />
      <h5 class="text-center">Related System Logs</h5>

      <div v-if="logs.length === 0" class="text-center text-muted my-3">
        No logs available for this run
      </div>
      <div v-else class="mt-3">
        <BaseTable :columns="logsColumns" :items="logs" :showSearch="false" :showPagination="false" />
      </div>
    </div>
  </div>
</template>

<style scoped>
.file-icon {
  width: 96px;
  height: 96px;
  border: 2px solid #333;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  background: #fff;
}
.file-block a:hover .file-icon { transform: translateY(-4px); }
</style>
