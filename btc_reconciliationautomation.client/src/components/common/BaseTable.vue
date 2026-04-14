<script setup>
const props = defineProps({
  columns: { type: Array, default: () => [] },
  items: { type: Array, default: () => [] },
  tableClass: { type: String, default: '' }
})
</script>

<template>
  <div class="base-table">
    <div class="table-responsive">
      <table :class="['table align-middle mb-0', props.tableClass]">
        <thead>
          <tr class="table-header table-dark">
            <th v-for="col in props.columns" :key="col.key" :style="{ width: col.width || 'auto' }">
              {{ col.title }}
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in props.items" :key="item.id || item.key">
            <td v-for="col in props.columns" :key="col.key">
              <slot :name="col.key" :item="item">
                <template v-if="col.render === 'timestamp'">
                  <div class="timestamp">
                    <div class="time">{{ item[col.key] }}</div>
                  </div>
                </template>
                <template v-else-if="col.render === 'status'">
                  <span v-if="item[col.key] === 'ok' || item[col.key] === true" class="status-badge success">✓</span>
                  <span v-else class="status-badge danger">✕</span>
                </template>
                <template v-else>
                  {{ item[col.key] }}
                </template>
              </slot>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style scoped>
.table-header {
  font-weight: 700;
}

.status-badge {
  display: inline-flex;
  width: 28px;
  height: 28px;
  border-radius: 50%;
  align-items: center;
  justify-content: center;
  color: #fff;
  font-weight: 700;
}
.status-badge.success { background: #28a745; }
.status-badge.danger { background: #dc3545; }

.timestamp .time {
  text-decoration: underline;
  color: #000;
}

.base-table table {
  border: 1px solid #cfcfcf;
}

.base-table td, .base-table th {
  vertical-align: middle;
  padding: 0.9rem 1rem;
}
</style>
