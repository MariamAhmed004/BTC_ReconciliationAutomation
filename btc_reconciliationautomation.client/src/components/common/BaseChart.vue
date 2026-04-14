<script setup>
import { onMounted, ref } from 'vue'
import Highcharts from 'highcharts'

const props = defineProps({
  options: { type: Object, default: () => ({}) }
})

const chartContainer = ref(null)

onMounted(() => {
  Highcharts.chart(chartContainer.value, props.options)
})
</script>

<template>
  <div ref="chartContainer" style="width:100%; height:400px"></div>
</template>

<style scoped></style>
