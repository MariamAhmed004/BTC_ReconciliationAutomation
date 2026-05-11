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
  triggeredBy: '',
  recordsProcessed: 0,
  totalDiscrepancies: 0,
  addedToRowB: 0,
  missingInCustomer: 0,
  matchedPackages: 0,
  errorMessage: null
})

const relatedConfig = ref(null)

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

// A config is auto-triggered when a schedule frequency is defined
const isAutoTriggered = computed(() => !!relatedConfig.value?.frequency)

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
    details.value.addedToRowB       = data.missingInBilling  || 0
    details.value.missingInCustomer  = data.missingInCustomer || 0

    // Map configuration
    relatedConfig.value = data.configuration ?? null

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

const aiSentences = ref([])
const aiLoading = ref(false)
const aiError = ref(null)

async function loadAiSummary() {
  aiSentences.value = []
  aiError.value = null
  aiLoading.value = true
  try {
    const res = await fetch(`/api/AiSummary/summary/${id}`)
    if (!res.ok) throw new Error(`HTTP error! status: ${res.status}`)
    const data = await res.json()
    if (data.debug?.error) {
      aiError.value = data.debug.error
    } else {
      aiSentences.value = Array.isArray(data.sentences) ? data.sentences : []
    }
  } catch (err) {
    aiError.value = err.message
    console.error('Failed to load AI summary', err)
  } finally {
    aiLoading.value = false
  }
}
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
          </div>
        </div>
      </div>

      <!-- Related Configuration -->
      <div class="card border-0 shadow-sm mb-4">
        <div class="card-body">
          <h6 class="fw-semibold text-muted mb-3">
            <i class="bi bi-sliders me-2"></i>Related Configuration
          </h6>

          <div v-if="!relatedConfig" class="text-muted small">No configuration linked to this run.</div>

          <div v-else>
            <!-- Auto-triggered layout -->
            <div v-if="isAutoTriggered" class="row g-3">
              <div class="col-md-4">
                <div class="detail-card">
                  <div class="detail-card-label">Config ID</div>
                  <div class="detail-card-value">#{{ relatedConfig.configId }}</div>
                </div>
              </div>
              <div class="col-md-4">
                <div class="detail-card">
                  <div class="detail-card-label">Schedule</div>
                  <div class="detail-card-value">
                    {{ relatedConfig.frequency || 'N/A' }}
                    <span v-if="relatedConfig.dayOfMonth"> — Day {{ relatedConfig.dayOfMonth }}</span>
                    <span v-if="relatedConfig.runTime"> at {{ relatedConfig.runTime }}</span>
                  </div>
                </div>
              </div>
              <div class="col-md-4">
                <div class="detail-card">
                  <div class="detail-card-label">Email Delivery</div>
                  <div class="detail-card-value">{{ relatedConfig.emailRecipients || 'N/A' }}</div>
                </div>
              </div>
              <div class="col-md-4">
                <div class="detail-card">
                  <div class="detail-card-label">Default Directory</div>
                  <div class="detail-card-value">{{ relatedConfig.defaultFilePath || 'N/A' }}</div>
                </div>
              </div>
              <div class="col-md-4">
                <div class="detail-card">
                  <div class="detail-card-label">Effective From</div>
                  <div class="detail-card-value">{{ formatDate(relatedConfig.effectiveFrom) }}</div>
                </div>
              </div>
              <div class="col-md-4">
                <div class="detail-card">
                  <div class="detail-card-label">Status</div>
                  <div :class="['detail-card-value fw-semibold', relatedConfig.isActive === 'Y' ? 'text-success' : 'text-secondary']">
                    {{ relatedConfig.isActive === 'Y' ? 'Active' : 'Inactive' }}
                  </div>
                </div>
              </div>
              <div class="col-md-4">
                <div class="detail-card">
                  <div class="detail-card-label">Effective To</div>
                  <div class="detail-card-value">{{ formatDate(relatedConfig.effectiveTo) }}</div>
                </div>
              </div>
              <div class="col-md-4">
                <div class="detail-card">
                  <div class="detail-card-label">Added By</div>
                  <div class="detail-card-value">{{ relatedConfig.addedBy || 'N/A' }}</div>
                </div>
              </div>
            </div>

            <!-- Manual (not auto-triggered) layout -->
            <div v-else class="row g-3">
              <div class="col-md-4">
                <div class="detail-card">
                  <div class="detail-card-label">Config ID</div>
                  <div class="detail-card-value">#{{ relatedConfig.configId }}</div>
                </div>
              </div>
              <div class="col-md-4">
                <div class="detail-card">
                  <div class="detail-card-label">Default Directory</div>
                  <div class="detail-card-value">{{ relatedConfig.defaultFilePath || 'N/A' }}</div>
                </div>
              </div>
              <div class="col-md-4">
                <div class="detail-card">
                  <div class="detail-card-label">Effective From</div>
                  <div class="detail-card-value">{{ formatDate(relatedConfig.effectiveFrom) }}</div>
                </div>
              </div>
              <div class="col-md-4">
                <div class="detail-card">
                  <div class="detail-card-label">Status</div>
                  <div :class="['detail-card-value fw-semibold', relatedConfig.isActive === 'Y' ? 'text-success' : 'text-secondary']">
                    {{ relatedConfig.isActive === 'Y' ? 'Active' : 'Inactive' }}
                  </div>
                </div>
              </div>
              <div class="col-md-4">
                <div class="detail-card">
                  <div class="detail-card-label">Effective To</div>
                  <div class="detail-card-value">{{ formatDate(relatedConfig.effectiveTo) }}</div>
                </div>
              </div>
              <div class="col-md-4">
                <div class="detail-card">
                  <div class="detail-card-label">Added By</div>
                  <div class="detail-card-value">{{ relatedConfig.addedBy || 'N/A' }}</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Process Summary -->
      <div class="card border-0 shadow-sm mb-4">
        <div class="card-body">
          <h6 class="fw-semibold text-muted mb-3">
            <i class="bi bi-bar-chart-fill me-2"></i>Process Summary
          </h6>
          <div class="row g-3">
            <div class="col-md-4">
              <div class="detail-card">
                <div class="detail-card-label">Records Processed</div>
                <div class="detail-card-value">{{ details.recordsProcessed }}</div>
              </div>
            </div>
            <div class="col-md-4">
              <div class="detail-card">
                <div class="detail-card-label">Total Discrepancies</div>
                <div class="detail-card-value text-danger">{{ details.totalDiscrepancies }}</div>
              </div>
            </div>
            <div class="col-md-4">
              <div class="detail-card">
                <div class="detail-card-label">Matched Packages</div>
                <div class="detail-card-value">{{ details.matchedPackages }}</div>
              </div>
            </div>
            <div class="col-md-4">
              <div class="detail-card">
                <div class="detail-card-label">Added to ROWB</div>
                <div class="detail-card-value">{{ details.addedToRowB }}</div>
              </div>
            </div>
            <div class="col-md-4">
              <div class="detail-card">
                <div class="detail-card-label">Missing in Customer</div>
                <div class="detail-card-value">{{ details.missingInCustomer }}</div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Related Files -->
      <div class="card border-0 shadow-sm mb-4">
        <div class="card-body">
          <h6 class="fw-semibold text-muted mb-3">
            <i class="bi bi-file-earmark-fill me-2"></i>Related Files
          </h6>
          <div v-if="files.length === 0" class="text-muted small">
            No files generated for this run
          </div>
          <div v-else class="d-flex justify-content-center align-items-start gap-4 mt-2 flex-wrap">
            <div v-for="(f, idx) in files" :key="idx" class="text-center file-block">
              <a :href="f.url" class="text-decoration-none text-dark" :title="`Created: ${f.createdAt}`">
                <div class="file-icon mb-2">XLSX</div>
                <div class="small">{{ f.name }}</div>
                <div v-if="f.deliveryMethod" class="small text-muted">{{ f.deliveryMethod }}</div>
                <div v-if="f.emailStatus" class="small text-muted">{{ f.emailStatus }}</div>
              </a>
            </div>
          </div>
        </div>
      </div>

      <!-- Related System Logs -->
      <div class="card border-0 shadow-sm mb-4">
        <div class="card-body">
          <h6 class="fw-semibold text-muted mb-3">
            <i class="bi bi-journal-text me-2"></i>Related System Logs
          </h6>
          <div v-if="logs.length === 0" class="text-muted small">
            No logs available for this run
          </div>
          <div v-else class="mt-2">
            <BaseTable :columns="logsColumns" :items="logs" :showSearch="false" :showPagination="false">
              <template #logLevel="{ item }">
                <span
                  v-if="item.logLevel"
                  :class="[
                    'badge text-white',
                    item.logLevel.toUpperCase() === 'ERROR'   ? 'bg-danger' :
                    item.logLevel.toUpperCase() === 'WARNING' ? 'bg-warning' :
                    item.logLevel.toUpperCase() === 'INFO'    ? 'bg-info' :
                    'bg-secondary'
                  ]"
                >
                  {{ item.logLevel }}
                </span>
                <span v-else class="text-muted">-</span>
              </template>
            </BaseTable>
          </div>
        </div>
      </div>

      <!-- AI Summary -->
      <div class="card border-0 shadow-sm mb-4">
        <div class="card-body">
          <div class="d-flex align-items-center justify-content-between mb-3">
            <h6 class="fw-semibold text-muted mb-0">
              <i class="bi bi-stars me-2"></i>AI Summary
            </h6>
            <button
              class="btn btn-sm btn-outline-primary"
              :disabled="aiLoading"
              @click="loadAiSummary"
            >
              <span v-if="aiLoading" class="spinner-border spinner-border-sm me-1" role="status"></span>
              <i v-else class="bi bi-arrow-clockwise me-1"></i>
              {{ aiLoading ? 'Generating…' : (aiSentences.length ? 'Regenerate' : 'Generate AI Summary') }}
            </button>
          </div>

          <div v-if="aiError" class="alert alert-danger mb-0 small">
            <strong>Error:</strong> {{ aiError }}
          </div>

          <div v-else-if="aiSentences.length" class="ai-summary-block">
            <div
              v-for="(sentence, idx) in aiSentences"
              :key="idx"
              class="ai-sentence"
            >
              <span class="ai-sentence-number">{{ idx + 1 }}</span>
              <span>{{ sentence }}</span>
            </div>
          </div>

          <p v-else class="text-muted small mb-0">
            Click the button above to generate an AI-powered summary for this run.
          </p>
        </div>
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

.detail-card {
  background: #f8f9fa;
  border-radius: 8px;
  padding: 0.85rem 1rem;
}

.detail-card-label {
  font-size: 0.78rem;
  color: #6c757d;
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  margin-bottom: 0.3rem;
}

.detail-card-value {
  font-size: 0.95rem;
  font-weight: 600;
  color: #212529;
  word-break: break-all;
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

.ai-summary-block {
  display: flex;
  flex-direction: column;
  gap: 0.65rem;
}

.ai-sentence {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  background: #f8f9fa;
  border-left: 3px solid #0d6efd;
  border-radius: 6px;
  padding: 0.7rem 1rem;
  font-size: 0.93rem;
  color: #212529;
  line-height: 1.6;
}

.ai-sentence-number {
  flex-shrink: 0;
  width: 22px;
  height: 22px;
  border-radius: 50%;
  background: #0d6efd;
  color: #fff;
  font-size: 0.72rem;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-top: 1px;
}
</style>
