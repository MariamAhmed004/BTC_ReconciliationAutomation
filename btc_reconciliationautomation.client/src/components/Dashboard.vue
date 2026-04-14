<script setup>
import BaseCard from './common/BaseCard.vue'
import BaseTable from './common/BaseTable.vue'
import BaseChart from './common/BaseChart.vue'

// Simple dashboard to verify Bootstrap/Bootswatch integration -- presentational only

const columns = [
  { key: 'id', title: 'ID', width: '80px' },
  { key: 'status', title: 'Status', width: '80px', render: 'status' },
  { key: 'timestamp', title: 'Timestamp', render: 'timestamp' },
  { key: 'discrepancies', title: 'Discrepancies Count' },
  { key: 'triggeredBy', title: 'Triggered by' }
]

const chartOptions = {
  chart: { type: 'column' },
  title: { text: 'Sample Reconciliation Volume' },
  xAxis: { categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May'] },
  yAxis: { title: { text: 'Count' } },
  series: [ { name: 'Reconciled', data: [29, 71, 106, 129, 144] }, { name: 'Pending', data: [39, 51, 66, 89, 104] } ]
}

const items = [
  { id: '1001', status: 'ok', timestamp: '11:23:04 October 12, 2025', discrepancies: '32: Ali Hassan', triggeredBy: 'Application Admin' },
  { id: '2002', status: 'error', timestamp: '11:23:04 October 12, 2025', discrepancies: '12: Sarah Ahmed', triggeredBy: 'info@acme' },
  { id: '1011', status: 'ok', timestamp: '11:23:04 October 12, 2025', discrepancies: '18: Dana Maohammed', triggeredBy: 'hello@edu' }
]
</script>

<template>

    <div class="container pt-4">
      <div class="row mb-4">
        <div class="col-md-4">
          <BaseCard title="Reconciled" headerClass="bg-success text-white" bodyClass="display-6">
            1,234
          </BaseCard>
        </div>
        <div class="col-md-4">
          <BaseCard title="Pending" headerClass="bg-warning text-dark" bodyClass="display-6">
            56
          </BaseCard>
        </div>
        <div class="col-12 mt-4">
          <BaseTable :columns="columns" :items="items" />
        </div>
        <div class="col-12 mt-4">
          <BaseChart :options="chartOptions" />
        </div>
      </div>
    </div>


</template>

<style scoped>
/* small spacing tweak */
.display-6 { font-size: 2rem; }
</style>
