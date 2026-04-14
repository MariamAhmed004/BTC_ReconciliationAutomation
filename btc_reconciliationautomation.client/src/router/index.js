import { createRouter, createWebHistory } from 'vue-router'
import Dashboard from '@/components/Dashboard.vue'

const Reconciliation = () => import('@/pages/Reconciliation.vue')
const Management = () => import('@/pages/Management.vue')
const Alerts = () => import('@/pages/Alerts.vue')
const FilesRepository = () => import('@/pages/FilesRepository.vue')
const Logs = () => import('@/pages/Logs.vue')

const routes = [
  { path: '/', name: 'Dashboard', component: Dashboard },
  { path: '/reconciliation', name: 'Reconciliation', component: Reconciliation },
  { path: '/management', name: 'Management', component: Management },
  { path: '/alerts', name: 'Alerts', component: Alerts },
  { path: '/files', name: 'FilesRepository', component: FilesRepository },
  { path: '/logs', name: 'Logs', component: Logs }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
  linkActiveClass: 'active'
})

export default router
