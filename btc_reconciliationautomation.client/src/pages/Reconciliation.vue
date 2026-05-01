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
  ], default: '', sortable: false },
  { key: 'triggeredBy', label: 'Triggered by', type: 'text', placeholder: 'Username or system' },
  { key: 'timestampRaw', label: 'Date', type: 'date' }
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
    const mappedItems = (data || []).map(r => {
      const summaries = r.reconciliation_summaries || []
      // sum discrepancy totals from summaries when available, otherwise fallback to number of summaries
      const totalDiscrepancies = Array.isArray(summaries)
        ? summaries.reduce((acc, s) => acc + (Number(s.totaL_DISCREPANCIES ?? s.TOTAL_DISCREPANCIES ?? 0) || 0), 0)
        : (summaries ?? 0)

      // Get status from RUN_STATUS relationship
      const statusValue = r.ruN_STATUS?.ruN_STATUS1 ?? r.RUN_STATUS?.RUN_STATUS1 ?? 'UNKNOWN'

      // Get raw date for filtering
      const rawDate = r.ruN_DATE ?? r.RUN_DATE ?? r.runDate
      const runDateMs = rawDate ? new Date(rawDate).getTime() : null

      return {
        id: r.ruN_ID ?? r.RUN_ID ?? r.id,
        status: statusValue,
        statusBadge: statusValue, // Use statusBadge for rendering
        timestamp: formatDate(rawDate), // Formatted for display
        timestampRaw: rawDate, // Keep raw date for filtering
        runDateMs: Number.isFinite(runDateMs) ? runDateMs : null,
        discrepancies: totalDiscrepancies,
        triggeredBy: r.triggereD_BY ?? r.TRIGGERED_BY ?? r.triggeredBy
      }
    })

    // Sort by run timestamp descending (latest first), fallback to ID
    items.value = mappedItems.sort((a, b) => {
      const aDate = a.runDateMs
      const bDate = b.runDateMs
      if (aDate != null && bDate != null && aDate !== bDate) return bDate - aDate
      if (aDate != null && bDate == null) return -1
      if (aDate == null && bDate != null) return 1
      return (b.id ?? 0) - (a.id ?? 0)
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
        <i class="bi bi-clock-history" style="font-size: 2rem; color: #495057;"></i>
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
