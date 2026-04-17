<script setup>
import PageHeader from '@/components/common/PageHeader.vue'
import BaseTable from '@/components/common/BaseTable.vue'

const columns = [
  { key: 'id', title: 'ID', width: '80px' },
  { key: 'status', title: 'Status', width: '80px', render: 'status' },
  { key: 'timestamp', title: 'Timestamp', render: 'timestamp' },
  { key: 'discrepancies', title: 'Discrepancies Count' },
  { key: 'triggeredBy', title: 'Triggered by' }
]

const items = [
  { id: '1001', status: 'ok', timestamp: '11:23:04 October 12, 2025', discrepancies: '32: Ali Hassan', triggeredBy: 'Application Admin' },
  { id: '2002', status: 'error', timestamp: '11:23:04 October 12, 2025', discrepancies: '12: Sarah Ahmed', triggeredBy: 'info@acme' },
  { id: '1011', status: 'ok', timestamp: '11:23:04 October 12, 2025', discrepancies: '18: Dana Maohammed', triggeredBy: 'hello@edu' }
]
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
      <BaseTable :columns="columns" :items="items" :showSearch="true" :showPagination="true" :rowClickable="true" @row-click="(item) => $router.push({ name: 'LogDetails', params: { id: item.id } })" />
    </section>
  </div>
</template>

<style scoped></style>
