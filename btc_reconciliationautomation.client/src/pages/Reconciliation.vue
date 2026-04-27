<script setup>
import { ref, onMounted } from 'vue'
import PageHeader from '@/components/common/PageHeader.vue'
import BaseTable from '@/components/common/BaseTable.vue'

const columns = [
  { key: 'id', title: 'ID', width: '80px' },
  { key: 'status', title: 'Status', width: '120px', render: 'statusBadge' },
  { key: 'timestamp', title: 'Timestamp', render: 'timestamp' },
  { key: 'discrepancies', title: 'Discrepancies Count' },
  { key: 'triggeredBy', title: 'Triggered by' }
]

// Helper function to get badge class based on status
function getStatusBadgeClass(status) {
  if (!status) return 'badge bg-secondary'

  const statusUpper = status.toUpperCase()
  if (statusUpper === 'COMPLETED') return 'badge bg-success'
  if (statusUpper === 'FAILED') return 'badge bg-danger'
  return 'badge bg-secondary'
}

const items = ref([])

const filters = [
  { key: 'status', label: 'Status', type: 'select', options: [
    { value: '', label: 'Any' },
    { value: 'COMPLETED', label: 'Completed' },
    { value: 'FAILED', label: 'Failed' },
    { value: 'PENDING', label: 'Pending' }
  ], default: '' },
  { key: 'triggeredBy', label: 'Triggered by', type: 'text', placeholder: 'Username or system' },
  { key: 'timestamp', label: 'Date', type: 'date' }
]

function formatDate(d) {
  if (!d) return ''
  try {
    const dt = new Date(d)
    return dt.toLocaleString()
  } catch (e) {
    return String(d)
  }
}

async function loadRuns() {
  try {
    const res = await fetch('/api/Reconciliation')
    if (!res.ok) {
      console.error('Failed to fetch reconciliation runs', res.status)
      return
    }

    const contentType = res.headers.get('content-type') || ''
    let data
    if (contentType.includes('application/json')) {
      data = await res.json()
    } else {
      // server returned HTML (likely index.html) which means the request didn't reach the API
      const txt = await res.text()
      console.error('Expected JSON but received:', txt.slice(0, 300))
      // helpful console hints
      console.error('Check that the backend API is available at /api/Reconciliation and that the dev proxy is configured.')
      items.value = []
      return
    }
    // map server model to table item shape
    // map server model to table item shape using the API response casing
    items.value = (data || []).map(r => {
      const summaries = r.reconciliation_summaries || []
      // sum discrepancy totals from summaries when available, otherwise fallback to number of summaries
      const totalDiscrepancies = Array.isArray(summaries)
        ? summaries.reduce((acc, s) => acc + (Number(s.totaL_DISCREPANCIES ?? s.TOTAL_DISCREPANCIES ?? 0) || 0), 0)
        : (summaries ?? 0)

      // Get status from RUN_STATUS relationship
      const statusValue = r.ruN_STATUS?.ruN_STATUS1 ?? r.RUN_STATUS?.RUN_STATUS1 ?? 'UNKNOWN'

      return {
        id: r.ruN_ID ?? r.RUN_ID ?? r.id,
        status: statusValue,
        statusBadge: statusValue, // Use statusBadge for rendering
        timestamp: formatDate(r.ruN_DATE ?? r.RUN_DATE ?? r.runDate),
        discrepancies: totalDiscrepancies,
        triggeredBy: r.triggereD_BY ?? r.TRIGGERED_BY ?? r.triggeredBy
      }
    })
  } catch (err) {
    console.error('Error loading reconciliation runs', err)
  }
}

onMounted(() => { loadRuns() })
</script>

<template>
  <div class="container pt-4">
    <PageHeader
      title="Reconciliation Execution History"
      subtitle="Following are the details of the reconciliation process runs :"
      instruction="Click on the row to view the log details"
    >
      <template #icon>
        <!-- simple inline icon for now; replace with proper icon later -->
        <svg width="36" height="36" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" aria-hidden>
          <path d="M3 3h7v7H3z" fill="#6c757d" />
          <path d="M14 3h7v7h-7z" fill="#6c757d" />
          <path d="M3 14h7v7H3z" fill="#6c757d" />
        </svg>
      </template>

    </PageHeader>

    <section class="mt-4">
      <BaseTable
        :columns="columns"
        :items="items"
        :showSearch="true"
        :showPagination="true"
        :pageSizeOptions="[10,25,50]"
        :filters="filters"
        :rowClickable="true"
        @row-click="(item) => $router.push({ name: 'LogDetails', params: { id: item.id } })"
      />
    </section>
  </div>
</template>

<style scoped></style>
