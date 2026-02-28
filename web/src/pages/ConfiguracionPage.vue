<template>
  <div>
    <nav class="nav">
      <div class="nav-container">
        <router-link to="/" class="nav-link">Inicio</router-link>
        <a href="#" class="nav-link">Dashboard</a>
        <router-link to="/gastos-fijos" class="nav-link">Gastos Fijos</router-link>
        <a href="#" class="nav-link">Tarjeta Visa</a>
        <a href="#" class="nav-link">Caja USD</a>
        <a href="#" class="nav-link">Inversiones</a>
        <router-link to="/configuracion" class="nav-link active">Configuración</router-link>
      </div>
    </nav>
    <div class="container">
      <div class="header">
        <h1>Configuración</h1>
      </div>

      <div class="tabs">
        <button class="tab" :class="{ active: activeTab === 'accounts' }" @click="activeTab = 'accounts'">
          Cuentas de gasto
        </button>
        <button class="tab" :class="{ active: activeTab === 'fx' }" @click="activeTab = 'fx'">Tipo de cambio</button>
        <button class="tab" disabled>Tarjetas</button>
        <button class="tab" disabled>Categorías</button>
        <button class="tab" disabled>Objetivos</button>
      </div>

      <div v-if="activeTab === 'accounts'" class="section">
        <div class="section-header">
          <div class="section-title">Cuentas de gasto</div>
          <button @click="openCreateModal">+ Agregar cuenta</button>
        </div>

        <div v-if="store.loading && !store.accounts.length">Cargando...</div>

        <table v-else>
          <thead>
            <tr>
              <th>Nombre</th>
              <th>Tipo</th>
              <th>Moneda</th>
              <th>Estado</th>
              <th style="width: 180px;">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="account in store.accounts" :key="account.id">
              <td>{{ account.name }}</td>
              <td>{{ formatType(account.type) }}</td>
              <td>{{ account.currency }}</td>
              <td>
                <span :class="['badge', account.isActive ? 'active' : 'inactive']">
                  {{ account.isActive ? 'Activa' : 'Inactiva' }}
                </span>
              </td>
              <td class="actions">
                <button class="btn-small secondary" @click="openEditModal(account)">Editar</button>
                <button class="btn-small btn-delete" @click="handleDelete(account.id)">Eliminar</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div v-if="activeTab === 'fx'" class="section">
        <div class="section-header">
          <div class="section-title">Tipo de cambio USD/ARS</div>
        </div>
        <div v-if="monthStore.loading">Cargando...</div>
        <table v-else>
          <thead>
            <tr>
              <th>Mes</th>
              <th>ARS por 1 USD</th>
              <th style="width: 120px;">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="month in monthStore.months" :key="month.id">
              <td>{{ formatMonth(month.year, month.monthNumber) }}</td>
              <td><input type="number" v-model="fxEdits[month.id]" style="width: 120px;" /></td>
              <td>
                <button class="btn-small secondary" @click="saveFxRate(month.id)">Guardar</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <AccountModal v-if="showModal" :account="selectedAccount" @close="closeModal" @submit="handleSubmit" />

      <Notification :message="notification.message" :type="notification.type" />
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { useExpenseAccountStore } from '../stores/expenseAccount'
import { useMonthStore } from '../stores/month'
import AccountModal from '../components/AccountModal.vue'
import Notification from '../components/Notification.vue'

const store = useExpenseAccountStore()
const monthStore = useMonthStore()
const activeTab = ref('accounts')
const showModal = ref(false)
const selectedAccount = ref(null)
const notification = ref({ message: '', type: 'success' })
const fxEdits = ref({})

watch(() => monthStore.months, (months) => {
  months.forEach(m => { fxEdits.value[m.id] = m.rate ?? '' })
}, { immediate: true })

onMounted(() => {
  store.fetchAccounts()
  monthStore.fetchMonths()
})

function openCreateModal() {
  selectedAccount.value = null
  showModal.value = true
}

function openEditModal(account) {
  selectedAccount.value = account
  showModal.value = true
}

function closeModal() {
  showModal.value = false
  selectedAccount.value = null
}

async function handleSubmit(formData) {
  try {
    if (selectedAccount.value) {
      await store.updateAccount(selectedAccount.value.id, formData)
      showNotification('Cuenta actualizada correctamente', 'success')
    } else {
      await store.createAccount(formData)
      showNotification('Cuenta creada correctamente', 'success')
    }
    closeModal()
  } catch (error) {
    showNotification(error.message, 'error')
  }
}

async function handleDelete(id) {
  if (!confirm('¿Está seguro de eliminar esta cuenta?')) return

  try {
    await store.deleteAccount(id)
    showNotification('Cuenta eliminada correctamente', 'success')
  } catch (error) {
    showNotification(error.message, 'error')
  }
}

function formatType(type) {
  const types = {
    Bank: 'Banco',
    Cash: 'Efectivo',
    CC: 'Tarjeta de Crédito'
  }
  return types[type] || type
}

function formatMonth(year, month) {
  return new Date(year, month - 1).toLocaleString('es-AR', { month: 'long', year: 'numeric' })
}

async function saveFxRate(monthId) {
  try {
    await monthStore.updateFxRate(monthId, fxEdits.value[monthId])
    showNotification('Tipo de cambio actualizado', 'success')
  } catch (e) {
    showNotification(e.message, 'error')
  }
}

function showNotification(message, type) {
  notification.value = { message, type }
}
</script>
