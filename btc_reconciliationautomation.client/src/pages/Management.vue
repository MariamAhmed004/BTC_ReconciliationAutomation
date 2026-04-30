<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import PageHeader from '../components/common/PageHeader.vue'
import BaseCard from '../components/common/BaseCard.vue'
import BaseTable from '../components/common/BaseTable.vue'
import BaseModal from '../components/common/BaseModal.vue'

const router = useRouter()
const title = 'Configuration and manual trigger'

const configurations = ref([])
const loading = ref(false)
const error = ref(null)
const latestActiveConfig = ref(null)

const showModal = ref(false)
const modalTitle = ref('')
const modalMessage = ref('')
const modalVariant = ref('info')
const isRunning = ref(false)

const columns = [
  { key: 'config_id', title: 'ID', width: '80px' },
  { key: 'is_active', title: 'Status', width: '100px', render: 'status' },
  { key: 'effective_from', title: 'Effective From', render: 'timestamp' },
  { key: 'effective_to', title: 'Effective To', render: 'timestamp' },
  { key: 'added_by', title: 'Added By' }
]

const showSearch = computed(() => configurations.value.length > 10)
const showPagination = computed(() => configurations.value.length > 10)

const modalButtons = computed(() => [
  { text: 'Close', variant: 'secondary', action: 'close' }
])

// Extract the latest active configuration
const activeConfiguration = computed(() => {
  const activeConfigs = configurations.value.filter(c => c.is_active === 'Y')
  if (activeConfigs.length === 0) return null

  // Sort by created_at descending and get the latest one
  return activeConfigs.sort((a, b) => {
    const dateA = new Date(a.created_at)
    const dateB = new Date(b.created_at)
    return dateB - dateA
  })[0]
})

// Format schedule expression into readable text
function formatSchedule(config) {
  if (!config) return 'No schedule configured'

  const frequency = config.frequency || 'N/A'
  const runTime = config.run_time || 'N/A'
  const dayOfMonth = config.day_of_month || 'N/A'

  if (frequency === 'N/A' || runTime === 'N/A') {
    return 'No schedule configured'
  }

  // Build a readable sentence
  let schedule = `Runs ${frequency.toLowerCase()}`

  if (dayOfMonth !== 'N/A' && frequency.toLowerCase() === 'monthly') {
    schedule += ` on day ${dayOfMonth}`
  }

  schedule += ` at ${runTime}`

  return schedule
}

async function fetchConfigurations() {
  loading.value = true
  error.value = null
  try {
    const response = await fetch('api/configuration')
    if (response.ok) {
      const data = await response.json()
      configurations.value = data.map(config => ({
        config_id: config.confiG_ID,
        is_active: config.iS_ACTIVE,
        effective_from: config.effectivE_FROM ? new Date(config.effectivE_FROM).toLocaleString() : 'N/A',
        effective_to: config.effectivE_TO ? new Date(config.effectivE_TO).toLocaleString() : 'N/A',
        added_by: config.addeD_BY || 'N/A',
        created_at: config.createD_AT ? new Date(config.createD_AT).toLocaleString() : 'N/A',
        // Additional fields for the card display
        email_recipients: config.emaiL_RECIPIENTS || 'N/A',
        frequency: config.frequencY || 'N/A',
        run_time: config.ruN_TIME || 'N/A',
        day_of_month: config.daY_OF_MONTH || 'N/A',
        default_file_path: config.defaulT_FILE_PATH || 'N/A',
        days_to_delete_auditlogs: config.dayS_TO_DELETE_AUDITLOGS ?? 'N/A'
      }))
    } else {
      error.value = `Failed to fetch configurations: ${response.statusText}`
    }
  } catch (err) {
    error.value = `Error fetching configurations: ${err.message}`
    console.error('Error fetching configurations:', err)
  } finally {
    loading.value = false
  }
}

async function runReconciliation() {
  isRunning.value = true
  try {
    // TODO: replace 'MA' with the actual logged-in username when auth is implemented
    const triggeredBy = 'MA'

    const response = await fetch('api/reconciliation/run', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ triggeredBy })
    })

    if (response.ok) {
      // Response is a zip file — trigger browser download
      const blob = await response.blob()
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = 'reconciliation_results.zip'
      document.body.appendChild(a)
      a.click()
      a.remove()
      URL.revokeObjectURL(url)

      modalTitle.value = 'Success'
      modalMessage.value = 'Reconciliation completed. Your files are downloading.'
      modalVariant.value = 'success'
      showModal.value = true

      await fetchConfigurations()
    } else {
      const errorData = await response.json().catch(() => ({ message: response.statusText }))
      modalTitle.value = 'Error'
      modalMessage.value = errorData.message || 'Failed to run reconciliation'
      modalVariant.value = 'danger'
      showModal.value = true
    }
  } catch (err) {
    console.error('Exception caught:', err)
    modalTitle.value = 'Error'
    modalMessage.value = `Error running reconciliation: ${err.message}`
    modalVariant.value = 'danger'
    showModal.value = true
  } finally {
    isRunning.value = false
  }
}

function closeModal() {
  showModal.value = false
}

function navigateToReconfigure() {
  router.push('/reconfigure')
}

onMounted(() => {
  fetchConfigurations()
})

</script>

<template>
  <div class="container pt-4">
    <PageHeader :title="title" />

    <div v-if="loading" class="alert alert-info mt-3">
      Loading configurations...
    </div>

    <div v-if="error" class="alert alert-danger mt-3">
      {{ error }}
    </div>

    <div class="row mt-4">
      <div class="col-md-7">
        <BaseCard title="Current Active Configuration">
          <div v-if="loading" class="text-center text-muted">
            Loading...
          </div>
          <div v-else-if="activeConfiguration" class="config-details">
            <!-- Configuration ID -->
            <div class="config-item">
              <span class="config-label">Configuration ID:</span>
              <span class="config-value">#{{ activeConfiguration.config_id }}</span>
            </div>

            <!-- Scheduling -->
            <div class="config-item">
              <span class="config-label">Scheduling:</span>
              <span class="config-value">{{ formatSchedule(activeConfiguration) }}</span>
            </div>

            <!-- Email Recipients -->
            <div class="config-item">
              <span class="config-label">Email Delivery:</span>
              <span class="config-value">Reports are sent to {{ activeConfiguration.email_recipients }}</span>
            </div>

            <!-- Default File Path -->
            <div class="config-item">
              <span class="config-label">Default File Path:</span>
              <span class="config-value">{{ activeConfiguration.default_file_path }}</span>
            </div>

            <!-- Log Retention -->
            <div class="config-item">
              <span class="config-label">Log Retention:</span>
              <span class="config-value">
                Audit logs are deleted after {{ activeConfiguration.days_to_delete_auditlogs }} days of creation
              </span>
            </div>

            <!-- Effective From -->
            <div class="config-item">
              <span class="config-label">Effective From:</span>
              <span class="config-value">{{ activeConfiguration.effective_from }}</span>
            </div>

            <!-- Added By -->
            <div class="config-item">
              <span class="config-label">Added By:</span>
              <span class="config-value">{{ activeConfiguration.added_by }}</span>
            </div>
          </div>
          <div v-else class="text-center text-muted py-4">
            No active configuration found
          </div>
        </BaseCard>
      </div>

      <div class="col-md-5">
        <BaseCard title="Change Configurations" :bodyClass="'action-card-body'">
          <div class="w-100 h-100 d-flex align-items-center justify-content-center">
            <button 
              class="btn btn-lg btn-outline-secondary"
              @click="navigateToReconfigure"
            >
              Reconfigure
            </button>
          </div>
        </BaseCard>

        <BaseCard title="Manual Trigger" class="mt-3" :bodyClass="'action-card-body'">
          <div class="w-100 h-100 d-flex align-items-center justify-content-center">
            <button 
              class="btn btn-lg btn-outline-primary" 
              @click="runReconciliation"
              :disabled="isRunning"
            >
              <span v-if="isRunning" class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
              {{ isRunning ? 'Running...' : 'Run Reconciliation' }}
            </button>
          </div>
        </BaseCard>
      </div>
    </div>

    <div class="mt-5">
      <h4 class="mb-3">Previously Effective Configurations <small class="text-muted fs-6 fw-normal">— click a row to view full details</small></h4>
      <BaseTable
        v-if="!loading && configurations.length > 0"
        :columns="columns"
        :items="configurations"
        :showSearch="showSearch"
        :showPagination="showPagination"
        :rowClickable="true"
        @row-click="(item) => $router.push({ name: 'ConfigurationDetails', params: { id: item.config_id } })"
      />
      <div v-else-if="!loading" class="alert alert-warning">
        No configurations available
      </div>
    </div>

    <BaseModal
      :show="showModal"
      :title="modalTitle"
      :message="modalMessage"
      :buttons="modalButtons"
      @close="closeModal"
    />

    <!-- Debug info - remove after testing -->
    <div v-if="false" class="mt-3 alert alert-info">
      Debug: showModal={{ showModal }}, title={{ modalTitle }}, message={{ modalMessage }}
    </div>
  </div>
</template>

<style scoped>
.config-details {
  font-size: 0.95rem;
}

.config-item {
  padding: 0.75rem 0;
  border-bottom: 1px solid #e9ecef;
  display: flex;
  flex-direction: row;
  gap: 0.5rem;
  align-items: flex-start;
}

.config-item:last-child {
  border-bottom: none;
}

.config-label {
  font-size: 0.85rem;
  color: #6c757d;
  font-weight: 600;
  min-width: 140px;
  flex-shrink: 0;
}

.config-value {
  font-size: 0.9rem;
  color: #212529;
  font-weight: 400;
  word-wrap: break-word;
  word-break: break-word;
  overflow-wrap: break-word;
  flex: 1;
  min-width: 0;
}

.config-details .text-muted {
  font-size: 0.85rem;
}

.action-card-body {
  min-height: 120px;
  display: flex;
  align-items: center;
  justify-content: center;
}
</style>
