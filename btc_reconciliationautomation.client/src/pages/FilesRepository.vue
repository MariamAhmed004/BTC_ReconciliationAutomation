<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import PageHeader from '@/components/common/PageHeader.vue'
import BaseTable from '../components/common/BaseTable.vue'

const router = useRouter()

// Table columns configuration
const columns = [
  { key: 'fileName', title: 'File Name', width: '30%' },
  { key: 'createdAt', title: 'Created At', width: '20%', render: 'timestamp' },
  { key: 'fileType', title: 'File Type', width: '25%' },
  { key: 'actions', title: 'Actions', width: '25%' }
]

// Reactive state
const items = ref([])
const fileTypes = ref([])
const loading = ref(false)

function getFileTypeBadgeClass(fileType) {
  const t = String(fileType || '').trim().toUpperCase()

  if (t.includes('UPDATE') && t.includes('INCORRECT') && t.includes('PACKAGE')) return 'filetype-update-incorrect-packages'
  if (t.includes('DEACTIVATE') && t.includes('RECORD')) return 'filetype-deactivate-records'
  if (t.includes('LINK') && t.includes('RECORD') && t.includes('PACKAGE')) return 'filetype-link-records-to-packages'
  if (t.includes('ADD') && t.includes('NEW') && t.includes('RECORD')) return 'filetype-add-new-records'

  return 'filetype-other'
}

/**
 * Format date to locale string
 */
function formatDate(d) {
  if (!d) return ''
  try {
    const date = new Date(d)
    return date.toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: true
    })
  } catch {
    return String(d)
  }
}

/**
 * Load files from the API
 */
async function loadFiles() {
  loading.value = true

  try {
    const res = await fetch('/api/File/latest?count=100')
    if (res.ok) {
      const data = await res.json()
      items.value = (data || []).map(i => ({
        id: i.filE_ID ?? i.FILE_ID,
        runId: i.ruN_ID ?? i.RUN_ID,
        fileName: i.filE_NAME ?? i.FILE_NAME ?? 'N/A',
        createdAt: formatDate(i.createD_AT ?? i.CREATED_AT),
        createdAtRaw: i.createD_AT ?? i.CREATED_AT, // Keep raw for sorting
        fileType: i.filE_TYPE?.filE_TYPE_NAME ?? i.FILE_TYPE?.FILE_TYPE_NAME ?? 'Unknown',
        status: i.ruN?.ruN_STATUS?.ruN_STATUS1 ?? i.RUN?.RUN_STATUS?.RUN_STATUS1 ?? 'unknown',
        canDelete: canDeleteFile(i),
        canDownload: canDownloadFile(i),
        canNavigate: !!(i.ruN_ID ?? i.RUN_ID)
      }))
    } else {
      console.error('Failed to load files')
      loadSampleData()
    }
  } catch (e) {
    console.error('Failed to load files:', e)
    loadSampleData()
  } finally {
    loading.value = false
  }
}

/**
 * Load sample/fallback data
 */
function loadSampleData() {
  items.value = [
    {
      id: 1,
      runId: 101,
      fileName: '32_Ali_Hassan.xml',
      createdAt: formatDate('2025-01-15T11:23:04'),
      createdAtRaw: '2025-01-15T11:23:04',
      fileType: 'XML',
      status: 'completed',
      canDelete: true,
      canDownload: true,
      canNavigate: true
    },
    {
      id: 2,
      runId: 102,
      fileName: '12_Sarah_Ahmed.xml',
      createdAt: formatDate('2025-01-15T11:20:10'),
      createdAtRaw: '2025-01-15T11:20:10',
      fileType: 'XML',
      status: 'failed',
      canDelete: false,
      canDownload: true,
      canNavigate: true
    },
    {
      id: 3,
      runId: 103,
      fileName: '18_Dana_Mohammed.xml',
      createdAt: formatDate('2025-01-14T09:05:30'),
      createdAtRaw: '2025-01-14T09:05:30',
      fileType: 'XML',
      status: 'completed',
      canDelete: true,
      canDownload: true,
      canNavigate: true
    }
  ]
}

/**
 * Load file types from the API
 */
async function loadFileTypes() {
  try {
    const res = await fetch('/api/File/types')
    if (res.ok) {
      const types = await res.json()
      fileTypes.value = types || []
    } else {
      // Fallback to common types
      fileTypes.value = [
        { value: 'XML', label: 'XML' },
        { value: 'CSV', label: 'CSV' },
        { value: 'JSON', label: 'JSON' }
      ]
    }
  } catch (e) {
    console.error('Failed to load file types:', e)
    fileTypes.value = [
      { value: 'XML', label: 'XML' },
      { value: 'CSV', label: 'CSV' },
      { value: 'JSON', label: 'JSON' }
    ]
  }
}

/**
 * Business logic: Determine if a file can be deleted
 */
function canDeleteFile(file) {
  const status = file.ruN?.ruN_STATUS?.ruN_STATUS1 ?? file.RUN?.RUN_STATUS?.RUN_STATUS1
  // Allow deletion if status is completed or failed, but not if pending/processing
  return status && ['COMPLETED', 'FAILED'].includes(status.toUpperCase())
}

/**
 * Business logic: Determine if a file can be downloaded
 */
function canDownloadFile(file) {
  // Files can be downloaded if they exist and have a valid ID
  return !!(file.filE_ID ?? file.FILE_ID)
}

/**
 * Dynamic filters based on loaded file types
 */
const filters = computed(() => [
  {
    key: 'fileType',
    label: 'File Type',
    type: 'select',
    options: [
      { value: '', label: 'All Types' },
      ...fileTypes.value.map(ft => ({
        value: ft.value ?? ft.filE_TYPE_NAME ?? ft.FILE_TYPE_NAME,
        label: ft.label ?? ft.filE_TYPE_NAME ?? ft.FILE_TYPE_NAME
      }))
    ],
    default: '',
    sortable: false
  },
  {
    key: 'createdAtRaw',
    label: 'Created Date',
    type: 'date',
    sortable: true
  }
])

/**
 * Handle row click - navigate to run details
 */
function onRowClick(item) {
  // keep row click disabled; navigation is via explicit action buttons
}

/**
 * Navigate to run details page
 */
function navigateToRunDetails(runId) {
  if (runId) {
    router.push({ name: 'LogDetails', params: { id: runId } })
  }
}

/**
 * Download file - direct API call
 */
async function handleDownload(fileId) {
  try {
    const res = await fetch(`/api/File/download/${fileId}`)
    if (res.ok) {
      const blob = await res.blob()
      const url = window.URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      const contentDisposition = res.headers.get('Content-Disposition')
      const fileName = contentDisposition
        ? contentDisposition.split('filename=')[1]?.replace(/"/g, '')
        : `file_${fileId}.xml`
      a.download = fileName
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      window.URL.revokeObjectURL(url)
    } else {
      alert('Failed to download file')
    }
  } catch (e) {
    console.error('Download error:', e)
    alert('Error downloading file')
  }
}

/**
 * Delete file - direct API call
 */
async function handleDelete(fileId, fileName) {
  if (!confirm(`Are you sure you want to delete "${fileName}"?`)) {
    return
  }

  try {
    const res = await fetch(`/api/File/${fileId}`, {
      method: 'DELETE'
    })
    if (res.ok) {
      alert('File deleted successfully')
      // Reload files after deletion
      await loadFiles()
    } else {
      alert('Failed to delete file')
    }
  } catch (e) {
    console.error('Delete error:', e)
    alert('Error deleting file')
  }
}

// Load data on component mount
onMounted(async () => {
  await Promise.all([loadFiles(), loadFileTypes()])
})
</script>

<template>
  <div class="container pt-4">
    <PageHeader
      title="Files Repository"
      subtitle="Following are the details of the reconciliation process runs :"
      instruction="Click on the row to view the log details"
    >
      <template #icon>
        <i class="bi bi-folder-fill" style="font-size: 2rem; color: #6c757d;"></i>
      </template>
    </PageHeader>

    <!-- Loading State -->
    <div v-if="loading" class="text-center py-5">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
      <p class="mt-3 text-muted">Loading files...</p>
    </div>

    <!-- Files Table -->
    <section v-else class="mt-4">
      <BaseTable
        :columns="columns"
        :items="items"
        :showSearch="true"
        :showPagination="true"
        :pageSizeOptions="[10, 25, 50, 100]"
        :filters="filters"
        :rowClickable="false"
      >
        <!-- File Name Column -->
        <template #fileName="{ item }">
          <div class="d-flex align-items-center">
            <i class="bi bi-file-earmark-text me-2 text-primary"></i>
            <span class="fw-medium">{{ item.fileName }}</span>
          </div>
        </template>

        <template #fileType="{ item }">
          <span :class="['filetype-badge', getFileTypeBadgeClass(item.fileType)]">
            <span class="filetype-dot" aria-hidden="true"></span>
            {{ item.fileType || 'Unknown' }}
          </span>
        </template>

        <!-- Actions Column -->
        <template #actions="{ item }">
          <div class="d-flex gap-1">
            <button
              @click.stop="navigateToRunDetails(item.runId)"
              class="btn btn-sm btn-outline-primary"
              :disabled="!item.canNavigate"
              :title="item.canNavigate ? 'View run details' : 'No run associated'"
            >
              <i class="bi bi-arrow-right-circle"></i>
              <span class="d-none d-lg-inline ms-1">Run Details</span>
            </button>

            <button
              @click.stop="handleDownload(item.id)"
              class="btn btn-sm btn-outline-success"
              :disabled="!item.canDownload"
              :title="item.canDownload ? 'Download file' : 'Cannot download this file'"
            >
              <i class="bi bi-download"></i>
              <span class="d-none d-lg-inline ms-1">Download</span>
            </button>
          </div>
        </template>
      </BaseTable>
    </section>

    <!-- Empty State -->
    <div v-if="!loading && items.length === 0" class="text-center py-5">
      <i class="bi bi-inbox display-1 text-muted"></i>
      <p class="mt-3 text-muted">No files found in the repository</p>
    </div>
  </div>
</template>

<style scoped>
.btn-sm {
  font-size: 0.875rem;
  padding: 0.375rem 0.75rem;
}

.btn-sm i {
  font-size: 1rem;
}

.btn:disabled {
  cursor: not-allowed;
  opacity: 0.5;
}

.fw-medium {
  font-weight: 500;
}

.filetype-badge {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.25rem 0.7rem;
  border-radius: 999px;
  border: 1px solid #dee2e6;
  border-left-width: 4px;
  border-left-color: var(--filetype-accent, #adb5bd);
  font-size: 0.8rem;
  font-weight: 500;
  color: #495057;
  background: #f8f9fa;
}

.filetype-dot {
  width: 0.45rem;
  height: 0.45rem;
  border-radius: 999px;
  background-color: var(--filetype-accent, #adb5bd);
  opacity: 0.9;
}


.filetype-update-incorrect-packages { --filetype-accent: #0d6efd; }
.filetype-deactivate-records { --filetype-accent: #dc3545; }
.filetype-link-records-to-packages { --filetype-accent: #6f42c1; }
.filetype-add-new-records { --filetype-accent: #198754; }
.filetype-other { --filetype-accent: #6c757d; }

@media (max-width: 992px) {
  .d-none.d-lg-inline {
    display: none !important;
  }
}
</style>

