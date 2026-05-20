<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import PageHeader from '../components/common/PageHeader.vue'

const route = useRoute()
const router = useRouter()
const id = route.params.id

const config = ref(null)
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

const statusDisplay = computed(() => {
  if (!config.value) return { text: 'N/A', class: 'text-secondary' }
  return config.value.iS_ACTIVE === 'Y'
    ? { text: 'Active', class: 'text-success' }
    : { text: 'Inactive', class: 'text-secondary' }
})

async function loadConfig() {
  isLoading.value = true
  error.value = null
  try {
    const res = await fetch(`/api/Configuration/${id}`)
    if (!res.ok) throw new Error(`HTTP error! status: ${res.status}`)
    config.value = await res.json()
  } catch (err) {
    error.value = err.message
    console.error('Failed to load configuration details', err)
  } finally {
    isLoading.value = false
  }
}

function goBack() {
  router.push('/management')
}

onMounted(() => { loadConfig() })
</script>

<template>
  <div class="container pt-4">
    <PageHeader
      title="Configuration Details"
      subtitle="Full details of the selected system configuration:"
      instruction="This configuration may be linked to one or more reconciliation runs"
    >
      <template #icon>
        <i class="bi bi-arrow-left-square-fill" style="font-size: 2rem; color: #495057; cursor: pointer;" @click="goBack"></i>
      </template>
    </PageHeader>

    <!-- Loading -->
    <div v-if="isLoading" class="text-center my-5">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
      <p class="mt-2 text-muted">Loading configuration...</p>
    </div>

    <!-- Error -->
    <div v-if="error" class="alert alert-danger mt-3" role="alert">
      <strong>Error:</strong> {{ error }}
    </div>

    <!-- Content -->
    <div v-if="!isLoading && !error && config">

      <!-- Status & Meta -->
      <div class="detail-section mb-4">
        <div class="d-flex align-items-center gap-3 mb-3">
          <span class="detail-label">Status</span>
          <span :class="['fw-semibold', statusDisplay.class]">{{ statusDisplay.text }}</span>
        </div>
        <div class="row g-3">
          <div class="col-md-4">
            <div class="detail-card">
              <div class="detail-card-label">Config ID</div>
              <div class="detail-card-value">#{{ config.confiG_ID }}</div>
            </div>
          </div>
          <div class="col-md-4">
            <div class="detail-card">
              <div class="detail-card-label">Added By</div>
              <div class="detail-card-value">{{ config.addeD_BY || 'N/A' }}</div>
            </div>
          </div>
          <div class="col-md-4">
            <div class="detail-card">
              <div class="detail-card-label">Created At</div>
              <div class="detail-card-value">{{ formatDate(config.createD_AT) }}</div>
            </div>
          </div>
          <div class="col-md-4">
            <div class="detail-card">
              <div class="detail-card-label">Effective From</div>
              <div class="detail-card-value">{{ formatDate(config.effectivE_FROM) }}</div>
            </div>
          </div>
          <div class="col-md-4">
            <div class="detail-card">
              <div class="detail-card-label">Effective To</div>
              <div class="detail-card-value">{{ formatDate(config.effectivE_TO) }}</div>
            </div>
          </div>
        </div>
      </div>

      <hr />

      <!-- Schedule Settings -->
      <div class="detail-section mb-4">
        <h5 class="detail-section-title">
          <i class="bi bi-clock-fill me-2 text-muted"></i>Schedule Settings
        </h5>
        <div class="row g-3">
          <div class="col-md-4">
            <div class="detail-card">
              <div class="detail-card-label">Frequency</div>
              <div class="detail-card-value">{{ config.frequency || 'N/A' }}</div>
            </div>
          </div>
          <div class="col-md-4">
            <div class="detail-card">
              <div class="detail-card-label">Run Time</div>
              <div class="detail-card-value">{{ config.ruN_TIME || 'N/A' }}</div>
            </div>
          </div>
          <div class="col-md-4">
            <div class="detail-card">
              <div class="detail-card-label">Days of Month</div>
              <div class="detail-card-value">{{ config.daY_OF_MONTH || 'N/A' }}</div>
            </div>
          </div>
        </div>
      </div>

      <hr />

      <!-- Delivery Settings -->
      <div class="detail-section mb-4">
        <h5 class="detail-section-title">
          <i class="bi bi-envelope-fill me-2 text-muted"></i>Delivery Settings
        </h5>
        <div class="row g-3">
          <div class="col-12">
            <div class="detail-card">
              <div class="detail-card-label">Email Recipients</div>
              <div class="detail-card-value">{{ config.emaiL_RECIPIENTS || 'N/A' }}</div>
            </div>
          </div>
        </div>
      </div>

      <hr />

      <!-- Storage & Processing Settings -->
      <div class="detail-section mb-4">
        <h5 class="detail-section-title">
          <i class="bi bi-folder-fill me-2 text-muted"></i>Storage & Processing
        </h5>
        <div class="row g-3">
          <div class="col-md-6">
            <div class="detail-card">
              <div class="detail-card-label">Default File Path</div>
              <div class="detail-card-value">{{ config.defaulT_FILE_PATH || 'N/A' }}</div>
            </div>
          </div>
          <div class="col-md-3">
            <div class="detail-card">
              <div class="detail-card-label">Log Retention (days)</div>
              <div class="detail-card-value">{{ config.dayS_TO_DELETE_AUDITLOGS ?? 'N/A' }}</div>
            </div>
          </div>
          <div class="col-md-3">
            <div class="detail-card">
              <div class="detail-card-label">Ignore Conditions</div>
              <div class="detail-card-value">{{ config.ignorE_CONDITIONS || 'N/A' }}</div>
            </div>
          </div>
        </div>
      </div>

    </div>
  </div>
</template>

<style scoped>
.detail-section-title {
  font-size: 1rem;
  font-weight: 600;
  color: #495057;
  margin-bottom: 1rem;
}

.detail-label {
  font-size: 0.85rem;
  color: #6c757d;
  font-weight: 500;
  min-width: 80px;
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
</style>
