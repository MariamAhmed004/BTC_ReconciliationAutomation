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

// Live statistics (updated on refresh)
const liveStats = ref({
  missingInRowb: 0,
  notActiveInSiebel: 0,
  mismatchedPackages: 0,
  timestamp: null
})

// Total across all three live discrepancy categories
const liveTotal = computed(() =>
  (liveStats.value.missingInRowb ?? 0) +
  (liveStats.value.notActiveInSiebel ?? 0) +
  (liveStats.value.mismatchedPackages ?? 0)
)

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

// Dashboard alerts
const alertInfo    = ref(null)   // schedule info
const alertFailed  = ref(false)  // last run failed
const alertHighDisc = ref({ show: false, count: 0 })

const showInfoAlert    = ref(false)
const showFailedAlert  = ref(false)
const showHighDiscAlert = ref(false)

const fetchAlerts = async () => {
  try {
    const res = await fetch('/api/reconciliation/dashboard/alerts')
    if (!res.ok) return
    const data = await res.json()

    if (data.scheduleAlert) {
      alertInfo.value = data.scheduleAlert
      showInfoAlert.value = true
    }

    if (data.lastRunFailed) {
      alertFailed.value = true
      showFailedAlert.value = true
    }
  } catch (err) {
    console.error('Error fetching dashboard alerts:', err)
  }
}

const formatScheduleLabel = (info) => {
  if (!info) return ''
  const freq = (info.frequency ?? '').toUpperCase()
  if (freq === 'MONTHLY') {
    const day = info.dayOfMonth ? `on day ${info.dayOfMonth}` : ''
    const time = info.runTime ? `at ${info.runTime}` : ''
    return `Monthly ${day} ${time}`.trim()
  }
  if (freq === 'DAILY') {
    const time = info.runTime ? `at ${info.runTime}` : ''
    return `Daily ${time}`.trim()
  }
  if (freq === 'WEEKLY') {
    const time = info.runTime ? `at ${info.runTime}` : ''
    return `Weekly ${time}`.trim()
  }
  return info.frequency ?? ''
}

const computeNextRun = (info) => {
  if (!info) return null
  const freq = (info.frequency ?? '').toUpperCase()

  // Parse runTime "HH:mm" or "HH:mm:ss"
  const [hours, minutes] = (info.runTime ?? '00:00').split(':').map(Number)

  const now = new Date()
  let next = null

  if (freq === 'DAILY') {
    next = new Date(now.getFullYear(), now.getMonth(), now.getDate(), hours, minutes, 0)
    if (next <= now) next.setDate(next.getDate() + 1)
  }

  if (freq === 'MONTHLY') {
    // Parse comma-separated day numbers e.g. "15,17,20"
    const days = (info.dayOfMonth ?? '')
      .split(',')
      .map(d => parseInt(d.trim(), 10))
      .filter(d => !isNaN(d) && d >= 1 && d <= 31)
      .sort((a, b) => a - b)

    if (days.length === 0) return null

    // Find the nearest upcoming day in the current or next month
    let found = null
    for (const day of days) {
      const candidate = new Date(now.getFullYear(), now.getMonth(), day, hours, minutes, 0)
      if (candidate > now) {
        found = candidate
        break
      }
    }
    // If none found this month, take the first day of next month
    if (!found) {
      found = new Date(now.getFullYear(), now.getMonth() + 1, days[0], hours, minutes, 0)
    }
    next = found
  }

  if (freq === 'WEEKLY') {
    // Parse comma-separated day names e.g. "Monday,Wednesday"
    const dayNameMap = {
      sunday: 0, monday: 1, tuesday: 2, wednesday: 3,
      thursday: 4, friday: 5, saturday: 6
    }
    const targetDays = (info.dayOfMonth ?? '')
      .split(',')
      .map(d => dayNameMap[d.trim().toLowerCase()])
      .filter(d => d !== undefined)
      .sort((a, b) => a - b)

    if (targetDays.length === 0) return null

    const todayDow = now.getDay()
    let minDaysAhead = null
    for (const dow of targetDays) {
      let diff = dow - todayDow
      if (diff < 0 || (diff === 0 && new Date(now.getFullYear(), now.getMonth(), now.getDate(), hours, minutes, 0) <= now)) {
        diff += 7
      }
      if (minDaysAhead === null || diff < minDaysAhead) minDaysAhead = diff
    }
    next = new Date(now.getFullYear(), now.getMonth(), now.getDate() + minDaysAhead, hours, minutes, 0)
  }

  if (!next) return null

  const dayName  = next.toLocaleDateString('en-US', { weekday: 'long' })
  const dayNum   = next.getDate()
  const month    = next.toLocaleDateString('en-US', { month: 'long' })
  const year     = next.getFullYear()
  const timeStr  = next.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', hour12: false })

  return `${dayName}, ${dayNum} ${month} ${year} at ${timeStr}`
}

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

      const runDate = r.ruN_DATE ?? r.RUN_DATE ?? r.runDate
      const runDateMs = runDate ? new Date(runDate).getTime() : null

      return {
        id: r.ruN_ID ?? r.RUN_ID ?? r.id,
        status: statusValue,
        statusBadge: statusValue,
        timestamp: formatDateForTable(runDate),
        runDateMs: Number.isFinite(runDateMs) ? runDateMs : null,
        discrepancies: totalDiscrepancies,
        triggeredBy: r.triggereD_BY ?? r.TRIGGERED_BY ?? r.triggeredBy
      }
    })

    // Sort by run timestamp descending (latest first), fallback to ID, and take only 5
    items.value = allItems
      .sort((a, b) => {
        const aDate = a.runDateMs
        const bDate = b.runDateMs
        if (aDate != null && bDate != null && aDate !== bDate) return bDate - aDate
        if (aDate != null && bDate == null) return -1
        if (aDate == null && bDate != null) return 1
        return (b.id ?? 0) - (a.id ?? 0)
      })
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

// Fetch live reconciliation statistics
const fetchLiveStats = async () => {
  try {
    const response = await fetch('/api/reconciliation/dashboard/live-stats')
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`)
    }
    const data = await response.json()
    console.log('Live stats received:', data)
    liveStats.value = data

    // Drive the high-discrepancy alert from the live totals
    const total = (data.missingInRowb ?? 0) + (data.notActiveInSiebel ?? 0) + (data.mismatchedPackages ?? 0)
    if (total > 0) {
      alertHighDisc.value = { show: true, count: total }
      showHighDiscAlert.value = true
    } else {
      alertHighDisc.value = { show: false, count: 0 }
      showHighDiscAlert.value = false
    }
  } catch (err) {
    console.error('Error fetching live stats:', err)
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
      text: 'Overall Discrepancy Types Distribution'
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
  await fetchLiveStats()
  await fetchChartData()
  await fetchLatestRuns()
}

onMounted(() => {
  fetchDashboardStats()
  fetchLiveStats()
  fetchChartData()
  fetchLatestRuns()
  fetchAlerts()
})
</script>

<template>

    <div class="container pt-4">

      <div class="row mb-4">
        <div class="col-12">
          <h3 class="fw-bold">Reconciliation Automation Dashboard</h3>
          <div class="text-muted">
            Monitor reconciliation runs, discrepancies, and completion status in one place.
          </div>
        </div>
      </div>

      <!-- Dashboard Alerts -->
      <div class="row mb-3" v-if="showInfoAlert || showFailedAlert || showHighDiscAlert">
        <div class="col-12">

          <!-- Info: schedule + email recipients -->
          <div v-if="showInfoAlert && alertInfo" class="alert alert-info alert-dismissible d-flex align-items-start gap-2 mb-2" role="alert">
            <i class="bi bi-info-circle-fill flex-shrink-0 mt-1"></i>
            <div>
              <strong>Scheduled Run Configured</strong><br>
              The active configuration has an automated schedule: <strong>{{ formatScheduleLabel(alertInfo) }}</strong>.<br>
              <span v-if="alertInfo.emailRecipients">
                Email notifications will be sent to: <strong>{{ alertInfo.emailRecipients }}</strong><br>
              </span>
              <span v-if="computeNextRun(alertInfo)">
                Next run will be on: <strong>{{ computeNextRun(alertInfo) }}</strong>
              </span>
            </div>
            <button type="button" class="btn-close ms-auto" aria-label="Close" @click="showInfoAlert = false"></button>
          </div>

          <!-- Error: last reconciliation failed -->
          <div v-if="showFailedAlert" class="alert alert-danger alert-dismissible d-flex align-items-start gap-2 mb-2" role="alert">
            <i class="bi bi-x-circle-fill flex-shrink-0 mt-1"></i>
            <div>
              <strong>Last Reconciliation Failed</strong><br>
              The most recent reconciliation run did not complete successfully. Please review the logs for details.
            </div>
            <button type="button" class="btn-close ms-auto" aria-label="Close" @click="showFailedAlert = false"></button>
          </div>

          <!-- Warning: high discrepancy count -->
          <div v-if="showHighDiscAlert" class="alert alert-warning alert-dismissible d-flex align-items-start gap-2 mb-2" role="alert">
            <i class="bi bi-exclamation-triangle-fill flex-shrink-0 mt-1"></i>
            <div>
              <strong>High Discrepancy Count Detected</strong><br>
              The current live data shows <strong>{{ alertHighDisc.count }}</strong> total discrepancies across all three categories (Missing in ROWB, Not Active in Siebel, Mismatched Packages). A reconciliation action should be considered.
            </div>
            <button type="button" class="btn-close ms-auto" aria-label="Close" @click="showHighDiscAlert = false"></button>
          </div>

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
                  <span v-if="isLoading" class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                  <i v-else class="bi bi-arrow-clockwise me-2" aria-hidden="true"></i>
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
                <BaseCard title="Missing in ROWB" headerClass="bg-light text-dark border" bodyClass="py-3 fs-4 fw-semibold">
                  {{ liveStats.missingInRowb }}
                </BaseCard>
              </div>
              <div class="col-md-4 mb-2">
                <BaseCard title="Not Active in Siebel" headerClass="bg-light text-dark border" bodyClass="py-3 fs-4 fw-semibold">
                  {{ liveStats.notActiveInSiebel }}
                </BaseCard>
              </div>
              <div class="col-md-4 mb-2">
                <BaseCard title="Mismatched packages" headerClass="bg-light text-dark border" bodyClass="py-3 fs-4 fw-semibold">
                  {{ liveStats.mismatchedPackages }}
                </BaseCard>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Charts Section -->
      <div class="row mb-4">
        <div class="col-12 col-lg-8 mb-4 mb-lg-0">
          <div class="visual-card h-100">
            <BaseChart :options="pieChartOptions" />
          </div>
        </div>
        <div class="col-12 col-lg-4">
          <div class="visual-card h-100">
            <div class="success-rate-panel text-center" style="min-height: 350px;">
              <h5 class="mb-2">Reconciliation Success Rate</h5>

              <div class="success-rate-ring-wrap flex-grow-1">
                <div class="success-rate-circle" :style="{ borderColor: getSuccessRateColor }">
                  <div class="success-rate-value" :style="{ color: getSuccessRateColor }">
                    {{ chartData.successRate.toFixed(1) }}%
                  </div>
                </div>
              </div>

              <div class="mt-2 text-muted">Successful Completion Rate</div>
            </div>
          </div>
        </div>
      </div>

      <div class="row mb-4">
        <div class="col-12">
          <div class="visual-card">
            <BaseChart :options="lineChartOptions" />
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
            <div class="text-start">
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


</template>

<style scoped>
/* small spacing tweak */
.display-6 { font-size: 2rem; }
.fst-italic { font-style: italic; }
.file-icon { }

.visual-card {
  background: #f8f9fa;
  border-radius: 8px;
  padding: 1rem 1.25rem;
}

.success-rate-panel {
  display: flex;
  flex-direction: column;
  justify-content: center;
  height: 100%;
}

.success-rate-ring-wrap {
  display: flex;
  align-items: center;
  justify-content: center;
}

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
