<script setup>
// Welcome page - no navbar/footer (presentational only)
</script>

<template>
  <div class="welcome-page vh-100 d-flex flex-column">
    <header class="py-3 px-4">
      <img src="/src/assets/logo.png" alt="Logo" width="140" />
    </header>

    <main class="flex-fill d-flex align-items-center justify-content-center">
      <div class="container text-center">
        <h1 class="fw-bold display-4">BTC Reconciliation Automation</h1>
        <p class="lead text-muted mt-3">Streamline and monitor your automated reconciliation processes in one place</p>
        <p class="text-muted">Gain full visibility into reconciliation results, discrepancies, and process history</p>
        <router-link to="/login" class="btn btn-dark btn-lg mt-4">Get Started</router-link>
      </div>
    </main>

    <footer class="py-4 text-center text-muted small">
      <!-- intentionally empty footer area for welcome page -->
    </footer>
  </div>
</template>

<style scoped>
.welcome-page {
  background: url('/src/assets/hero-bg.png') center/cover no-repeat;
}
</style>
