<script setup>
import { ref } from 'vue'
import BaseTable from '../components/common/BaseTable.vue'

const columns = [
  { key: 'id', title: 'ID', width: '80px' },
  { key: 'message', title: 'Message' },
  { key: 'level', title: 'Level', width: '120px' },
  { key: 'user', title: 'User', width: '140px' },
  { key: 'createdAt', title: 'Date', width: '160px', render: 'timestamp' }
]

const items = ref([
  { id: 1, message: 'File processed', level: 'info', user: 'alice', createdAt: '2026-04-10' },
  { id: 2, message: 'Mismatch found', level: 'warning', user: 'bob', createdAt: '2026-04-11' },
  { id: 3, message: 'Reconciliation failed', level: 'error', user: 'carol', createdAt: '2026-04-12' },
  { id: 4, message: 'Auto-resolved', level: 'info', user: 'dave', createdAt: '2026-04-13' }
])

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
