<template>
  <div>
    <AppNav />

    <div class="container" style="max-width:1400px;">
      <div class="header" style="display:flex;justify-content:space-between;align-items:center;">
        <h1>Dashboard <button @click="hideNumbers = !hideNumbers" class="btn-eye" :title="hideNumbers ? 'Mostrar números' : 'Ocultar números'">{{ hideNumbers ? '👁️' : '🙈' }}</button></h1>
        <div style="display:flex;gap:12px;">
          <select v-model="totalizarEn" style="width:auto;">
            <option value="ARS">Totalizar en ARS</option>
            <option value="USD">Totalizar en USD</option>
          </select>
          <select v-model="selectedCount" @change="loadData" style="width:auto;">
            <option :value="3">Últimos 3 meses</option>
            <option :value="6">Últimos 6 meses</option>
            <option :value="12">Últimos 12 meses</option>
          </select>
        </div>
      </div>

      <!-- KPIs -->
      <div class="kpi-row" v-if="summary">
        <div class="kpi-card">
          <div class="kpi-label">Costo mensual ARS</div>
          <div class="kpi-value ars">{{ hideNumbers ? '****' : summary.kpiCostoMensualArs.toLocaleString('es-AR', { style: 'currency', currency: 'ARS', maximumFractionDigits: 0 }) }}</div>
        </div>
        <div class="kpi-card">
          <div class="kpi-label">Costo mensual USD</div>
          <div class="kpi-value usd">{{ hideNumbers ? '****' : 'U$S ' + summary.kpiCostoMensualUsd.toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) }}</div>
        </div>
        <div class="kpi-card">
          <div class="kpi-label">Δ Patrimonio USD</div>
          <div class="kpi-value" :style="summary.kpiPatrimonioUsdDelta >= 0 ? 'color:#27ae60' : 'color:#c0392b'">{{ hideNumbers ? '****' : (summary.kpiPatrimonioUsdDelta >= 0 ? '+' : '') + 'U$S ' + summary.kpiPatrimonioUsdDelta.toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) }}</div>
        </div>
        <div class="kpi-card">
          <div class="kpi-label">Promedio ahorro 6m</div>
          <div class="kpi-value usd">{{ hideNumbers ? '****' : 'USD 0' }}</div>
        </div>
      </div>

      <div class="section" style="overflow-x:auto;">
        <div v-if="loading" style="padding:24px;color:#7f8c8d;">Cargando...</div>
        <table v-else-if="summary">
          <thead>
            <tr>
              <th style="min-width:180px;">Concepto</th>
              <th v-for="m in summary.months" :key="m.id" :class="{ 'current-month': isCurrentMonth(m) }" style="text-align:right;min-width:110px;">
                {{ formatMonth(m.year, m.monthNumber) }}
                <span v-if="m.isClosed" title="Mes cerrado">🔒</span>
                <button v-else-if="isLastOpenMonth(m)" @click="closeMonth(m)" :disabled="closingMonth" class="btn-close-month" title="Cerrar mes">Cerrar mes</button>
              </th>
            </tr>
          </thead>
          <tbody>
            <!-- Valor dólar row -->
            <tr>
              <td>Valor dólar</td>
              <td v-for="m in summary.months" :key="m.id" :class="{ 'current-month': isCurrentMonth(m) }" style="text-align:right;">
                {{ m.rate != null ? (hideNumbers ? '****' : formatNumber(m.rate)) : '—' }}
              </td>
            </tr>

            <!-- Gastos Fijos group -->
            <tr class="row-group" style="cursor:pointer;" @click="fixedExpensesOpen = !fixedExpensesOpen">
              <td>{{ fixedExpensesOpen ? '▼' : '▶' }} Gastos Fijos</td>
              <td v-for="m in summary.months" :key="m.id" :class="{ 'current-month': isCurrentMonth(m) }" style="text-align:right;color:#c0392b;">
                {{ fixedExpensesTotal(m.id) !== 0 ? (hideNumbers ? '****' : '-' + formatTotal(fixedExpensesTotal(m.id))) : '—' }}
              </td>
            </tr>
            <template v-if="fixedExpensesOpen">
              <tr v-for="account in summary.fixedExpenses" :key="account.accountId" class="row-subitem">
                <td>{{ account.accountName }}</td>
                <td v-for="mt in account.months" :key="mt.monthId" :class="{ 'current-month': isCurrentMonth(mt) }" style="text-align:right;">
                  {{ hideNumbers ? '****' : formatTotal(mt.total) }}
                </td>
              </tr>
            </template>

            <!-- Gastos Variables group -->
            <tr class="row-group" style="cursor:pointer;" @click="variableExpensesOpen = !variableExpensesOpen">
              <td>{{ variableExpensesOpen ? '▼' : '▶' }} Gastos Variables</td>
              <td v-for="m in summary.months" :key="m.id" :class="{ 'current-month': isCurrentMonth(m) }" style="text-align:right;color:#c0392b;">
                {{ variableExpensesTotal(m.id) !== 0 ? (hideNumbers ? '****' : '-' + formatTotal(variableExpensesTotal(m.id))) : '—' }}
              </td>
            </tr>
            <template v-if="variableExpensesOpen">
              <tr v-for="account in summary.variableExpenses" :key="account.accountId" class="row-subitem">
                <td>{{ account.accountName }}</td>
                <td v-for="mt in account.months" :key="mt.monthId" :class="{ 'current-month': isCurrentMonth(mt) }" style="text-align:right;">
                  {{ hideNumbers ? '****' : formatTotal(mt.total) }} <span v-if="mt.unpaid" title="Sin pagar">⚠️</span>
                </td>
              </tr>
            </template>

            <!-- Total gastos -->
            <tr class="row-total-gastos">
              <td>Total gastos</td>
              <td v-for="m in summary.months" :key="m.id" :class="{ 'current-month': isCurrentMonth(m) }" style="text-align:right;">
                {{ (fixedExpensesTotal(m.id) + variableExpensesTotal(m.id)) !== 0 ? (hideNumbers ? '****' : '-' + formatTotal(fixedExpensesTotal(m.id) + variableExpensesTotal(m.id))) : '—' }}
              </td>
            </tr>

            <!-- Spacer -->
            <tr class="row-spacer"><td colspan="999"></td></tr>

            <!-- Caja group -->
            <tr class="row-group" style="cursor:pointer;" @click="cajaOpen = !cajaOpen">
              <td>{{ cajaOpen ? '▼' : '▶' }} Caja</td>
              <td v-for="m in summary.months" :key="m.id" :class="{ 'current-month': isCurrentMonth(m) }" style="text-align:right;">
                {{ hideNumbers ? '****' : formatTotal(cajaTotal(m.id)) }}
              </td>
            </tr>
            <template v-if="cajaOpen">
              <tr v-for="account in summary.savings" :key="account.accountId" class="row-subitem">
                <td>{{ account.accountName }}</td>
                <td v-for="mt in account.months" :key="mt.monthId" :class="{ 'current-month': isCurrentMonth(mt) }" style="text-align:right;">
                  {{ hideNumbers ? '****' : formatTotal(mt.total) }}
                </td>
              </tr>
            </template>

            <!-- Inversiones group -->
            <tr class="row-group" style="cursor:pointer;" @click="inversionesOpen = !inversionesOpen">
              <td>{{ inversionesOpen ? '▼' : '▶' }} Inversiones</td>
              <td v-for="m in summary.months" :key="m.id" :class="{ 'current-month': isCurrentMonth(m) }" style="text-align:right;">
                {{ hideNumbers ? '****' : formatTotal(inversionesTotal(m.id)) }}
              </td>
            </tr>
            <template v-if="inversionesOpen">
              <tr v-for="account in summary.investments" :key="account.accountId" class="row-subitem">
                <td>{{ account.accountName }}</td>
                <td v-for="mt in account.months" :key="mt.monthId" :class="{ 'current-month': isCurrentMonth(mt) }" style="text-align:right;">
                  {{ hideNumbers ? '****' : formatTotal(mt.total) }}
                </td>
              </tr>
            </template>

            <!-- Patrimonio total -->
            <tr class="row-patrimonio" style="font-weight:700;font-size:14px;">
              <td>Patrimonio total</td>
              <td v-for="m in summary.months" :key="m.id" :class="{ 'current-month': isCurrentMonth(m) }" style="text-align:right;">
                {{ hideNumbers ? '****' : formatTotal(cajaTotal(m.id) + inversionesTotal(m.id)) }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Modal cierre de mes -->
    <div v-if="closeModal.open" class="modal" @click.self="closeModal.open = false">
      <div class="modal-content">
        <!-- Paso 1: advertencias -->
        <template v-if="closeModal.step === 1">
          <div class="modal-header">
            <span class="modal-title">Cerrar {{ formatMonth(closeModal.month?.year, closeModal.month?.monthNumber) }}</span>
            <button class="modal-close" @click="closeModal.open = false">×</button>
          </div>
          <div v-if="closeModal.loading" style="padding:16px;color:#7f8c8d;">Verificando...</div>
          <template v-else>
            <div v-if="closeModal.unpaidFixed.length || closeModal.unpaidCards.length" class="modal-warnings">
              <p style="color:#c0392b;font-weight:600;">⚠️ Hay ítems pendientes de pago:</p>
              <ul>
                <li v-for="name in closeModal.unpaidFixed" :key="name">Gasto fijo: <strong>{{ name }}</strong></li>
                <li v-for="name in closeModal.unpaidCards" :key="name">Tarjeta: <strong>{{ name }}</strong></li>
              </ul>
              <p style="font-size:13px;color:#7f8c8d;">Resolvé los pendientes antes de cerrar el mes.</p>
            </div>
            <p v-else style="color:#27ae60;">✅ No hay ítems pendientes de pago.</p>
            <div class="form-actions">
              <button class="secondary" @click="closeModal.open = false">Cancelar</button>
              <button @click="closeModal.step = 2" :disabled="closeModal.unpaidFixed.length > 0 || closeModal.unpaidCards.length > 0">Continuar →</button>
            </div>
          </template>
        </template>
        <!-- Paso 2: confirmación final -->
        <template v-else-if="closeModal.step === 2">
          <div class="modal-header">
            <span class="modal-title">⚠️ Confirmar cierre</span>
            <button class="modal-close" @click="closeModal.open = false">×</button>
          </div>
          <p>Estás por cerrar <strong>{{ formatMonth(closeModal.month?.year, closeModal.month?.monthNumber) }}</strong>.</p>
          <p style="color:#c0392b;font-size:13px;margin-top:8px;">Este proceso <strong>no tiene vuelta atrás</strong>. No se podrá editar información de un mes cerrado.</p>
          <div class="form-actions">
            <button class="secondary" @click="closeModal.step = 1">← Volver</button>
            <button class="btn-danger" @click="confirmClose" :disabled="closingMonth">{{ closingMonth ? 'Cerrando...' : 'Cerrar mes' }}</button>
          </div>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import AppNav from '../components/AppNav.vue'

const API = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5080'

const selectedCount = ref(6)
const totalizarEn = ref('ARS')
const loading = ref(false)
const summary = ref(null)
const fixedExpensesOpen = ref(false)
const variableExpensesOpen = ref(false)
const cajaOpen = ref(false)
const inversionesOpen = ref(false)

const hideNumbers = ref(false)

const now = new Date()
function isCurrentMonth(m) {
  return m.year === now.getFullYear() && m.monthNumber === now.getMonth() + 1
}

function isLastOpenMonth(m) {
  if (!summary.value) return false
  const openMonths = summary.value.months.filter(x => !x.isClosed)
  if (openMonths.length === 0) return false
  const last = openMonths.reduce((a, b) => (a.year * 100 + a.monthNumber) > (b.year * 100 + b.monthNumber) ? a : b)
  return last.year === m.year && last.monthNumber === m.monthNumber
}

function fixedExpensesTotal(monthId) {
  return summary.value.fixedExpenses.reduce((sum, account) => {
    const mt = account.months.find(m => m.monthId === monthId)
    return sum + (mt?.total ?? 0)
  }, 0)
}

function variableExpensesTotal(monthId) {
  return summary.value.variableExpenses.reduce((sum, account) => {
    const mt = account.months.find(m => m.monthId === monthId)
    return sum + (mt?.total ?? 0)
  }, 0)
}

function cajaTotal(monthId) {
  return summary.value.savings.reduce((sum, account) => {
    const mt = account.months.find(m => m.monthId === monthId)
    return sum + (mt?.total ?? 0)
  }, 0)
}

function inversionesTotal(monthId) {
  return summary.value.investments.reduce((sum, account) => {
    const mt = account.months.find(m => m.monthId === monthId)
    return sum + (mt?.total ?? 0)
  }, 0)
}

const closingMonth = ref(false)
const closeModal = ref({ open: false, step: 1, month: null, loading: false, unpaidFixed: [], unpaidCards: [] })

async function loadData() {
  loading.value = true
  try {
    const res = await fetch(`${API}/api/dashboard/summary?currency=${totalizarEn.value}&lastMonths=${selectedCount.value}`)
    summary.value = await res.json()
  } finally {
    loading.value = false
  }
}

async function closeMonth(m) {
  closeModal.value = { open: true, step: 1, month: m, loading: true, unpaidFixed: [], unpaidCards: [] }
  try {
    const res = await fetch(`${API}/api/monthly-snapshots/close-check/${m.year}/${m.monthNumber}`)
    const data = await res.json()
    closeModal.value.unpaidFixed = data.unpaidFixedExpenses ?? []
    closeModal.value.unpaidCards = data.unpaidCards ?? []
  } finally {
    closeModal.value.loading = false
  }
}

async function confirmClose() {
  closingMonth.value = true
  try {
    const { year, monthNumber } = closeModal.value.month
    const res = await fetch(`${API}/api/monthly-snapshots/close`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ year, month: monthNumber })
    })
    if (!res.ok) {
      const err = await res.json()
      alert(err.error || 'Error al cerrar el mes')
      return
    }
    closeModal.value.open = false
    await loadData()
  } finally {
    closingMonth.value = false
  }
}

watch([selectedCount, totalizarEn], loadData)
onMounted(loadData)

function formatMonth(year, month) {
  return new Date(year, month - 1).toLocaleString('es-AR', { month: 'short', year: '2-digit' })
}

function formatNumber(n) {
  return Number(n).toLocaleString('es-AR')
}

function formatTotal(n) {
  if (n === 0) return '—'
  return totalizarEn.value === 'USD'
    ? 'U$S ' + n.toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
    : n.toLocaleString('es-AR', { style: 'currency', currency: 'ARS', maximumFractionDigits: 0 })
}
</script>

<style scoped>
.kpi-row {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 20px;
  margin-bottom: 32px;
}
.kpi-card {
  background: white;
  padding: 24px;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.04);
}
.kpi-label {
  font-size: 13px;
  color: #7f8c8d;
  margin-bottom: 8px;
}
.kpi-value {
  font-size: 28px;
  font-weight: 600;
}
.kpi-value.usd { color: #27ae60; }
.kpi-value.ars { color: #2c3e50; }
.row-group td {
  background: #ecf0f1;
  font-weight: 600;
  font-size: 13px;
  color: #34495e;
}
.row-subitem td:first-child {
  padding-left: 28px;
  color: #555;
}
th.current-month, td.current-month {
  background: var(--current-month-cell);
}
th.current-month {
  background: var(--current-month-header);
}
.row-total-gastos td {
  background: #922b21;
  color: white;
  font-weight: 700;
  font-size: 14px;
}
.row-spacer td {
  background: transparent;
  height: 20px;
  padding: 0;
}
.row-patrimonio td {
  background: #2c3e50;
  color: white;
  font-weight: 700;
  font-size: 14px;
}
.btn-close-month {
  display: block;
  margin: 4px auto 0;
  font-size: 10px;
  padding: 2px 6px;
  background: #2c3e50;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}
.btn-close-month:disabled { opacity: 0.5; cursor: not-allowed; }
.btn-eye {
  background: white;
  border: 1px solid #ccc;
  border-radius: 6px;
  padding: 4px 10px;
  cursor: pointer;
  font-size: 16px;
  line-height: 1;
  vertical-align: middle;
  margin-left: 8px;
}
</style>
