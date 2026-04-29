<script setup>
// Presentational navbar component (no logic) — renders brand, navigation links and user avatar
</script>

<template>
  <nav class="navbar navbar-expand-lg bg-light border-bottom shadow-sm">
    <div class="container-fluid">
      <router-link to="/" class="navbar-brand d-flex align-items-center">
        <img src="/src/assets/logo.png" alt="Logo" width="140" height="40" class="me-2" />
      </router-link>

      <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav" aria-controls="navbarNav" aria-expanded="false" aria-label="Toggle navigation">
        <span class="navbar-toggler-icon"></span>
      </button>

      <div class="collapse navbar-collapse" id="navbarNav">
        <ul class="navbar-nav mx-auto">
          <li class="nav-item px-2">
            <router-link class="nav-link" to="/">Dashboard</router-link>
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
            <router-link class="nav-link" to="/logs">Logs</router-link>
          </li>
        </ul>

        <div class="d-flex align-items-center">
          <div class="me-3 d-none d-md-block text-muted small"> </div>
          <div class="user-avatar rounded-circle d-flex align-items-center justify-content-center">MA</div>
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
}

.navbar-nav .nav-link {
  color: #333;
  transition: all 0.12s ease-in-out;
  font-weight: 500;
  font-size: 0.98rem;
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
</style>
