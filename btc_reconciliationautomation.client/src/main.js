import 'bootswatch/dist/flatly/bootstrap.min.css' // bootswatch theme (includes Bootstrap styles)
import 'bootstrap-icons/font/bootstrap-icons.css' // Bootstrap Icons
import * as bootstrap from 'bootstrap'            // Import Bootstrap and expose it
import './assets/main.css'

import { createApp } from 'vue'
import App from './App.vue'
import router from './router'

// Make Bootstrap available globally
window.bootstrap = bootstrap

createApp(App).use(router).mount('#app')
