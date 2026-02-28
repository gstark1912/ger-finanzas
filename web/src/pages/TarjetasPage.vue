<template>
  <div>
    <AppNav />
    <div class="container" style="max-width:1400px;">
      <div class="header" style="display:flex;justify-content:space-between;align-items:center;">
        <h1>Tarjetas</h1>
        <div style="display:flex;gap:12px;align-items:center;">
          <select v-model="totalizarEn" @change="loadData" style="width:auto;">
            <option value="ARS">Totalizar en ARS</option>
            <option value="USD">Totalizar en USD</option>
          </select>
          <button @click="showForm = true">+ Nuevo Gasto</button>
        </div>
      </div>

      <div v-if="loading">Cargando...</div>
      <div v-else-if="summary" class="section" style="overflow-x:auto;">
        <table>
          <MonthRateHeader :months="summary.monthHeaders" :leadingCols="1" />
          <tbody v-for="group in summary.groups" :key="group.accountId">
            <tr class="group-header-row">
              <td :colspan="1 + summary.monthHeaders.length">{{ group.accountName }}</td>
            </tr>
            <tr v-for="inst in group.installments" :key="inst.id">
              <td style="padding-left:16px;">
                {{ inst.description }} ({{ inst.installments }}c)
                <button class="btn-icon" style="margin-left:6px;font-size:12px;opacity:0.5;" @click="deleteInstallment(inst.id)">🗑</button>
              </td>
              <td v-for="m in inst.months" :key="m.year+'-'+m.month" style="text-align:center;">
                <template v-if="m.total !== 0">
                  <span :style="m.paid ? 'color:#27ae60;font-weight:500;' : 'color:#555;'">
                    {{ formatTotal(m.total) }}
                  </span>
                  <button v-if="m.cemId && isRealMonth(m.year, m.month)" class="btn-small" :class="m.paid ? 'secondary' : ''"
                    style="margin-left:6px;font-size:11px;"
                    @click="togglePaid(m.cemId, !m.paid)">
                    {{ m.paid ? '↩' : '✓' }}
                  </button>
                </template>
                <span v-else style="color:#ccc;">—</span>
              </td>
            </tr>
            <tr class="group-total-row">
              <td style="font-weight:600;">Total {{ group.accountName }}</td>
              <td v-for="mh in summary.monthHeaders" :key="mh.year+'-'+mh.month" style="text-align:center;font-weight:600;">
                {{ formatTotal(groupTotal(group, mh.year, mh.month)) }}
              </td>
            </tr>
          </tbody>
          <tfoot>
            <tr class="grand-total-row">
              <td style="font-weight:700;">Total</td>
              <td v-for="mh in summary.monthHeaders" :key="mh.year+'-'+mh.month" style="text-align:center;font-weight:700;">
                {{ formatTotal(grandTotal(mh.year, mh.month)) }}
              </td>
            </tr>
          </tfoot>
        </table>
      </div>
    </div>

    <!-- New installment modal -->
    <div v-if="showForm" class="modal" @click.self="showForm = false">
      <div class="modal-content">
        <div class="modal-header">
          <span class="modal-title">Nuevo Gasto</span>
          <button class="modal-close" @click="showForm = false">×</button>
        </div>
        <div class="form-group">
          <label>Tarjeta</label>
          <select v-model="form.expenseAccountId">
            <option value="">Seleccionar...</option>
            <option v-for="a in ccAccounts" :key="a.id" :value="a.id">{{ a.name }}</option>
          </select>
        </div>
        <div class="form-group">
          <label>Descripción</label>
          <input v-model="form.description" type="text" />
        </div>
        <div class="form-group">
          <label>Total</label>
          <input v-model.number="form.total" type="number" min="0" />
        </div>
        <div class="form-group">
          <label>Moneda</label>
          <select v-model="form.currency">
            <option value="ARS">ARS</option>
            <option value="USD">USD</option>
          </select>
        </div>
        <div class="form-group">
          <label>Cuotas</label>
          <input v-model.number="form.installments" type="number" min="1" />
        </div>
        <div class="form-group">
          <label>Fecha</label>
          <input v-model="form.date" type="date" />
        </div>
        <div class="form-group">
          <label>Mes de inicio</label>
          <select v-model.number="form.startingMonth">
            <option :value="0">Mes actual</option>
            <option :value="1">Mes siguiente</option>
          </select>
        </div>
        <div class="form-actions">
          <button class="secondary" @click="showForm = false">Cancelar</button>
          <button @click="submitForm">Crear</button>
        </div>
      </div>
    </div>

    <Notification :message="notification.message" :type="notification.type" />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import AppNav from '../components/AppNav.vue'
import MonthRateHeader from '../components/MonthRateHeader.vue'
import Notification from '../components/Notification.vue'
import { useExpenseAccountStore } from '../stores/expenseAccount'

const API = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5080'
const accountStore = useExpenseAccountStore()

const totalizarEn = ref('ARS')
const loading = ref(false)
const summary = ref(null)
const showForm = ref(false)
const notification = ref({ message: '', type: 'success' })

const today = new Date()
const form = ref({
  expenseAccountId: '',
  description: '',
  total: 0,
  currency: 'ARS',
  installments: 1,
  date: today.toISOString().slice(0, 10),
  startingMonth: 0
})

const ccAccounts = computed(() => accountStore.accounts.filter(a => a.type === 'CC' && a.isActive))

async function loadData() {
  loading.value = true
  try {
    const res = await fetch(`${API}/api/card-installments/summary?currency=${totalizarEn.value}`)
    summary.value = await res.json()
  } finally {
    loading.value = false
  }
}

async function deleteInstallment(id) {
  if (!confirm('¿Eliminar este gasto y todas sus cuotas?')) return
  await fetch(`${API}/api/card-installments/${id}`, { method: 'DELETE' })
  await loadData()
}

function groupTotal(group, year, month) {
  return group.installments.reduce((sum, inst) => {
    const m = inst.months.find(m => m.year === year && m.month === month)
    return sum + (m?.total ?? 0)
  }, 0)
}

function grandTotal(year, month) {
  return summary.value.groups.reduce((sum, group) => sum + groupTotal(group, year, month), 0)
}

function isRealMonth(year, month) {
  return summary.value?.monthHeaders.find(h => h.year === year && h.month === month)?.isReal ?? false
}

async function togglePaid(cemId, paid) {
  await fetch(`${API}/api/card-expense-months/${cemId}/paid?paid=${paid}`, { method: 'PATCH' })
  await loadData()
}

async function submitForm() {
  if (!form.value.expenseAccountId || !form.value.description || !form.value.total) {
    notify('Tarjeta, descripción y total son requeridos', 'error')
    return
  }
  const res = await fetch(`${API}/api/card-installments`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(form.value)
  })
  if (!res.ok) {
    const err = await res.json()
    notify(err.error ?? 'Error al crear', 'error')
    return
  }
  showForm.value = false
  form.value = { expenseAccountId: '', description: '', total: 0, currency: 'ARS', installments: 1, date: today.toISOString().slice(0, 10), startingMonth: 0 }
  notify('Gasto creado', 'success')
  await loadData()
}

function formatMonth(year, month) {
  return new Date(year, month - 1).toLocaleString('es-AR', { month: 'short', year: '2-digit' })
}

function formatTotal(n) {
  if (n === 0) return '—'
  return totalizarEn.value === 'USD'
    ? 'U$S ' + n.toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
    : n.toLocaleString('es-AR', { style: 'currency', currency: 'ARS', maximumFractionDigits: 0 })
}

function notify(message, type) {
  notification.value = { message, type }
}

onMounted(async () => {
  await accountStore.fetchAccounts()
  await loadData()
})</script>

<style scoped>
.group-header-row td {
  background: #ecf0f1;
  font-weight: 600;
  font-size: 13px;
  color: #34495e;
  padding: 8px 12px;
}
.group-total-row td {
  background: #f0f4f8;
  font-size: 13px;
  padding: 8px 12px;
  border-top: 1px solid #dde3ea;
}
.grand-total-row td {
  background: #2c3e50;
  color: white;
  font-size: 14px;
  padding: 10px 12px;
  border-top: 2px solid #1a252f;
}
</style>
