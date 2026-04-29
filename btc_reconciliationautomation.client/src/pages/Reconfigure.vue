<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import PageHeader from '../components/common/PageHeader.vue'
import ConfigField from '../components/common/ConfigField.vue'

const router = useRouter()

const formData = ref({
  emailRecipients: '',
  timeExecution: '',
  frequency: 'Monthly',
  daysOfMonth: '',
  logRetentionDays: '',
  filePath: '',
  ignoreFieldsWith: ''
})

const saving = ref(false)

async function saveConfiguration() {
  saving.value = true
  try {
    // TODO: Implement API call to save configuration
    console.log('Saving configuration:', formData.value)

    // Simulate API call
    await new Promise(resolve => setTimeout(resolve, 1500))

    // Navigate back to management page after successful save
    router.push('/management')
  } catch (err) {
    console.error('Error saving configuration:', err)
    alert('Failed to save configuration')
  } finally {
    saving.value = false
  }
}

function cancel() {
  router.push('/management')
}
</script>

<template>
  <div class="container pt-4 ">
    <PageHeader
      title="Override Process Configurations"
      subtitle="Rewrite reconciliation process configurations using the following fields:"
      instruction="New configuration will deactivate previous active configuration"
    >
      <template #icon>
        <i class="bi bi-arrow-left-square-fill" style="font-size: 2rem; color: #495057; cursor: pointer;" @click="cancel"></i>
      </template>
    </PageHeader>

    <div class="row justify-content-start mt-4">
      <div class="col-12 col-md-10 col-lg-8 ms-lg-4 ms-md-3 ms-2">
          <form @submit.prevent="saveConfiguration" class="config-form">
            <ConfigField
              v-model="formData.emailRecipients"
              label="Email to send files to"
              icon="bi-envelope-fill"
              type="email"
              infoText="Enter the email address(es) where reconciliation reports will be sent. Separate multiple emails with commas."
            />

            <ConfigField
              v-model="formData.timeExecution"
              label="Time Execution"
              icon="bi-clock-fill"
              type="time"
              infoText="Set the time when the reconciliation process should run daily."
            />

            <ConfigField
              v-model="formData.frequency"
              label="Frequency"
              icon="bi-arrow-left-right"
              type="text"
              infoText="Specify how often the reconciliation should run (e.g., Daily, Weekly, Monthly)."
            />

            <ConfigField
              v-model="formData.daysOfMonth"
              label="Days of Month"
              icon="bi-calendar-date-fill"
              placeholder="25 28 30"
              type="text"
              infoText="Enter the days of the month when reconciliation should run. Separate multiple days with spaces (e.g., 1 15 30)."
            />

            <ConfigField
              v-model="formData.logRetentionDays"
              label="Clear System Logs after (days)"
              icon="bi-trash-fill"
              placeholder="30"
              type="number"
              infoText="Number of days to retain system logs before automatic deletion. Default is 30 days."
            />

            <ConfigField
              v-model="formData.filePath"
              label="Store files under the path"
              icon="bi-folder-fill"
              type="text"
              infoText="Specify the directory path where reconciliation output files should be stored."
            />

            <ConfigField
              v-model="formData.ignoreFieldsWith"
              label="Ignore fields with"
              icon="bi-dash-circle-fill"
              type="text"
              infoText="Enter field names or patterns to exclude from reconciliation. Separate multiple values with commas."
            />

            <div class="d-flex gap-3 justify-content-end mt-4 pt-3 border-top buttons-row">
              <button
                type="button"
                class="btn btn-outline-secondary"
                @click="cancel"
                :disabled="saving"
              >
                Cancel
              </button>
              <button
                type="submit"
                class="btn btn-primary"
                :disabled="saving"
              >
                <span v-if="saving" class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                {{ saving ? 'Saving...' : 'Save Configuration' }}
              </button>
            </div>
          </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
.config-form {
  padding: 1rem 0;
}

.config-form :deep(.form-control) {
  background-color: #f0f0f0;
}

.bi {
  font-size: inherit;
}

.buttons-row {
  position: fixed;
  bottom: 2rem;
  right: 2.5rem;
  width: auto;
  border-top: none !important;
  padding-top: 0 !important;
  margin-top: 0 !important;
  z-index: 100;
}
</style>
