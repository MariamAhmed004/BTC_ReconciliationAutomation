<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import PageHeader from '../components/common/PageHeader.vue'
import BaseCard from '../components/common/BaseCard.vue'
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
  <div class="container pt-4">
    <PageHeader
      title="Override Process Configurations"
      subtitle="Rewrite reconciliation process configurations using the following fields:"
      instruction="New configuration will deactivate previous active configuration"
    >
      <template #icon>
        <i class="bi bi-gear-fill" style="font-size: 2rem; color: #495057;"></i>
      </template>
    </PageHeader>

    <div class="row justify-content-center mt-4">
      <div class="col-lg-8 col-xl-7">
        <BaseCard>
          <form @submit.prevent="saveConfiguration" class="config-form">
            <ConfigField
              v-model="formData.emailRecipients"
              label="Email to send files to"
              icon="bi-envelope-fill"
              placeholder="example@btc.com.bh"
              type="email"
              infoText="Enter the email address(es) where reconciliation reports will be sent. Separate multiple emails with commas."
            />

            <ConfigField
              v-model="formData.timeExecution"
              label="Time Execution"
              icon="bi-clock-fill"
              placeholder="HH:MM"
              type="time"
              infoText="Set the time when the reconciliation process should run daily."
            />

            <ConfigField
              v-model="formData.frequency"
              label="Frequency: Monthly"
              icon="bi-arrow-repeat"
              placeholder="Monthly"
              type="text"
              infoText="Specify how often the reconciliation should run (e.g., Daily, Weekly, Monthly)."
            />

            <ConfigField
              v-model="formData.daysOfMonth"
              label="Days of Month"
              icon="bi-calendar-fill"
              placeholder="25 28 30"
              type="text"
              infoText="Enter the days of the month when reconciliation should run. Separate multiple days with spaces (e.g., 1 15 30)."
            />

            <ConfigField
              v-model="formData.logRetentionDays"
              label="Clear System Logs after: XX days"
              icon="bi-trash-fill"
              placeholder="30"
              type="number"
              infoText="Number of days to retain system logs before automatic deletion. Default is 30 days."
            />

            <ConfigField
              v-model="formData.filePath"
              label="Store files under the path"
              icon="bi-folder-fill"
              placeholder="/path/to/files"
              type="text"
              infoText="Specify the directory path where reconciliation output files should be stored."
            />

            <ConfigField
              v-model="formData.ignoreFieldsWith"
              label="Ignore fields with"
              icon="bi-dash-circle-fill"
              placeholder="Enter keywords"
              type="text"
              infoText="Enter field names or patterns to exclude from reconciliation. Separate multiple values with commas."
            />

            <div class="d-flex gap-3 justify-content-end mt-4 pt-3 border-top">
              <button
                type="button"
                class="btn btn-lg btn-outline-secondary"
                @click="cancel"
                :disabled="saving"
              >
                Cancel
              </button>
              <button
                type="submit"
                class="btn btn-lg btn-primary"
                :disabled="saving"
              >
                <span v-if="saving" class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                {{ saving ? 'Saving...' : 'Save Configuration' }}
              </button>
            </div>
          </form>
        </BaseCard>
      </div>
    </div>
  </div>
</template>

<style scoped>
.config-form {
  padding: 1rem 0;
}

.bi {
  font-size: inherit;
}
</style>
