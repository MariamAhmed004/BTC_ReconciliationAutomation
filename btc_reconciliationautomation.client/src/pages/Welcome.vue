<script setup>
// Welcome page - no navbar/footer (presentational only)
</script>

<template>
  <div class="welcome-page vh-100 d-flex flex-column">
    <header class="py-3 px-4">
      <img src="/src/assets/logo.png" alt="Logo" width="140" />
    </header>

    <main class="flex-fill d-flex align-items-center">
      <div class="container">
        <div class="row align-items-center">
          <div class="col-md-6 text-center d-none d-md-block">
            <img src="/src/assets/robot.png" alt="Robot" class="img-fluid" style="max-height:400px;" />
          </div>
          <div class="col-md-6">
            <h1 class="fw-bold">Welcome</h1>
            <p class="lead text-muted">Access the reconciliation dashboard</p>
            <p class="text-muted">This website provides you with details regarding automated processes</p>
            <router-link to="/" class="btn btn-dark btn-lg mt-3">Access Dashboard</router-link>
          </div>
        </div>
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
