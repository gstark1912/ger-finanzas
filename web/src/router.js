import { createRouter, createWebHistory } from 'vue-router'
import HomePage from './pages/HomePage.vue'
import ConfiguracionPage from './pages/ConfiguracionPage.vue'
import GastosFijosPage from './pages/GastosFijosPage.vue'
import DashboardPage from './pages/DashboardPage.vue'
import CajaUsdPage from './pages/CajaUsdPage.vue'

const routes = [
  { path: '/', component: HomePage },
  { path: '/dashboard', component: DashboardPage },
  { path: '/caja-usd', component: CajaUsdPage },
  { path: '/configuracion', component: ConfiguracionPage },
  { path: '/gastos-fijos', component: GastosFijosPage }
]

export default createRouter({
  history: createWebHistory(),
  routes
})
