<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()

const firstName = ref('')
const lastName = ref('')
const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const error = ref('')
const loading = ref(false)

async function signUp() {
  error.value = ''

  if (password.value !== confirmPassword.value) {
    error.value = 'Passwords do not match.'
    return
  }

  loading.value = true
  try {
    const res = await fetch('/api/auth/signup', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        username: email.value,
        email: email.value,
        password: password.value,
        firstName: firstName.value,
        lastName: lastName.value
      })
    })

    if (res.ok) {
      router.push({ name: 'Login' })
    } else {
      const data = await res.json()
      error.value = data.message ?? (data.errors ? data.errors.join(' ') : 'Sign up failed.')
    }
  } catch {
    error.value = 'Unable to reach the server.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="auth-page vh-100 d-flex flex-column">
    <header class="py-3 px-4">
      <img src="/src/assets/logo.png" alt="Logo" width="140" />
    </header>

    <main class="flex-fill d-flex align-items-center">
      <div class="container">
        <div class="row justify-content-center">
          <div class="col-md-6 col-lg-5">
            <div class="auth-card p-4">
              <h2 class="fw-bold text-center">Sign Up</h2>
              <p class="text-muted text-center mb-4">Create an account to access the reconciliation dashboard</p>

              <div v-if="error" class="alert alert-danger py-2">{{ error }}</div>

              <form @submit.prevent="signUp">
                <div class="row mb-3">
                  <div class="col">
                    <input v-model="firstName" type="text" class="form-control" placeholder="First Name" required />
                  </div>
                  <div class="col">
                    <input v-model="lastName" type="text" class="form-control" placeholder="Last Name" required />
                  </div>
                </div>

                <div class="mb-3">
                  <div class="input-group">
                    <span class="input-group-text bg-white border-end-0" aria-hidden>
                      <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="#6c757d" viewBox="0 0 16 16"><path d="M3 14s-1 0-1-1 1-4 6-4 6 3 6 4-1 1-1 1H3z"/><path fill-rule="evenodd" d="M8 8a3 3 0 100-6 3 3 0 000 6z"/></svg>
                    </span>
                    <input v-model="email" type="email" class="form-control border-start-0" placeholder="Email" required />
                  </div>
                </div>

                <div class="mb-3">
                  <div class="input-group">
                    <span class="input-group-text bg-white border-end-0" aria-hidden>
                      <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="#6c757d" viewBox="0 0 16 16"><path d="M8 1a4 4 0 00-4 4v2H3a1 1 0 00-1 1v5a1 1 0 001 1h10a1 1 0 001-1v-5a1 1 0 00-1-1h-1V5a4 4 0 00-4-4zM6 5a2 2 0 114 0v2H6V5z"/></svg>
                    </span>
                    <input v-model="password" type="password" class="form-control border-start-0" placeholder="Password" required />
                  </div>
                </div>

                <div class="mb-3">
                  <div class="input-group">
                    <span class="input-group-text bg-white border-end-0" aria-hidden>
                      <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="#6c757d" viewBox="0 0 16 16"><path d="M8 1a4 4 0 00-4 4v2H3a1 1 0 00-1 1v5a1 1 0 001 1h10a1 1 0 001-1v-5a1 1 0 00-1-1h-1V5a4 4 0 00-4-4zM6 5a2 2 0 114 0v2H6V5z"/></svg>
                    </span>
                    <input v-model="confirmPassword" type="password" class="form-control border-start-0" placeholder="Confirm Password" required />
                  </div>
                </div>

                <div class="d-grid">
                  <button type="submit" class="btn btn-dark btn-lg" :disabled="loading">
                    {{ loading ? 'Creating account…' : 'Sign Up' }}
                  </button>
                </div>
              </form>

              <div class="text-center mt-4">
                <small class="text-muted">Already have an account? <router-link to="/login">Login</router-link></small>
              </div>
            </div>
          </div>
        </div>
      </div>
    </main>

    <footer class="py-4 text-center text-muted small">
      <!-- empty footer for auth pages -->
    </footer>
  </div>
</template>

<style scoped>
.auth-page {
  background: url('/src/assets/hero-bg.png') center/cover no-repeat;
}

.auth-card {
  background: rgba(255,255,255,0.9);
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.08);
  padding: 2rem;
}

.input-group .form-control {
  border-left: 0;
}

.input-group-text {
  border-right: 0;
}
</style>
