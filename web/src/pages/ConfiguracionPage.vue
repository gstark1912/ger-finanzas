<template>
  <div>
    <AppNav />
    <div class="container">
      <div class="header">
        <h1>Configuración</h1>
      </div>

      <div class="tabs">
        <button class="tab" :class="{ active: activeTab === 'fx' }" @click="activeTab = 'fx'">Tipo de cambio</button>
        <button class="tab" :class="{ active: activeTab === 'accounts' }" @click="activeTab = 'accounts'">Cuentas de gasto</button>
        <button class="tab" :class="{ active: activeTab === 'saving' }" @click="activeTab = 'saving'">Cuentas de Ahorro</button>
        <button class="tab" :class="{ active: activeTab === 'investment' }" @click="activeTab = 'investment'">Cuentas de Inversión</button>
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

      <div v-if="activeTab === 'saving'" class="section">
        <div class="section-header">
          <div class="section-title">Cuentas de Ahorro</div>
          <button @click="openSavingCreate">+ Agregar cuenta</button>
        </div>
        <div v-if="savingStore.loading">Cargando...</div>
        <table v-else>
          <thead>
            <tr>
              <th>Nombre</th>
              <th>Tipo</th>
              <th>Moneda</th>
              <th>Estado</th>
              <th style="width:180px;">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="a in savingStore.accounts" :key="a.id">
              <td>{{ a.name }}</td>
              <td>{{ a.type === 'Bank' ? 'Banco' : 'Efectivo' }}</td>
              <td>{{ a.currency }}</td>
              <td><span :class="['badge', a.isActive ? 'active' : 'inactive']">{{ a.isActive ? 'Activa' : 'Inactiva' }}</span></td>
              <td class="actions">
                <button class="btn-small secondary" @click="openSavingEdit(a)">Editar</button>
                <button class="btn-small btn-delete" @click="deleteSaving(a.id)">Eliminar</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div v-if="activeTab === 'investment'" class="section">
        <div class="section-header">
          <div class="section-title">Cuentas de Inversión</div>
          <button @click="openInvestmentCreate">+ Agregar cuenta</button>
        </div>
        <div v-if="investmentStore.loading">Cargando...</div>
        <table v-else>
          <thead>
            <tr>
              <th>Nombre</th>
              <th>Moneda</th>
              <th>Retorno anual</th>
              <th>Estado</th>
              <th style="width:180px;">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="a in investmentStore.accounts" :key="a.id">
              <td>{{ a.name }}</td>
              <td>{{ a.currency }}</td>
              <td>{{ a.expectedAnnualReturnPct != null ? a.expectedAnnualReturnPct + '%' : '-' }}</td>
              <td><span :class="['badge', a.isActive ? 'active' : 'inactive']">{{ a.isActive ? 'Activa' : 'Inactiva' }}</span></td>
              <td class="actions">
                <button class="btn-small secondary" @click="openInvestmentEdit(a)">Editar</button>
                <button class="btn-small btn-delete" @click="deleteInvestment(a.id)">Eliminar</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <AccountModal v-if="showModal" :account="selectedAccount" @close="closeModal" @submit="handleSubmit" />

      <!-- Saving account modal -->
      <div v-if="showSavingModal" class="modal" @click.self="showSavingModal = false">
        <div class="modal-content">
          <div class="modal-header">
            <span class="modal-title">{{ editingSaving ? 'Editar' : 'Nueva' }} cuenta de ahorro</span>
            <button class="modal-close" @click="showSavingModal = false">×</button>
          </div>
          <div class="form-group">
            <label>Nombre</label>
            <input v-model="savingForm.name" type="text" />
          </div>
          <div class="form-group">
            <label>Tipo</label>
            <select v-model="savingForm.type">
              <option value="Bank">Banco</option>
              <option value="Cash">Efectivo</option>
            </select>
          </div>
          <div class="form-group">
            <label>Moneda</label>
            <select v-model="savingForm.currency">
              <option value="USD">USD</option>
              <option value="ARS">ARS</option>
            </select>
          </div>
          <div v-if="editingSaving" class="form-group">
            <label>Estado</label>
            <select v-model="savingForm.isActive">
              <option :value="true">Activa</option>
              <option :value="false">Inactiva</option>
            </select>
          </div>
          <div class="form-actions">
            <button class="secondary" @click="showSavingModal = false">Cancelar</button>
            <button @click="submitSaving">Guardar</button>
          </div>
        </div>
      </div>

      <!-- Investment account modal -->
      <div v-if="showInvestmentModal" class="modal" @click.self="showInvestmentModal = false">
        <div class="modal-content">
          <div class="modal-header">
            <span class="modal-title">{{ editingInvestment ? 'Editar' : 'Nueva' }} cuenta de inversión</span>
            <button class="modal-close" @click="showInvestmentModal = false">×</button>
          </div>
          <div class="form-group">
            <label>Nombre</label>
            <input v-model="investmentForm.name" type="text" />
          </div>
          <div class="form-group">
            <label>Moneda</label>
            <select v-model="investmentForm.currency">
              <option value="USD">USD</option>
              <option value="ARS">ARS</option>
            </select>
          </div>
          <div class="form-group">
            <label>Retorno anual esperado (%)</label>
            <input v-model="investmentForm.expectedAnnualReturnPct" type="number" step="0.01" placeholder="Opcional" />
          </div>
          <div v-if="editingInvestment" class="form-group">
            <label>Estado</label>
            <select v-model="investmentForm.isActive">
              <option :value="true">Activa</option>
              <option :value="false">Inactiva</option>
            </select>
          </div>
          <div class="form-actions">
            <button class="secondary" @click="showInvestmentModal = false">Cancelar</button>
            <button @click="submitInvestment">Guardar</button>
          </div>
        </div>
      </div>

      <Notification :message="notification.message" :type="notification.type" />
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import AppNav from '../components/AppNav.vue'
import { useExpenseAccountStore } from '../stores/expenseAccount'
import { useMonthStore } from '../stores/month'
import { useSavingAccountStore } from '../stores/savingAccount'
import { useInvestmentAccountStore } from '../stores/investmentAccount'
import AccountModal from '../components/AccountModal.vue'
import Notification from '../components/Notification.vue'

const store = useExpenseAccountStore()
const monthStore = useMonthStore()
const savingStore = useSavingAccountStore()
const investmentStore = useInvestmentAccountStore()
const route = useRoute()
const activeTab = ref(route.query.tab || 'fx')
const showModal = ref(false)
const selectedAccount = ref(null)
const notification = ref({ message: '', type: 'success' })
const fxEdits = ref({})
const showSavingModal = ref(false)
const editingSaving = ref(null)
const savingForm = ref({ name: '', type: 'Bank', currency: 'USD', isActive: true })
const showInvestmentModal = ref(false)
const editingInvestment = ref(null)
const investmentForm = ref({ name: '', currency: 'USD', isActive: true, expectedAnnualReturnPct: null })

watch(() => monthStore.months, (months) => {
  months.forEach(m => { fxEdits.value[m.id] = m.rate ?? '' })
}, { immediate: true })

onMounted(() => {
  store.fetchAccounts()
  monthStore.fetchMonths()
  savingStore.fetchAccounts()
  investmentStore.fetchAccounts()
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

function openSavingCreate() { editingSaving.value = null; savingForm.value = { name: '', type: 'Bank', currency: 'USD', isActive: true }; showSavingModal.value = true }
function openSavingEdit(a) { editingSaving.value = a; savingForm.value = { name: a.name, type: a.type, currency: a.currency, isActive: a.isActive }; showSavingModal.value = true }
async function submitSaving() {
  if (!savingForm.value.name) { showNotification('Nombre requerido', 'error'); return }
  try {
    if (editingSaving.value) {
      await savingStore.updateAccount(editingSaving.value.id, savingForm.value)
      showNotification('Cuenta actualizada', 'success')
    } else {
      await savingStore.createAccount(savingForm.value)
      showNotification('Cuenta creada', 'success')
    }
    showSavingModal.value = false
  } catch (e) { showNotification(e.message, 'error') }
}
async function deleteSaving(id) {
  if (!confirm('¿Eliminar esta cuenta?')) return
  await savingStore.deleteAccount(id)
  showNotification('Cuenta eliminada', 'success')
}

function openInvestmentCreate() { editingInvestment.value = null; investmentForm.value = { name: '', currency: 'USD', isActive: true, expectedAnnualReturnPct: null }; showInvestmentModal.value = true }
function openInvestmentEdit(a) { editingInvestment.value = a; investmentForm.value = { name: a.name, currency: a.currency, isActive: a.isActive, expectedAnnualReturnPct: a.expectedAnnualReturnPct }; showInvestmentModal.value = true }
async function submitInvestment() {
  if (!investmentForm.value.name) { showNotification('Nombre requerido', 'error'); return }
  const payload = { ...investmentForm.value, expectedAnnualReturnPct: investmentForm.value.expectedAnnualReturnPct !== '' && investmentForm.value.expectedAnnualReturnPct != null ? Number(investmentForm.value.expectedAnnualReturnPct) : null }
  try {
    if (editingInvestment.value) {
      await investmentStore.updateAccount(editingInvestment.value.id, payload)
      showNotification('Cuenta actualizada', 'success')
    } else {
      await investmentStore.createAccount(payload)
      showNotification('Cuenta creada', 'success')
    }
    showInvestmentModal.value = false
  } catch (e) { showNotification(e.message, 'error') }
}
async function deleteInvestment(id) {
  if (!confirm('¿Eliminar esta cuenta?')) return
  await investmentStore.deleteAccount(id)
  showNotification('Cuenta eliminada', 'success')
}

function showNotification(message, type) {
  notification.value = { message, type }
}
</script>
