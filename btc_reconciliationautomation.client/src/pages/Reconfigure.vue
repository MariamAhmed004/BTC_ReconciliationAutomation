<script setup>
import { ref, computed, watch } from 'vue'
import { useRouter } from 'vue-router'
import PageHeader from '../components/common/PageHeader.vue'
import ConfigField from '../components/common/ConfigField.vue'
import BaseModal from '../components/common/BaseModal.vue'

const router = useRouter()

const formData = ref({
  emailRecipients: '',
  timeExecution: '',
  frequency: '',
  dayOfMonth: '',
  logRetentionDays: '',
  filePath: '',
  ignoreFieldsWith: ''
})

const saving = ref(false)
const errorMessage = ref('')
const fieldErrors = ref({})

const showWarningModal = ref(false)
const showConfirmModal = ref(false)
const showEmailModal = ref(false)
const showFreqModal = ref(false)
const showDayModal = ref(false)
const showFilePathModal = ref(false)

const showIgnoreModal = ref(false)
const isMonthly = computed(() => formData.value.frequency === 'Monthly')
const isWeekly  = computed(() => formData.value.frequency === 'Weekly')
const isDaily   = computed(() => formData.value.frequency === 'Daily')

const dayLabel = computed(() => {
  if (isMonthly.value) return 'Day of Month'
  if (isWeekly.value)  return 'Day of Week'
  return 'Day'
})

const dayPlaceholder = computed(() => {
  if (isMonthly.value) return 'e.g. 5, 15, 28'
  if (isWeekly.value)  return 'e.g. Monday, Wednesday'
  return 'Select a frequency first'
})

const dayInfoText = computed(() => {
  if (isMonthly.value)
    return 'Enter one or more days of the month (1–31) on which the reconciliation should run, separated by commas (e.g. 5, 15, 28). These values are used to configure the scheduled Linux run, so each entry must be a valid calendar day number. If a specified day does not exist in a given month (e.g. 31 in February), that occurrence will be skipped by the scheduler.'
  if (isWeekly.value)
    return 'Enter one or more days of the week on which the reconciliation should run, separated by commas (e.g. Monday, Wednesday). These values are used to configure the scheduled Linux run. Day names must be spelled in full and in English.'
  return 'Select a frequency above to unlock this field.'
})

watch(() => formData.value.frequency, () => {
  formData.value.dayOfMonth = ''
  delete fieldErrors.value.dayOfMonth
})

// ── Validation ─────────────────────────────────────────────────────
const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

function validateForm() {
  const errs = {}

  const anyFilled = Object.values(formData.value).some(v => v !== '' && v !== null && v !== undefined)
  if (!anyFilled) {
    errs._global = 'Please fill in at least one field before saving a new configuration.'
  }

  if (formData.value.emailRecipients) {
    const raw = formData.value.emailRecipients.trim()
    if (raw.includes(',') || raw.includes(';')) {
      errs.emailRecipients = 'Only one email address is allowed.'
    } else if (!emailRegex.test(raw)) {
      errs.emailRecipients = 'Please enter a valid email address (e.g. user@example.com).'
    }
  }

  const hasFreq  = !!formData.value.frequency
  const hasTime  = !!formData.value.timeExecution
  const hasDay   = !!formData.value.dayOfMonth
  const needsDay = isMonthly.value || isWeekly.value

  if (hasFreq || hasTime) {
    if (!hasFreq) errs.frequency     = 'Frequency is required when Time Execution is set.'
    if (!hasTime) errs.timeExecution = 'Time Execution is required when Frequency is set.'
    if (needsDay && !hasDay) errs.dayOfMonth = `${dayLabel.value} is required for ${formData.value.frequency} schedules.`
  }

  if (formData.value.logRetentionDays) {
    const n = Number(formData.value.logRetentionDays)
    if (!Number.isInteger(n) || n < 1) {
      errs.logRetentionDays = 'Log retention must be a positive whole number of days.'
    }
  }

  fieldErrors.value = errs
  return Object.keys(errs).length === 0
}

// ── Submit flow ────────────────────────────────────────────────────
const hasScheduling = computed(() =>
  !!(formData.value.frequency && formData.value.timeExecution)
)

function handleSubmit() {
  if (!validateForm()) return

  const hasEmail = !!formData.value.emailRecipients.trim()
  if (hasEmail && !hasScheduling.value) {
    showWarningModal.value = true
    return
  }

  showConfirmModal.value = true
}

function proceedFromWarning() {
  showWarningModal.value = false
  showConfirmModal.value = true
}

async function confirmSave() {
  showConfirmModal.value = false
  saving.value = true
  errorMessage.value = ''
  try {
    let addedBy = 'Admin'
    try {
      const meRes = await fetch('/api/auth/me', { credentials: 'include' })
      if (meRes.ok) {
        const me = await meRes.json()
        const fullName = [me.firstName, me.lastName].filter(Boolean).join(' ')
        addedBy = fullName || me.email || me.userName || 'Admin'
      }
    } catch { /* fallback */ }

    const payload = {
      EMAIL_RECIPIENTS: formData.value.emailRecipients.trim() || null,
      RUN_TIME:         formData.value.timeExecution || null,
      FREQUENCY:        formData.value.frequency ? formData.value.frequency.toUpperCase() : null,
      DAY_OF_MONTH:     formData.value.dayOfMonth || null,
      DAYS_TO_DELETE_AUDITLOGS: formData.value.logRetentionDays
        ? parseFloat(formData.value.logRetentionDays) : null,
      DEFAULT_FILE_PATH:  formData.value.filePath.trim() || null,
      IGNORE_CONDITIONS:  formData.value.ignoreFieldsWith.trim() || null,
      ADDED_BY: addedBy
    }

    const response = await fetch('/api/Configuration', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    })

    if (!response.ok) {
      const text = await response.text()
      throw new Error(text || `Server returned ${response.status}`)
    }

    router.push('/management')
  } catch (err) {
    console.error('Error saving configuration:', err)
    errorMessage.value = err.message || 'Failed to save configuration'
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
      subtitle="These settings directly control the reconciliation process running on the Linux server. Changes become active on the next scheduled or manual run."
      instruction="Click the ℹ icon next to each field to open detailed guidance for that field. All fields are individually optional, but at least one must be filled. Scheduling fields (Frequency + Time Execution) must always be configured together."
    >
      <template #icon>
        <i
          class="bi bi-arrow-left-square-fill"
          style="font-size: 2rem; color: #495057; cursor: pointer;"
          @click="cancel"
        ></i>
      </template>
    </PageHeader>

    <div class="row justify-content-start mt-4">
      <div class="col-12 col-md-10 col-lg-8 ms-lg-4 ms-md-3 ms-2">
        <form @submit.prevent="handleSubmit" class="config-form" novalidate>

          <!-- Global error -->
          <div v-if="fieldErrors._global" class="alert alert-danger py-2 mb-3">
            <i class="bi bi-exclamation-triangle-fill me-2"></i>{{ fieldErrors._global }}
          </div>

          <!-- Email -->
          <div class="config-field-wrapper mb-3">
            <label class="field-label mb-1">Email to send files to</label>
            <div class="d-flex align-items-center gap-2">
              <div class="field-icon">
                <i class="bi bi-envelope-fill"></i>
              </div>
              <div class="field-input flex-grow-1">
                <input
                  v-model="formData.emailRecipients"
                  type="email"
                  class="form-control"
                  placeholder="e.g. recon-team@company.com"
                />
              </div>
              <div class="field-info">
                <button
                  type="button"
                  class="btn btn-link p-0 info-btn"
                  title="About this field"
                  @click="showEmailModal = true"
                >
                  <i class="bi bi-info-circle"></i>
                </button>
              </div>
            </div>
          </div>
          <div v-if="fieldErrors.emailRecipients" class="field-error mb-3">
            <i class="bi bi-exclamation-circle me-1"></i>{{ fieldErrors.emailRecipients }}
          </div>

          <!-- Time Execution -->
          <ConfigField
            v-model="formData.timeExecution"
            label="Time Execution"
            icon="bi-clock-fill"
            type="time"
            infoText="Set the time of day (24-hour clock) when the reconciliation process should automatically trigger on the Linux server. Must be configured together with Frequency."
          />
          <div v-if="fieldErrors.timeExecution" class="field-error mb-3">
            <i class="bi bi-exclamation-circle me-1"></i>{{ fieldErrors.timeExecution }}
          </div>

          <!-- Frequency (select) -->
          <div class="config-field-wrapper mb-3">
            <div class="d-flex align-items-center gap-2">
              <div class="field-icon">
                <i class="bi bi-arrow-left-right"></i>
              </div>
              <div class="flex-grow-1">
                <label class="field-label mb-1">Frequency</label>
                <select v-model="formData.frequency" class="form-select field-input">
                  <option value="">— Select frequency —</option>
                  <option value="Daily">Daily</option>
                  <option value="Weekly">Weekly</option>
                  <option value="Monthly">Monthly</option>
                </select>
              </div>
              <button
                type="button"
                class="btn btn-link p-0 info-btn align-self-end mb-1"
                title="About Frequency"
                @click="showFreqModal = true"
              >
                <i class="bi bi-info-circle"></i>
              </button>
            </div>
          </div>
          <div v-if="fieldErrors.frequency" class="field-error mb-3">
            <i class="bi bi-exclamation-circle me-1"></i>{{ fieldErrors.frequency }}
          </div>

          <!-- Day of Month / Week (conditional) -->
          <div class="config-field-wrapper mb-3">
            <div class="d-flex align-items-center gap-2">
              <div class="field-icon" :class="{ 'opacity-40': isDaily || !formData.frequency }">
                <i class="bi bi-calendar-date-fill"></i>
              </div>
              <div class="flex-grow-1">
                <label class="field-label mb-1">
                  {{ dayLabel }}
                  <span v-if="isDaily" class="badge bg-secondary fw-normal ms-2" style="font-size:0.7rem;">Not used for Daily</span>
                </label>
                <input
                  v-model="formData.dayOfMonth"
                  type="text"
                  class="form-control field-input"
                  :placeholder="isDaily || !formData.frequency ? 'Not applicable' : dayPlaceholder"
                  :disabled="isDaily || !formData.frequency"
                />
              </div>
              <button
                type="button"
                class="btn btn-link p-0 info-btn align-self-end mb-1"
                :disabled="isDaily || !formData.frequency"
                title="About this field"
                @click="showDayModal = true"
              >
                <i class="bi bi-info-circle"></i>
              </button>
            </div>
          </div>
          <div v-if="fieldErrors.dayOfMonth" class="field-error mb-3">
            <i class="bi bi-exclamation-circle me-1"></i>{{ fieldErrors.dayOfMonth }}
          </div>

          <!-- Log Retention -->
          <ConfigField
            v-model="formData.logRetentionDays"
            label="Clear System Logs after (days)"
            icon="bi-trash-fill"
            placeholder="e.g. 30"
            type="number"
            infoText="Number of days to keep audit log records before they are automatically purged. Enter a positive whole number. Recommended: 30 days. Setting this too low may delete logs needed for troubleshooting."
          />
          <div v-if="fieldErrors.logRetentionDays" class="field-error mb-3">
            <i class="bi bi-exclamation-circle me-1"></i>{{ fieldErrors.logRetentionDays }}
          </div>

          <!-- File Path -->
          <div class="config-field-wrapper mb-3">
            <label class="field-label mb-1">
              Store files under the path
              <span class="attention-badge ms-2">
                <i class="bi bi-exclamation-triangle-fill text-warning me-1" style="font-size:0.75rem;"></i>
                <span class="text-warning" style="font-size:0.72rem; font-weight:600;">Read field notes</span>
              </span>
            </label>
            <div class="d-flex align-items-center gap-2">
              <div class="field-icon">
                <i class="bi bi-folder-fill"></i>
              </div>
              <div class="field-input flex-grow-1">
                <input
                  v-model="formData.filePath"
                  type="text"
                  class="form-control"
                  placeholder="e.g. RECON_OUTPUT_DIR"
                />
              </div>
              <div class="field-info">
                <button
                  type="button"
                  class="btn btn-link p-0 info-btn"
                  title="Important: Read before filling"
                  @click="showFilePathModal = true"
                >
                  <i class="bi bi-info-circle"></i>
                </button>
              </div>
            </div>
          </div>

          <!-- Ignore Fields (expandable textarea) -->
          <div class="config-field-wrapper mb-3">
            <label class="field-label mb-1">
              Ignore fields with
              <span class="attention-badge ms-2">
                <i class="bi bi-exclamation-triangle-fill text-warning me-1" style="font-size:0.75rem;"></i>
                <span class="text-warning" style="font-size:0.72rem; font-weight:600;">Read field notes</span>
              </span>
            </label>
            <div class="d-flex align-items-start gap-2">
              <div class="field-icon mt-1">
                <i class="bi bi-dash-circle-fill"></i>
              </div>
              <div class="field-input flex-grow-1">
                <textarea
                  v-model="formData.ignoreFieldsWith"
                  class="form-control"
                  rows="3"
                  style="resize: vertical; min-height: 80px;"
                  placeholder="e.g. 000, Skip, eFax"
                ></textarea>
              </div>
              <div class="field-info">
                <button
                  type="button"
                  class="btn btn-link p-0 info-btn mt-1"
                  title="Important: Read before filling"
                  @click="showIgnoreModal = true"
                >
                  <i class="bi bi-info-circle"></i>
                </button>
              </div>
            </div>
          </div>

          <!-- Save error -->
          <div v-if="errorMessage" class="alert alert-danger mt-3" role="alert">
            <i class="bi bi-x-circle-fill me-2"></i>{{ errorMessage }}
          </div>

          <div class="d-flex gap-3 justify-content-end mt-4 pt-3 border-top mb-5 pb-3 buttons-row">
            <button type="button" class="btn btn-outline-secondary" @click="cancel" :disabled="saving">
              Cancel
            </button>
            <button type="submit" class="btn btn-primary" :disabled="saving">
              <span v-if="saving" class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
              {{ saving ? 'Saving...' : 'Save Configuration' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Email info modal -->
    <BaseModal
      :show="showEmailModal"
      title="About: Email to Send Files To"
      size="md"
      :buttons="[{ text: 'Got it', variant: 'primary', action: 'close' }]"
      @close="showEmailModal = false"
      @action="showEmailModal = false"
    >
      <div class="modal-info-body">
        <p class="mb-3">
          This field specifies the recipient email address to which the reconciliation output files will be delivered upon each completed automatic run.
        </p>

        <div class="info-note mb-3">
          <i class="bi bi-person-check-fill me-2"></i>
          <strong>One address only.</strong> Only a single recipient address is accepted per configuration. This is by design — the generated files are intended for upload into an external system, and delivering them to multiple recipients may introduce inconsistencies in how they are processed or submitted.
        </div>

        <p class="mb-2 small">If multiple stakeholders require access to the output files, it is recommended to use a <strong>designated shared mailbox or distribution list</strong> as the single configured recipient.</p>

        <div class="info-warn mt-3">
          <i class="bi bi-exclamation-triangle-fill me-2"></i>
          <strong>Format validation is enforced.</strong> The address must follow a standard email format — it must contain both an <code>@</code> symbol and a domain with a <code>.</code> (e.g. <code>user@company.com</code>). Addresses that do not meet this format will be rejected on submission.
        </div>
      </div>
    </BaseModal>

    <!-- Frequency info modal -->
    <BaseModal
      :show="showFreqModal"
      title="About: Frequency"
      size="md"
      :buttons="[{ text: 'Got it', variant: 'primary', action: 'close' }]"
      @close="showFreqModal = false"
      @action="showFreqModal = false"
    >
      <div class="modal-info-body">
        <p class="mb-3">Choose how often the reconciliation process should run automatically on the Linux server.</p>
        <table class="table table-sm table-bordered mb-3">
          <thead class="table-light">
            <tr><th>Option</th><th>Behaviour</th></tr>
          </thead>
          <tbody>
            <tr><td><strong>Daily</strong></td><td>Runs every day at the configured time. The day field is not required.</td></tr>
            <tr><td><strong>Weekly</strong></td><td>Runs once a week on the day you specify (e.g. Monday).</td></tr>
            <tr><td><strong>Monthly</strong></td><td>Runs once a month on the date you specify (e.g. the 5th).</td></tr>
          </tbody>
        </table>
        <div class="info-note">
          <i class="bi bi-link-45deg me-1"></i>
          Frequency must always be set <strong>together with Time Execution</strong>. Setting one without the other will be flagged as a validation error.
        </div>
      </div>
    </BaseModal>

    <!-- Day field info modal -->
    <BaseModal
      :show="showDayModal"
      title="About: Schedule Day"
      size="md"
      :buttons="[{ text: 'Got it', variant: 'primary', action: 'close' }]"
      @close="showDayModal = false"
      @action="showDayModal = false"
    >
      <div class="modal-info-body">
        <p class="mb-3">{{ dayInfoText }}</p>
        <table class="table table-sm table-bordered mb-3">
          <thead class="table-light">
            <tr><th>Frequency</th><th>What to enter</th><th>Example</th></tr>
          </thead>
          <tbody>
            <tr><td>Weekly</td><td>Full day name(s), comma-separated</td><td>Monday, Wednesday</td></tr>
            <tr><td>Monthly</td><td>Day number(s) 1–31, comma-separated</td><td>5, 15, 28</td></tr>
            <tr><td>Daily</td><td colspan="2" class="text-muted">Field is disabled — not needed</td></tr>
          </tbody>
        </table>
        <div class="info-note">
          <i class="bi bi-info-circle me-1"></i>
          Multiple days are supported and must be separated by commas. These values directly configure the Linux scheduler — ensure entries are valid and correctly formatted before saving.
        </div>
      </div>
    </BaseModal>

    <!-- File Path info modal -->
    <BaseModal
      :show="showFilePathModal"
      title="About: Store Files Under the Path"
      size="md"
      :buttons="[{ text: 'Understood', variant: 'primary', action: 'close' }]"
      @close="showFilePathModal = false"
      @action="showFilePathModal = false"
    >
      <div class="modal-info-body">
        <p class="mb-3">
          This field specifies <strong>where the reconciliation output files will be saved</strong> on the Linux server after each run.
        </p>
        <div class="info-warn mb-3">
          <i class="bi bi-exclamation-triangle-fill me-2"></i>
          <strong>This is not a regular folder path.</strong> You must enter the name of a pre-configured <strong>Oracle Directory Object</strong> — not a file-system path like <code>/home/oracle/files</code>.
        </div>
        <p class="mb-2 small">The reconciliation Oracle procedure resolves this name to an actual server path internally. For the procedure to write files successfully:</p>
        <ul class="small mb-3">
          <li>The Oracle Directory Object must already exist on the database server.</li>
          <li>It must have been granted <code>READ</code> and <code>WRITE</code> privileges to the Oracle user running the procedure.</li>
          <li>The underlying OS directory on the Linux server must exist and be accessible.</li>
        </ul>
        <div class="info-note">
          <i class="bi bi-person-fill me-1"></i>
          If the directory object has not been set up yet, contact your DBA before filling this field. Entering an incorrect or non-existent name will cause file output to fail silently during the run.
        </div>
      </div>
    </BaseModal>

    <!-- Ignore Fields info modal -->
    <BaseModal
      :show="showIgnoreModal"
      title="About: Ignore Fields With"
      size="md"
      :buttons="[{ text: 'Understood', variant: 'primary', action: 'close' }]"
      @close="showIgnoreModal = false"
      @action="showIgnoreModal = false"
    >
      <div class="modal-info-body">
        <p class="mb-3">
          This field lets you define <strong>reference prefixes to exclude from reconciliation comparisons</strong>. Records matching these prefixes will be skipped during the run and will not appear as discrepancies.
        </p>
        <p class="mb-2 small"><strong>Format:</strong> Enter values separated by commas.</p>
        <div class="example-box mb-3">
          <span class="example-label">Example</span>
          <code>000, Skip, eFax</code>
          <p class="mb-0 mt-1 small text-muted">Any transaction whose ROWB reference starts with <em>000</em>, <em>Skip</em>, or <em>eFax</em> will be ignored.</p>
        </div>
        <p class="mb-2 small">Typical use cases include:</p>
        <ul class="small mb-3">
          <li>Internal transfers or system-generated entries not expected to match.</li>
          <li>Legacy placeholder records with known dummy references.</li>
          <li>Test transactions that should not be flagged as discrepancies.</li>
        </ul>
        <div class="info-warn">
          <i class="bi bi-exclamation-triangle-fill me-2"></i>
          <strong>Caution:</strong> Incorrect or overly broad ignore conditions can silently exclude legitimate discrepancies from being reported. Review these values carefully with the reconciliation team before saving.
        </div>
      </div>
    </BaseModal>

    <!-- Warning modal: email set but no schedule -->
    <BaseModal
      :show="showWarningModal"
      title="No Scheduling Configured"
      :buttons="[
        { text: 'Go Back & Add Schedule', variant: 'secondary', action: 'close' },
        { text: 'Save Anyway', variant: 'warning', action: 'proceed' }
      ]"
      @close="showWarningModal = false"
      @action="(a) => { if (a === 'proceed') proceedFromWarning() }"
    >
      <div class="modal-info-body">
        <div class="info-warn mb-3">
          <i class="bi bi-exclamation-triangle-fill me-2"></i>
          You have entered an email address but have <strong>not configured any scheduling</strong> (Frequency and Time Execution).
        </div>
        <p class="mb-2 small">Without a schedule, the reconciliation process will <strong>never run automatically</strong> and no report will ever be sent to the email address you provided.</p>
        <p class="mb-0 small text-muted">Are you sure you want to save this configuration without scheduling?</p>
      </div>
    </BaseModal>

    <!-- Confirmation modal -->
    <BaseModal
      :show="showConfirmModal"
      title="Confirm New Configuration"
      :buttons="[
        { text: 'Cancel', variant: 'secondary', action: 'close' },
        { text: 'Confirm & Save', variant: 'primary', action: 'confirm' }
      ]"
      @close="showConfirmModal = false"
      @action="(a) => { if (a === 'confirm') confirmSave() }"
    >
      <div class="modal-info-body">
        <div class="info-note mb-3">
          <i class="bi bi-info-circle-fill me-2"></i>
          Saving will create a <strong>new active configuration</strong> and <strong>deactivate the current one</strong>.
        </div>
        <p class="mb-2 small">The previous configuration will be retained in the system for historical reference but will no longer be active.</p>
        <p class="mb-0 small text-muted">This change takes effect on the next scheduled or manual reconciliation run on the Linux server.</p>
      </div>
    </BaseModal>
  </div>
</template>

<style scoped>
.config-form {
  padding: 1rem 0;
}

.config-form :deep(.form-control),
.config-form :deep(.form-select) {
  background-color: #f0f0f0;
}

.config-field-wrapper {
  display: flex;
  flex-direction: column;
}

.field-icon {
  width: 48px;
  height: 48px;
  min-width: 48px;
  background: #e9ecef;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.4rem;
  color: #495057;
  transition: opacity 0.2s;
}

.opacity-40 {
  opacity: 0.4;
}

.field-label {
  display: block;
  font-size: 0.875rem;
  font-weight: 600;
  color: #495057;
}

.field-input {
  width: 100%;
}

.info-btn {
  color: #6c757d;
  font-size: 1.2rem;
  line-height: 1;
  flex-shrink: 0;
}

.info-btn:disabled {
  opacity: 0.3;
  pointer-events: none;
}

.attention-badge {
  display: inline-flex;
  align-items: center;
  vertical-align: middle;
}

.field-error {
  font-size: 0.82rem;
  color: #dc3545;
  padding-left: 0.25rem;
}

/* ── Modal content helpers ── */
.modal-info-body {
  font-size: 0.9rem;
  line-height: 1.6;
}

.info-note {
  background: #e8f4fd;
  border-left: 3px solid #0d6efd;
  border-radius: 4px;
  padding: 0.6rem 0.85rem;
  font-size: 0.83rem;
  color: #0a3760;
}

.info-warn {
  background: #fff8e1;
  border-left: 3px solid #ffc107;
  border-radius: 4px;
  padding: 0.6rem 0.85rem;
  font-size: 0.83rem;
  color: #5a4000;
}

.example-box {
  background: #f8f9fa;
  border: 1px dashed #ced4da;
  border-radius: 4px;
  padding: 0.6rem 0.85rem;
  position: relative;
}

.example-label {
  display: inline-block;
  font-size: 0.7rem;
  font-weight: 700;
  text-transform: uppercase;
  color: #6c757d;
  margin-bottom: 0.25rem;
  letter-spacing: 0.04em;
}

.bi {
  font-size: inherit;
}

.buttons-row {
  width: auto;
}
</style>
