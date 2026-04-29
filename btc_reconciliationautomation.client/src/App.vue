<script setup>
import NavBar from './components/common/NavBar.vue'
import AppFooter from './components/common/AppFooter.vue'
</script>

<template>
  <div class="d-flex flex-column min-vh-100">
    <NavBar v-if="!$route.meta.hideNav" />
    <div class="flex-grow-1">
      <router-view />
    </div>
    <AppFooter />
  </div>
</template>

<style scoped>
header {
  line-height: 1.5;
}

.logo {
  display: block;
  margin: 0 auto 2rem;
}

@media (min-width: 1024px) {
  header {
    display: flex;
    place-items: center;
    padding-right: calc(var(--section-gap) / 2);
  }

  .logo {
    margin: 0 2rem 0 0;
  }

  header .wrapper {
    display: flex;
    place-items: flex-start;
    flex-wrap: wrap;
  }
}
</style>
