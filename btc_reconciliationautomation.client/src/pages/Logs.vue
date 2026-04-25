<script setup>
import { ref, onMounted } from 'vue'
import BaseTable from '../components/common/BaseTable.vue'

const columns = [
  { key: 'id', title: 'ID', width: '80px' },
  { key: 'message', title: 'Message' },
  { key: 'level', title: 'Level', width: '120px' },
  { key: 'user', title: 'User', width: '140px' },
  { key: 'createdAt', title: 'Date', width: '160px', render: 'timestamp' }
]

const items = ref([])

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
      const levelName = l.loG_LEVEL ? (l.loG_LEVEL.LOG_LEVEL_NAME ?? l.loG_LEVEL.log_level_name) : (l.loG_LEVEL_ID ?? l.LOG_LEVEL_ID ?? l.loG_LEVEL_ID ?? '')
      const userName = l.run?.TRIGGERED_BY ?? l.run?.triggereD_BY ?? l.RUN?.TRIGGERED_BY ?? l.RUN?.triggereD_BY ?? l.TRIGGERED_BY ?? l.triggereD_BY ?? ''

      return {
        id: l.loG_ID ?? l.LOG_ID ?? l.id,
        message: l.loG_MESSAGE ?? l.LOG_MESSAGE ?? l.message,
        level: levelName,
        user: userName,
        createdAt: formatDate(l.createD_AT ?? l.CREATED_AT ?? l.createdAt)
      }
    })
  } catch (err) {
    console.error('Error loading logs', err)
  }
}

onMounted(() => { loadLogs() })

const filters = [
  { key: 'level', label: 'Level', type: 'select', options: [
    { value: 'info', label: 'Info' },
    { value: 'warning', label: 'Warning' },
    { value: 'error', label: 'Error' }
  ], default: '' },
  { key: 'user', label: 'User', type: 'text', placeholder: 'Username' },
  { key: 'createdAt', label: 'Date', type: 'date' }
]

function onRowClick(item) {
  // replace with navigation or details panel as needed
  console.log('row clicked', item)
}
</script>

<template>
  <div class="container pt-4">
    <h3>Logs</h3>

    <BaseTable
      :columns="columns"
      :items="items"
      :showSearch="true"
      :showPagination="true"
      :pageSizeOptions="[5,10,25]"
      :filters="filters"
      :rowClickable="true"
      @row-click="onRowClick"
    />
  </div>
</template>

<style scoped></style>
