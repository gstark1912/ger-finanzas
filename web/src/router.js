import { createRouter, createWebHistory } from 'vue-router'
import HomePage from './pages/HomePage.vue'
import ConfiguracionPage from './pages/ConfiguracionPage.vue'

const routes = [
  { path: '/', component: HomePage },
  { path: '/configuracion', component: ConfiguracionPage }
]

export default createRouter({
  history: createWebHistory(),
  routes
})
