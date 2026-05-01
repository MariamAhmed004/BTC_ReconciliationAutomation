<script setup>
import logo from '@/assets/logo.png'
</script>

<template>
    <footer class="border-top shadow-up mt-5">

      <div class="footer-main">
        <div class="container">
          <div class="row align-items-start">

            <!-- Left: Logo + tagline + links -->
            <div class="col-md-4 mb-4 mb-md-0">
              <div class="mb-3">
                <img :src="logo" alt="Batelco logo" class="footer-logo" />
              </div>
              <div class="footer-desc">
                Reconciliation Dashboard<br />
                Internal Reporting System<br />
                Automated reconciliation and XML generation
              </div>
            </div>

            <!-- Right: Compliance & Version -->
            <div class="col-md-8 text-md-end">
              <div class="footer-section-title">COMPLIANCE &amp; VERSION INFO</div>
              <div class="footer-version mt-2">
                System Version: v1.0.0<br />
                Last Update: May 2026
              </div>
            </div>

          </div>
        </div>
      </div>

      <div class="footer-bottom">
        <div class="container">
          <div class="text-center footer-copy">
            © 2026 Reconciliation System | Internal Use Only | All Rights Reserved
          </div>
        </div>
      </div>
    </footer>
</template>

<style scoped>
footer {
  background-color: #f1f3f5;
  color: #333;
  box-shadow: 0 -2px 8px rgba(0, 0, 0, 0.08);
}

.footer-main {
  padding: 2rem 0 1.5rem;
}

.footer-logo {
  width: 160px;
  height: auto;
  object-fit: contain;
}

.footer-desc {
  font-size: 0.8rem;
  color: #6c757d;
  line-height: 1.7;
}

.footer-section-title {
  font-size: 0.78rem;
  font-weight: 700;
  letter-spacing: 0.07em;
  color: #333;
}

.footer-links li {
  margin-top: 0.35rem;
}

.footer-links a {
  color: #555;
  text-decoration: none;
  font-size: 0.85rem;
  transition: color 0.2s;
}

.footer-links a:hover {
  color: #000;
}

.footer-version {
  font-size: 0.82rem;
  color: #6c757d;
  line-height: 1.7;
}

.footer-bottom {
  border-top: 1px solid #dee2e6;
  padding: 0.75rem 0;
}

.footer-copy {
  font-size: 0.8rem;
  color: #6c757d;
}
</style>

