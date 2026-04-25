<script setup>
import { ref, onMounted, computed } from 'vue'
import BaseCard from './common/BaseCard.vue'
import BaseTable from './common/BaseTable.vue'
import BaseChart from './common/BaseChart.vue'
import AppFooter from './common/AppFooter.vue'

const columns = [
  { key: 'id', title: 'ID', width: '80px' },
  { key: 'status', title: 'Status', width: '80px', render: 'statusBadge' },
  { key: 'timestamp', title: 'Timestamp', render: 'timestamp' },
  { key: 'discrepancies', title: 'Discrepancies Count' },
  { key: 'triggeredBy', title: 'Triggered by' }
]

const items = ref([])

// Dashboard statistics
const stats = ref({
  lastExecutionStatus: 'N/A',
  lastExecutionDate: null,
  lastExecutionError: null,
  totalRunsThisMonth: 0,
  totalDiscrepancies: 0,
  missingInRowb: 0,
  willBeDeactivatedInRowb: 0,
  mismatchedPackages: 0,
  triggeredBy: 'N/A'
})

// Chart data
const chartData = ref({
  pieChart: {
    missingInBilling: 0,
    missingInCustomer: 0,
    mismatch: 0
  },
  lineChart: [],
  successRate: 0
})

const isLoading = ref(false)
const error = ref(null)

// Helper function to format dates
function formatDateForTable(d) {
  if (!d) return ''
  try {
    const dt = new Date(d)
    return dt.toLocaleString()
  } catch (e) {
    return String(d)
  }
}

// Fetch latest 5 reconciliation runs for the table
const fetchLatestRuns = async () => {
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
      const txt = await res.text()
      console.error('Expected JSON but received:', txt.slice(0, 300))
      items.value = []
      return
    }

    // Map server model to table item shape and take only the latest 5
    const allItems = (data || []).map(r => {
      const summaries = r.reconciliation_summaries || []
      const totalDiscrepancies = Array.isArray(summaries)
        ? summaries.reduce((acc, s) => acc + (Number(s.totaL_DISCREPANCIES ?? s.TOTAL_DISCREPANCIES ?? 0) || 0), 0)
        : (summaries ?? 0)

      // Get status from RUN_STATUS relationship
      const statusValue = r.ruN_STATUS?.ruN_STATUS1 ?? r.RUN_STATUS?.RUN_STATUS1 ?? 'UNKNOWN'

      return {
        id: r.ruN_ID ?? r.RUN_ID ?? r.id,
        status: statusValue,
        statusBadge: statusValue,
        timestamp: formatDateForTable(r.ruN_DATE ?? r.RUN_DATE ?? r.runDate),
        discrepancies: totalDiscrepancies,
        triggeredBy: r.triggereD_BY ?? r.TRIGGERED_BY ?? r.triggeredBy
      }
    })

    // Sort by ID descending (latest first) and take only 5
    items.value = allItems
      .sort((a, b) => (b.id ?? 0) - (a.id ?? 0))
      .slice(0, 5)

  } catch (err) {
    console.error('Error loading reconciliation runs', err)
  }
}

// Fetch dashboard statistics
const fetchDashboardStats = async () => {
  isLoading.value = true
  error.value = null
  try {
    const response = await fetch('/api/reconciliation/dashboard/stats')
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`)
    }
    const data = await response.json()
    stats.value = data
  } catch (err) {
    error.value = err.message
    console.error('Error fetching dashboard stats:', err)
  } finally {
    isLoading.value = false
  }
}

// Fetch chart data
const fetchChartData = async () => {
  try {
    const response = await fetch('/api/reconciliation/dashboard/charts')
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`)
    }
    const data = await response.json()
    console.log('Chart data received:', data)
    console.log('Pie chart data:', data.pieChart)
    console.log('Line chart data:', data.lineChart)
    console.log('Success rate:', data.successRate)
    chartData.value = data
  } catch (err) {
    console.error('Error fetching chart data:', err)
  }
}

// Pie chart options
const pieChartOptions = computed(() => {
  const pieData = [
    { name: 'Missing in Billing', y: Number(chartData.value.pieChart.missingInBilling) },
    { name: 'Missing in Customer', y: Number(chartData.value.pieChart.missingInCustomer) },
    { name: 'Mismatched Package', y: Number(chartData.value.pieChart.mismatch) }
  ]

  console.log('Pie chart series data:', pieData)

  return {
    chart: {
      type: 'pie',
      height: 350
    },
    title: {
      text: 'Discrepancy Types Distribution'
    },
    plotOptions: {
      pie: {
        allowPointSelect: true,
        cursor: 'pointer',
        dataLabels: {
          enabled: true,
          format: '<b>{point.name}</b>: {point.percentage:.1f} %'
        }
      }
    },
    series: [{
      name: 'Discrepancies',
      colorByPoint: true,
      data: pieData
    }]
  }
})

// Line chart options
const lineChartOptions = computed(() => {
  const rawData = chartData.value.lineChart

  // Group data by month and aggregate discrepancies
  const monthlyData = {}
  rawData.forEach(item => {
    const date = new Date(item.date)
    const monthKey = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`

    if (!monthlyData[monthKey]) {
      monthlyData[monthKey] = {
        date: date,
        total: 0
      }
    }
    monthlyData[monthKey].total += Number(item.totalDiscrepancies) || 0
  })

  // Get date range (last 12 months or based on data)
  const now = new Date()
  const months = []
  const discrepancies = []

  // Generate last 12 months
  for (let i = 11; i >= 0; i--) {
    const d = new Date(now.getFullYear(), now.getMonth() - i, 1)
    const monthKey = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`
    const monthLabel = d.toLocaleDateString('en-US', { month: 'short', year: 'numeric' })

    months.push(monthLabel)
    discrepancies.push(monthlyData[monthKey]?.total || 0)
  }

  console.log('Line chart months:', months)
  console.log('Line chart discrepancies:', discrepancies)

  return {
    chart: {
      type: 'line',
      height: 350
    },
    title: {
      text: 'Total Discrepancies Over Time'
    },
    xAxis: {
      categories: months,
      title: {
        text: 'Month'
      }
    },
    yAxis: {
      title: {
        text: 'Total Discrepancies'
      },
      min: 0
    },
    series: [{
      name: 'Discrepancies',
      data: discrepancies,
      color: '#dc3545'
    }]
  }
})

// Success rate gauge color
const getSuccessRateColor = computed(() => {
  const rate = chartData.value.successRate
  if (rate >= 90) return '#28a745'
  if (rate < 70) return '#dc3545'
  return '#ffc107'
})

// Format date helper
const formatDate = (date) => {
  if (!date) return 'N/A'
  const d = new Date(date)
  return d.toLocaleString('en-US', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

// Get status display
const getStatusDisplay = () => {
  const status = stats.value.lastExecutionStatus?.toUpperCase()
  if (status === 'COMPLETED') return 'Success'
  if (status === 'FAILED') return 'Fail'
  return status || 'N/A'
}

// Get status class for styling
const getStatusClass = () => {
  const status = stats.value.lastExecutionStatus?.toUpperCase()
  if (status === 'COMPLETED') return 'text-success'
  if (status === 'FAILED') return 'text-danger'
  return 'text-muted'
}

const refreshAll = async () => {
  await fetchDashboardStats()
  await fetchChartData()
  await fetchLatestRuns()
}

onMounted(() => {
  fetchDashboardStats()
  fetchChartData()
  fetchLatestRuns()
})
</script>

<template>

    <div class="container pt-4">

      <div class="row mb-4">
        <div class="col-12">
          <h3 class="fw-bold">Reconciliation Automation Dashboard</h3>
        </div>
      </div>

      <!-- Info box with last run and three small metric cards -->
      <div class="row mb-4">
        <div class="col-12">
          <div class="border rounded p-3 bg-white">
            <div class="d-flex justify-content-between align-items-center mb-3">
              <div>
                <div class="small text-muted">Last Reconciliation Run: {{ formatDate(stats.lastExecutionDate) }}</div>
                <div class="small text-muted">Last detected discrepancies: {{ stats.totalDiscrepancies }}</div>
              </div>
              <div>
                <button class="btn btn-sm btn-success" @click="refreshAll" :disabled="isLoading">
                  {{ isLoading ? 'Loading...' : 'Refresh' }}
                </button>
              </div>
            </div>

            <div v-if="error" class="alert alert-danger small" role="alert">
              Error loading data: {{ error }}
            </div>

            <hr />

            <div class="row text-center">
              <div class="col-md-4 mb-2">
                <BaseCard title="Missing in ROWB" headerClass="bg-light text-dark border" bodyClass="py-3">
                  {{ stats.missingInRowb }}
                </BaseCard>
              </div>
              <div class="col-md-4 mb-2">
                <BaseCard title="To be deactivated in ROWB" headerClass="bg-light text-dark border" bodyClass="py-3">
                  {{ stats.willBeDeactivatedInRowb }}
                </BaseCard>
              </div>
              <div class="col-md-4 mb-2">
                <BaseCard title="Mismatched packages" headerClass="bg-light text-dark border" bodyClass="py-3">
                  {{ stats.mismatchedPackages }}
                </BaseCard>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Charts Section -->
      <div class="row mb-4">
        <div class="col-12">
          <h5 class="mb-3">Discrepancy Types Distribution</h5>
          <BaseChart :options="pieChartOptions" />
        </div>
      </div>

      <div class="row mb-4">
        <div class="col-12">
          <h5 class="mb-3">Total Discrepancies Over Time</h5>
          <BaseChart :options="lineChartOptions" />
        </div>
      </div>

      <div class="row mb-4">
        <div class="col-12">
          <h5 class="mb-3 text-center">Reconciliation Success Rate</h5>
          <div class="d-flex justify-content-center align-items-center" style="height: 300px;">
            <div class="text-center">
              <div class="success-rate-circle" :style="{ borderColor: getSuccessRateColor }">
                <div class="success-rate-value" :style="{ color: getSuccessRateColor }">
                  {{ chartData.successRate.toFixed(1) }}%
                </div>
              </div>
              <div class="mt-3 text-muted">Successful Completion Rate</div>
            </div>
          </div>
        </div>
      </div>

      <div class="row mb-4">
        <div class="col-md-4">
          <BaseCard title="Total runs this month" headerClass="bg-light text-dark border" bodyClass="display-6 text-center">
            {{ stats.totalRunsThisMonth }}
          </BaseCard>
        </div>
        <div class="col-md-4">
          <BaseCard title="Total discrepancies detected" headerClass="bg-light text-dark border" bodyClass="display-6 text-center">
            {{ stats.totalDiscrepancies }}
          </BaseCard>
        </div>
        <div class="col-md-4">
          <BaseCard title="Last Execution" headerClass="bg-light text-dark border" bodyClass="display-6 text-center">
            <span :class="getStatusClass()">{{ getStatusDisplay() }}</span>
          </BaseCard>
        </div>
      </div>

      <div class="row">
        <div class="col-12">
          <h5 class="mb-3">Latest Reconciliation Runs</h5>
          <BaseTable 
            :columns="columns" 
            :items="items" 
            :showSearch="false" 
            :showPagination="false" 
            :rowClickable="false" 
          />
        </div>
      </div>

      <div class="row mt-4">
        <div class="col-12">
          <BaseCard title="Last Execution Info" headerClass="bg-light text-dark border">
            <div class="text-center">
              <div><strong>Date:</strong> {{ formatDate(stats.lastExecutionDate) }}</div>
              <div><strong>Status:</strong> <span :class="getStatusClass()">{{ getStatusDisplay() }}</span></div>
              <div v-if="stats.lastExecutionError"><strong>Error:</strong> {{ stats.lastExecutionError }}</div>
              <div v-else><strong>Description:</strong> Execution completed successfully</div>
              <div><strong>Triggered By:</strong> {{ stats.triggeredBy }}</div>
            </div>
          </BaseCard>
        </div>
      </div>


</div>

      <AppFooter />

</template>

<style scoped>
/* small spacing tweak */
.display-6 { font-size: 2rem; }
.fst-italic { font-style: italic; }
.file-icon { }

/* Success rate circle styles */
.success-rate-circle {
  width: 180px;
  height: 180px;
  border-radius: 50%;
  border: 12px solid;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  transition: border-color 0.3s ease;
}

.success-rate-value {
  font-size: 3rem;
  font-weight: bold;
  transition: color 0.3s ease;
}
</style>
