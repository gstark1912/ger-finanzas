<template>
  <div>
    <AppNav />

    <div class="container" style="max-width:1400px;">
      <div class="header" style="display:flex;justify-content:space-between;align-items:center;">
        <h1>Dashboard</h1>
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
      <div class="kpi-row">
        <div class="kpi-card">
          <div class="kpi-label">Costo mensual ARS</div>
          <div class="kpi-value ars">$0</div>
        </div>
        <div class="kpi-card">
          <div class="kpi-label">Costo mensual USD</div>
          <div class="kpi-value usd">USD 0</div>
        </div>
        <div class="kpi-card">
          <div class="kpi-label">Δ Patrimonio USD</div>
          <div class="kpi-value usd">USD 0</div>
        </div>
        <div class="kpi-card">
          <div class="kpi-label">Promedio ahorro 6m</div>
          <div class="kpi-value usd">USD 0</div>
        </div>
      </div>

      <div class="section" style="overflow-x:auto;">
        <div v-if="loading" style="padding:24px;color:#7f8c8d;">Cargando...</div>
        <table v-else-if="summary">
          <thead>
            <tr>
              <th style="min-width:180px;">Concepto</th>
              <th v-for="m in summary.months" :key="m.id" style="text-align:right;min-width:110px;">
                {{ formatMonth(m.year, m.monthNumber) }}
              </th>
            </tr>
          </thead>
          <tbody>
            <!-- Valor dólar row -->
            <tr>
              <td>Valor dólar</td>
              <td v-for="m in summary.months" :key="m.id" style="text-align:right;">
                {{ m.rate != null ? formatNumber(m.rate) : '—' }}
              </td>
            </tr>

            <!-- Gastos Fijos group -->
            <tr class="row-group" style="cursor:pointer;" @click="fixedExpensesOpen = !fixedExpensesOpen">
              <td>{{ fixedExpensesOpen ? '▼' : '▶' }} Gastos Fijos</td>
              <td v-for="m in summary.months" :key="m.id" style="text-align:right;color:#c0392b;">
                {{ fixedExpensesTotal(m.id) !== 0 ? '-' + formatTotal(fixedExpensesTotal(m.id)) : '—' }}
              </td>
            </tr>
            <template v-if="fixedExpensesOpen">
              <tr v-for="account in summary.fixedExpenses" :key="account.accountId" class="row-subitem">
                <td>{{ account.accountName }}</td>
                <td v-for="mt in account.months" :key="mt.monthId" style="text-align:right;">
                  {{ formatTotal(mt.total) }}
                </td>
              </tr>
            </template>

            <!-- Caja group -->
            <tr class="row-group" style="cursor:pointer;" @click="cajaOpen = !cajaOpen">
              <td>{{ cajaOpen ? '▼' : '▶' }} Caja</td>
              <td v-for="m in summary.months" :key="m.id" style="text-align:right;">
                {{ formatTotal(cajaTotal(m.id)) }}
              </td>
            </tr>
            <template v-if="cajaOpen">
              <tr v-for="account in summary.savings" :key="account.accountId" class="row-subitem">
                <td>{{ account.accountName }}</td>
                <td v-for="mt in account.months" :key="mt.monthId" style="text-align:right;">
                  {{ formatTotal(mt.total) }}
                </td>
              </tr>
            </template>
          </tbody>
        </table>
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
const fixedExpensesOpen = ref(true)
const cajaOpen = ref(true)

function fixedExpensesTotal(monthId) {
  return summary.value.fixedExpenses.reduce((sum, account) => {
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

async function loadData() {
  loading.value = true
  try {
    const res = await fetch(`${API}/api/dashboard/summary?currency=${totalizarEn.value}&lastMonths=${selectedCount.value}`)
    summary.value = await res.json()
  } finally {
    loading.value = false
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
</style>
