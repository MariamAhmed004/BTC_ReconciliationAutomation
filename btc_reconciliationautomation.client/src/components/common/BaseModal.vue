<script setup>
import { ref, watch, onMounted, nextTick, onUnmounted } from 'vue'

const props = defineProps({
  show: { type: Boolean, default: false },
  title: { type: String, default: 'Modal' },
  message: { type: String, default: '' },
  size: { type: String, default: 'md' }, // sm, md, lg, xl
  buttons: {
    type: Array,
    default: () => [
      { text: 'Close', variant: 'secondary', action: 'close' }
    ]
  }
})

const emit = defineEmits(['close', 'action'])

const modalElement = ref(null)
let bootstrapModal = null

onMounted(() => {
  console.log('BaseModal mounted')
  if (modalElement.value) {
    // Check if Bootstrap is available
    if (typeof window.bootstrap === 'undefined') {
      console.error('Bootstrap is not loaded!')
      return
    }

    try {
      // Initialize Bootstrap modal
      bootstrapModal = new window.bootstrap.Modal(modalElement.value, {
        backdrop: true,
        keyboard: true
      })
      console.log('Bootstrap modal initialized:', bootstrapModal)

      // Listen to Bootstrap modal events
      modalElement.value.addEventListener('hidden.bs.modal', () => {
        console.log('Modal hidden event')
        emit('close')
      })
    } catch (error) {
      console.error('Error initializing modal:', error)
    }
  } else {
    console.error('Modal element not found')
  }
})

onUnmounted(() => {
  if (bootstrapModal) {
    try {
      bootstrapModal.dispose()
    } catch (e) {
      console.error('Error disposing modal:', e)
    }
  }
})

watch(() => props.show, async (newVal) => {
  console.log('Modal show prop changed to:', newVal)
  await nextTick()

  if (!bootstrapModal) {
    console.error('Bootstrap modal not initialized')
    return
  }

  try {
    if (newVal) {
      console.log('Showing modal...')
      bootstrapModal.show()
    } else {
      console.log('Hiding modal...')
      bootstrapModal.hide()
    }
  } catch (error) {
    console.error('Error showing/hiding modal:', error)
  }
}, { immediate: false })

function handleButtonClick(button) {
  console.log('Button clicked:', button)
  if (button.action === 'close') {
    if (bootstrapModal) {
      bootstrapModal.hide()
    }
    emit('close')
  } else {
    emit('action', button.action)
  }
}
</script>

<template>
  <div 
    ref="modalElement"
    class="modal fade" 
    tabindex="-1" 
    aria-labelledby="modalTitle"
    aria-hidden="true"
  >
    <div :class="['modal-dialog', size !== 'md' ? `modal-${size}` : '']">
      <div class="modal-content">
        <div class="modal-header">
          <h5 id="modalTitle" class="modal-title">{{ title }}</h5>
          <button 
            type="button" 
            class="btn-close" 
            data-bs-dismiss="modal"
            aria-label="Close"
            @click="handleButtonClick({ action: 'close' })"
          ></button>
        </div>
        <div class="modal-body">
          <slot>
            <p class="mb-0">{{ message }}</p>
          </slot>
        </div>
        <div class="modal-footer">
          <button 
            v-for="(button, index) in buttons" 
            :key="index"
            type="button" 
            :class="['btn', `btn-${button.variant || 'secondary'}`]"
            @click="handleButtonClick(button)"
          >
            {{ button.text }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.modal-header {
  background-color: #f1f3f5;
  border-bottom: 1px solid rgba(0,0,0,0.08);
}

.modal-title {
  font-weight: 600;
}
</style>
