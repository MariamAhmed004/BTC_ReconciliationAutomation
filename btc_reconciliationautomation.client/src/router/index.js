import { createRouter, createWebHistory } from 'vue-router'

import Dashboard from '@/components/Dashboard.vue'
const Welcome = () => import('@/pages/Welcome.vue')
const Login = () => import('@/pages/Login.vue')
const Reconciliation = () => import('@/pages/Reconciliation.vue')
const Management = () => import('@/pages/Management.vue')
const Reconfigure = () => import('@/pages/Reconfigure.vue')
const FilesRepository = () => import('@/pages/FilesRepository.vue')
const Logs = () => import('@/pages/Logs.vue')
const SignUp = () => import('@/pages/SignUp.vue')
const LogDetails = () => import('@/pages/LogDetails.vue')
const ConfigurationDetails = () => import('@/pages/ConfigurationDetails.vue')

const routes = [
  { path: '/', name: 'Welcome', component: Welcome, meta: { hideNav: true } },
  { path: '/dashboard', name: 'Dashboard', component: Dashboard },
  { path: '/login', name: 'Login', component: Login, meta: { hideNav: true } },
  { path: '/signup', name: 'SignUp', component: SignUp, meta: { hideNav: true } },
  { path: '/log/:id', name: 'LogDetails', component: LogDetails },
  { path: '/configuration/:id', name: 'ConfigurationDetails', component: ConfigurationDetails },
  { path: '/reconciliation', name: 'Reconciliation', component: Reconciliation },
  { path: '/management', name: 'Management', component: Management },
  { path: '/reconfigure', name: 'Reconfigure', component: Reconfigure },
  { path: '/files', name: 'FilesRepository', component: FilesRepository },
  { path: '/logs', name: 'Logs', component: Logs }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
  linkActiveClass: 'active'
})

export default router
