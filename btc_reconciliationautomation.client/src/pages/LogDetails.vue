<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import PageHeader from '@/components/common/PageHeader.vue'
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
    <PageHeader
      title="Reconciliation Execution Details"
      subtitle="Following are the details of the reconciliation process :"
      instruction="Click on a file icon to download"
    >
      <template #icon>
        <i class="bi bi-list-check" style="font-size: 2rem; color: #6c757d;"></i>
      </template>
    </PageHeader>

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
      <div class="run-details card border-0 shadow-sm mb-4">
        <div class="card-body">
          <div class="d-flex align-items-start justify-content-between flex-wrap gap-2">
            <div class="d-flex align-items-center gap-2">
              <span class="text-muted small">Status</span>
              <span :class="['badge', statusDisplay.class === 'text-success' ? 'bg-success' : statusDisplay.class === 'text-danger' ? 'bg-danger' : 'bg-secondary']">
                {{ statusDisplay.text }}
              </span>
            </div>
            <div v-if="details.errorMessage" class="text-danger small">
              <strong>Error:</strong> {{ details.errorMessage }}
            </div>
          </div>

          <div class="detail-grid mt-3">
            <div class="detail-row">
              <div class="detail-label">Run at</div>
              <div class="detail-value">{{ details.runAt }}</div>
            </div>
            <div class="detail-row">
              <div class="detail-label">Triggered by</div>
              <div class="detail-value">{{ details.triggeredBy }}</div>
            </div>
            <div class="detail-row">
              <div class="detail-label">Related configuration</div>
              <div class="detail-value">{{ details.relatedConfigurations }}</div>
            </div>
          </div>
        </div>
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
.run-details {
  border-radius: 0.5rem;
}

.detail-grid {
  display: grid;
  gap: 0.75rem;
}

.detail-row {
  display: flex;
  gap: 0.75rem;
  align-items: flex-start;
}

.detail-label {
  min-width: 160px;
  color: #6c757d;
  font-weight: 600;
  font-size: 0.875rem;
}

.detail-value {
  flex: 1;
  min-width: 0;
  overflow-wrap: anywhere;
}

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
