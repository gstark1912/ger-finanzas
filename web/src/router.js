import { createRouter, createWebHistory } from 'vue-router'
import HomePage from './pages/HomePage.vue'
import ConfiguracionPage from './pages/ConfiguracionPage.vue'
import GastosFijosPage from './pages/GastosFijosPage.vue'

const routes = [
  { path: '/', component: HomePage },
  { path: '/configuracion', component: ConfiguracionPage },
  { path: '/gastos-fijos', component: GastosFijosPage }
]

export default createRouter({
  history: createWebHistory(),
  routes
})
