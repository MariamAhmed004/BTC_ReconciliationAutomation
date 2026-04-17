<script setup>
import { ref, computed } from 'vue'
import PageHeader from '../components/common/PageHeader.vue'
import BaseCard from '../components/common/BaseCard.vue'
import BaseTable from '../components/common/BaseTable.vue'

const title = 'Configuration and manual trigger'

// sample existing configurations (previously effective)
const configurations = ref([
  { id: 1001, status: 'ok', timestamp: '11:23:04 October 12,2025', discrepanciesCount: '32: Ali Hassan', triggeredBy: 'Application Admin' },
  { id: 2002, status: 'failed', timestamp: '11:23:04 October 12,2025', discrepanciesCount: '12: Sarah Ahmed', triggeredBy: 'info@acme' },
  { id: 1011, status: 'ok', timestamp: '11:23:04 October 12,2025', discrepanciesCount: '18: Dana Maohammed', triggeredBy: 'hello@edu' }
])

const columns = [
  { key: 'id', title: 'ID', width: '80px' },
  { key: 'status', title: 'Status', width: '100px', render: 'status' },
  { key: 'timestamp', title: 'Timestamp', render: 'timestamp' },
  { key: 'discrepanciesCount', title: 'Discrepancies Count' },
  { key: 'triggeredBy', title: 'Triggered by' }
]

const showSearch = computed(() => configurations.value.length > 10)
const showPagination = computed(() => configurations.value.length > 10)

function runReconciliation() {
  // placeholder action for future wiring
  console.log('Run Reconciliation clicked')
  alert('Run Reconciliation triggered (placeholder)')
}
</script>

<template>
  <div class="container pt-4">
    <PageHeader :title="title" />

    <div class="row mt-4">
      <div class="col-md-4">
        <BaseCard title="Current Configurations">
          <ul class="list-unstyled mb-0 small text-muted">
            <li v-for="c in configurations" :key="c.id">Configuration {{ c.id }}</li>
          </ul>
        </BaseCard>

        <BaseCard title="Manual Trigger" class="mt-3">
          <div class="w-100 d-flex align-items-center justify-content-center">
            <button class="btn btn-lg btn-outline-primary" @click="runReconciliation">Run Reconciliation</button>
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
        :columns="columns"
        :items="configurations"
        :showSearch="showSearch"
        :showPagination="showPagination"
        :rowClickable="false"
      />
    </div>
  </div>
</template>

<style scoped>
.add-config-body {
  min-height: 320px;
  background: #e9e9e9;
}
</style>
