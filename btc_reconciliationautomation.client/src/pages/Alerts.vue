<script setup>
import { ref, onMounted } from 'vue'
import BaseTable from '../components/common/BaseTable.vue'

const columns = [
  { key: 'status', title: 'Status', width: '80px', render: 'status' },
  { key: 'timestamp', title: 'Timestamp', width: '200px', render: 'timestamp' },
  { key: 'user', title: 'User' },
  { key: 'recordsProcessed', title: 'Records Processed' }
]

const items = ref([])

function formatDate(d) {
  if (!d) return ''
  try { return new Date(d).toLocaleString() } catch { return String(d) }
}

async function loadAlerts() {
  try {
    const res = await fetch('/api/Alerts/latest')
    if (res.ok) {
      const data = await res.json()
      items.value = (data || []).map(a => ({
        status: a.status ?? a.STATUS ?? (a.level ?? ''),
        timestamp: formatDate(a.timestamp ?? a.createdAt ?? a.CREATED_AT),
        user: a.user ?? a.USER ?? a.triggeredBy ?? '',
        recordsProcessed: a.recordsProcessed ?? a.RECORDS_PROCESSED ?? ''
      }))
      return
    }
  } catch (e) {
    console.error('Failed to load alerts', e)
  }

  // fallback sample data
  items.value = [
    { status: 'error', timestamp: formatDate('2025-10-15T11:23:04'), user: 'Ali Hassan', recordsProcessed: '32' },
    { status: 'warning', timestamp: formatDate('2025-10-15T10:12:00'), user: 'Sarah Ahmed', recordsProcessed: '12' },
    { status: 'ok', timestamp: formatDate('2025-10-14T09:05:30'), user: 'Dana Maohammed', recordsProcessed: '18' }
  ]
}

onMounted(() => { loadAlerts() })

const filters = [
  { key: 'timestamp', label: 'Date', type: 'date' },
  { key: 'status', label: 'Status', type: 'select', options: [ { value: '', label: 'Any' }, { value: 'ok', label: 'Success' }, { value: 'warning', label: 'Warning' }, { value: 'error', label: 'Failure' } ], default: '' }
]

function onRowClick(item) {
  console.log('Alert row clicked', item)
}
</script>

<template>
  <div class="container pt-4">
    <h3>Alerts & Process Failures</h3>

    <hr />

    <p class="text-muted">Below are alerts and process failure messages produced during reconciliation runs. Click a row to view details.</p>

    <!-- Alert banners / highlights -->
    <div class="mb-3">
      <div class="alert alert-danger" role="alert">
        Newest Alert: Run XX failed due to generation error / data fetch error
      </div>
      <div class="alert alert-warning" role="alert">
        Warning: Please review records requiring manual intervention
      </div>
      <div class="alert alert-info" role="alert">
        Next reconciliation process on 12 April 2026. Required documents will be sent to xxx@xxx.com
      </div>
    </div>

    <p class="small text-muted">Alerts are derived from failed or error logs and provide a history, resolved issues, and recommendations for remediation.</p>

    <div class="mb-3">
      <label class="form-label">Filter By:</label>
    </div>

    <BaseTable
      :columns="columns"
      :items="items"
      :showSearch="true"
      :showPagination="true"
      :pageSizeOptions="[10,25,50]"
      :filters="filters"
      :rowClickable="true"
      @row-click="onRowClick"
    />
  </div>
</template>

<style scoped></style>
