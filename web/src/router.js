import { createRouter, createWebHistory } from 'vue-router'
import ConfiguracionPage from './pages/ConfiguracionPage.vue'
import GastosFijosPage from './pages/GastosFijosPage.vue'
import GastosVariablesPage from './pages/GastosVariablesPage.vue'
import DashboardPage from './pages/DashboardPage.vue'
import CajaUsdPage from './pages/CajaUsdPage.vue'
import InversionesPage from './pages/InversionesPage.vue'

import TarjetasPage from './pages/TarjetasPage.vue'

const routes = [
  { path: '/', component: DashboardPage },
  { path: '/dashboard', component: DashboardPage },
  { path: '/caja', component: CajaUsdPage },
  { path: '/inversiones', component: InversionesPage },
  { path: '/configuracion', component: ConfiguracionPage },
  { path: '/gastos-fijos', component: GastosFijosPage },
  { path: '/gastos-variables', component: GastosVariablesPage },
  { path: '/tarjetas', component: TarjetasPage }
]

export default createRouter({
  history: createWebHistory(),
  routes
})
