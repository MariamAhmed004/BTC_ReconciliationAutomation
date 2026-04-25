<script setup>
import { ref, computed, onMounted } from 'vue'
import PageHeader from '../components/common/PageHeader.vue'
import BaseCard from '../components/common/BaseCard.vue'
import BaseTable from '../components/common/BaseTable.vue'
import BaseModal from '../components/common/BaseModal.vue'

const title = 'Configuration and manual trigger'

const configurations = ref([])
const loading = ref(false)
const error = ref(null)

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
  { key: 'email_recipients', title: 'Email Recipients' },
  { key: 'schedule_expression', title: 'Schedule' }
]

const showSearch = computed(() => configurations.value.length > 10)
const showPagination = computed(() => configurations.value.length > 10)

const modalButtons = computed(() => [
  { text: 'Close', variant: 'secondary', action: 'close' }
])

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
        email_recipients: config.emaiL_RECIPIENTS || 'N/A',
        schedule_expression: config.schedulE_EXPRESSION || 'N/A',
        created_at: config.createD_AT ? new Date(config.createD_AT).toLocaleString() : 'N/A'
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
  console.log('Starting reconciliation...')
  try {
    const response = await fetch('api/reconciliation/run', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      }
    })

    console.log('Response status:', response.status, response.ok)

    if (response.ok) {
      const data = await response.json()
      console.log('Response data:', data)

      modalTitle.value = 'Success'
      modalMessage.value = data.message || 'Reconciliation process completed successfully'
      modalVariant.value = 'success'
      showModal.value = true

      console.log('Modal should show now:', showModal.value)

      // Optionally refresh configurations after successful run
      await fetchConfigurations()
    } else {
      const errorData = await response.json().catch(() => ({ message: response.statusText }))
      console.log('Error response:', errorData)

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
    console.log('Finished reconciliation call')
  }
}

function closeModal() {
  showModal.value = false
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
      <div class="col-md-4">
        <BaseCard title="Current Configurations">
          <div v-if="loading" class="text-center text-muted small">
            Loading...
          </div>
          <ul v-else-if="configurations.length > 0" class="list-unstyled mb-0 small text-muted">
            <li v-for="c in configurations" :key="c.config_id">
              Configuration {{ c.config_id }} - {{ c.is_active === 'Y' ? 'Active' : 'Inactive' }}
            </li>
          </ul>
          <div v-else class="text-center text-muted small">
            No configurations found
          </div>
        </BaseCard>

        <BaseCard title="Manual Trigger" class="mt-3">
          <div class="w-100 d-flex align-items-center justify-content-center">
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

      <div class="col-md-8">
        <BaseCard title="Add New Configuration" :bodyClass="'add-config-body'">
          <div class="w-100 h-100 d-flex align-items-center justify-content-center text-muted">Placeholder form for adding new configuration</div>
        </BaseCard>
      </div>
    </div>

    <div class="mt-5">
      <h4 class="mb-3">Previously Effective Configurations</h4>
      <BaseTable
        v-if="!loading && configurations.length > 0"
        :columns="columns"
        :items="configurations"
        :showSearch="showSearch"
        :showPagination="showPagination"
        :rowClickable="false"
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
.add-config-body {
  min-height: 320px;
  background: #e9e9e9;
}
</style>
