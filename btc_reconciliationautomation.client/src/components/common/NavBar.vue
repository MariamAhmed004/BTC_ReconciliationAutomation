<script setup>
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()

const isUserMenuOpen = ref(false)
const user = ref(null)

const initials = computed(() => {
  if (!user.value) return '?'
  const f = user.value.firstName?.[0] ?? ''
  const l = user.value.lastName?.[0] ?? ''
  return (f + l).toUpperCase() || user.value.userName?.[0]?.toUpperCase() || '?'
})

const fullName = computed(() => {
  if (!user.value) return ''
  return `${user.value.firstName ?? ''} ${user.value.lastName ?? ''}`.trim()
})

const toggleUserMenu = () => {
  isUserMenuOpen.value = !isUserMenuOpen.value
}

const closeUserMenu = () => {
  isUserMenuOpen.value = false
}

const onDocumentClick = (e) => {
  const target = e.target
  if (!(target instanceof Element)) return
  if (target.closest('.user-menu')) return
  closeUserMenu()
}

document.addEventListener('click', onDocumentClick)

onBeforeUnmount(() => {
  document.removeEventListener('click', onDocumentClick)
})

onMounted(async () => {
  try {
    const res = await fetch('/api/auth/me', { credentials: 'include' })
    if (res.ok) user.value = await res.json()
  } catch {
    // not logged in or network error
  }
})

const logout = async () => {
  closeUserMenu()
  try {
    await fetch('/api/auth/logout', { method: 'POST', credentials: 'include' })
  } catch {
    // ignore network errors, still proceed to redirect
  }
  user.value = null
  await router.push('/login')
}
</script>

<template>
  <nav class="navbar navbar-expand-lg bg-light border-bottom shadow-sm">
    <div class="container-fluid">
      <router-link to="/dashboard" class="navbar-brand d-flex align-items-center">
        <img src="/src/assets/logo.png" alt="Logo" width="140" height="40" class="me-2" />
      </router-link>

      <div class="collapse navbar-collapse flex-grow-1 order-3 order-lg-2" id="navbarNav">
        <ul class="navbar-nav mx-auto align-items-center text-center">
          <li class="nav-item px-2">
            <router-link class="nav-link" to="/dashboard">Dashboard</router-link>
          </li>
          <li class="nav-item px-2">
            <router-link class="nav-link" to="/reconciliation">Reconciliation Execution</router-link>
          </li>
          <li class="nav-item px-2">
            <router-link class="nav-link" to="/management">Management &amp; Configuration</router-link>
          </li>
          <li class="nav-item px-2">
            <router-link class="nav-link" to="/files">Files Repository</router-link>
          </li>
          <li class="nav-item px-2">
            <router-link class="nav-link" to="/logs">Audit Logs</router-link>
          </li>
        </ul>
      </div>

      <div class="d-flex align-items-center ms-auto order-2 order-lg-3 pe-2">
        <button class="navbar-toggler me-2" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav" aria-controls="navbarNav" aria-expanded="false" aria-label="Toggle navigation">
          <span class="navbar-toggler-icon"></span>
        </button>

        <div class="user-menu position-relative">
          <button
            type="button"
            class="user-avatar rounded-circle d-flex align-items-center justify-content-center"
            @click.stop="toggleUserMenu"
            aria-label="User menu"
            :aria-expanded="isUserMenuOpen ? 'true' : 'false'"
          >
            {{ initials }}
          </button>

          <div
            v-if="isUserMenuOpen"
            class="dropdown-menu dropdown-menu-end show user-dropdown"
            style="position: absolute; top: calc(100% + 0.5rem); right: 0;"
          >
            <div class="user-info px-3 py-2 border-bottom">
              <div class="fw-semibold">{{ fullName }}</div>
              <div class="text-muted small">{{ user?.email }}</div>
            </div>
            <button class="dropdown-item" type="button" @click="logout">
              Logout
            </button>
          </div>
        </div>
      </div>
    </div>
  </nav>
</template>

<style scoped>
.user-avatar {
  width: 40px;
  height: 40px;
  background: #f8f9fa;
  border: 1px solid #ddd;
  color: #333;
  font-weight: 600;
  cursor: pointer;
  user-select: none;
  padding: 0;
}

.navbar-collapse {
  justify-content: center;
}

.navbar-nav .nav-link {
  color: #333;
  transition: all 0.12s ease-in-out;
  font-weight: 500;
  font-size: 0.98rem;
}

.navbar-nav .nav-link:hover,
.navbar-nav .nav-link:focus,
.navbar-nav .nav-link.active,
.navbar-nav .nav-link.router-link-active {
  background-color: transparent !important;
}

.navbar-nav .nav-link:hover,
.navbar-nav .nav-link.router-link-active {
  color: #000;
  font-weight: 700;
  font-size: 1.03rem;
  text-decoration: underline;
  font-style: italic;
  /* lift slightly to emphasize */
  transform: translateY(-1px);
}

/* make the navbar background a soft highlight grey */
nav.navbar {
  background-color: #f1f3f5 !important;
}

/* ensure avatar text is black */
.user-avatar {
  color: #000;
}

.user-dropdown {
  min-width: 200px;
}

.user-info {
  line-height: 1.4;
}
</style>
