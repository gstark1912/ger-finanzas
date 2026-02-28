<template>
  <div>
    <nav class="nav">
      <div class="nav-container">
        <router-link to="/" class="nav-link">Inicio</router-link>
        <a href="#" class="nav-link">Dashboard</a>
        <a href="#" class="nav-link">Gastos Fijos</a>
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
        <button class="tab" disabled>Tipo de cambio</button>
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

      <AccountModal v-if="showModal" :account="selectedAccount" @close="closeModal" @submit="handleSubmit" />

      <Notification :message="notification.message" :type="notification.type" />
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useExpenseAccountStore } from '../stores/expenseAccount'
import AccountModal from '../components/AccountModal.vue'
import Notification from '../components/Notification.vue'

const store = useExpenseAccountStore()
const activeTab = ref('accounts')
const showModal = ref(false)
const selectedAccount = ref(null)
const notification = ref({ message: '', type: 'success' })

onMounted(() => {
  store.fetchAccounts()
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

function showNotification(message, type) {
  notification.value = { message, type }
}
</script>
