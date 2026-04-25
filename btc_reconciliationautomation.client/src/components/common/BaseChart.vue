<script setup>
import { onMounted, ref, watch } from 'vue'
import Highcharts from 'highcharts'

const props = defineProps({
  options: { type: Object, default: () => ({}) }
})

const chartContainer = ref(null)
let chart = null

onMounted(() => {
  if (chartContainer.value) {
    chart = Highcharts.chart(chartContainer.value, props.options)
  }
})

// Watch for changes in options and update the chart
watch(() => props.options, (newOptions) => {
  if (chart) {
    chart.update(newOptions, true)
  } else if (chartContainer.value) {
    chart = Highcharts.chart(chartContainer.value, newOptions)
  }
}, { deep: true })
</script>

<template>
  <div ref="chartContainer" style="width:100%; height:400px"></div>
</template>

<style scoped></style>
