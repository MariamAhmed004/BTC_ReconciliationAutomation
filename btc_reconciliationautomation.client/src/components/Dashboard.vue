<script setup>
// Simple dashboard to verify Bootstrap/Bootswatch integration
</script>

<template>
  <div>
    <nav class="navbar navbar-expand-lg navbar-dark bg-primary mb-4">
      <div class="container-fluid">
        <a class="navbar-brand" href="#">BTC Reconciliation - Dashboard</a>
        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav" aria-controls="navbarNav" aria-expanded="false" aria-label="Toggle navigation">
          <span class="navbar-toggler-icon"></span>
        </button>
        <div class="collapse navbar-collapse" id="navbarNav">
          <ul class="navbar-nav ms-auto">
            <li class="nav-item">
              <a class="nav-link active" aria-current="page" href="#">Home</a>
            </li>
            <li class="nav-item">
              <a class="nav-link" href="#">Reports</a>
            </li>
          </ul>
        </div>
      </div>
    </nav>

    <div class="container">
      <div class="row mb-4">
        <div class="col-md-4">
          <div class="card text-white bg-success mb-3">
            <div class="card-body">
              <h5 class="card-title">Reconciled</h5>
              <p class="card-text display-6">1,234</p>
            </div>
          </div>
        </div>
        <div class="col-md-4">
          <div class="card text-dark bg-warning mb-3">
            <div class="card-body">
              <h5 class="card-title">Pending</h5>
              <p class="card-text display-6">56</p>
            </div>
          </div>
        </div>
        <div class="col-md-4">
          <div class="card text-white bg-danger mb-3">
            <div class="card-body">
              <h5 class="card-title">Errors</h5>
              <p class="card-text display-6">3</p>
            </div>
          </div>
        </div>
      </div>

      <div class="row">
        <div class="col-12">
          <div class="card">
            <div class="card-header">
              Recent Items
            </div>
            <div class="card-body p-0">
              <table class="table mb-0">
                <thead class="table-light">
                  <tr>
                    <th scope="col">ID</th>
                    <th scope="col">Date</th>
                    <th scope="col">Status</th>
                    <th scope="col">Amount</th>
                  </tr>
                </thead>
                <tbody>
                  <tr>
                    <td>#1001</td>
                    <td>2026-04-01</td>
                    <td><span class="badge bg-success">Reconciled</span></td>
                    <td>0.1234 BTC</td>
                  </tr>
                  <tr>
                    <td>#1002</td>
                    <td>2026-04-02</td>
                    <td><span class="badge bg-warning text-dark">Pending</span></td>
                    <td>0.0500 BTC</td>
                  </tr>
                  <tr>
                    <td>#1003</td>
                    <td>2026-04-03</td>
                    <td><span class="badge bg-danger">Error</span></td>
                    <td>0.0050 BTC</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* small spacing tweak */
.display-6 { font-size: 2rem; }
</style>
