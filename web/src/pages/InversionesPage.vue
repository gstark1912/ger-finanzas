<template>
  <div>
    <AppNav />
    <div class="container" style="max-width:1400px;">
      <div class="header" style="display:flex;justify-content:space-between;align-items:center;">
        <h1>Inversiones</h1>
        <div style="display:flex;gap:12px;">
          <select v-model="totalizarEn" style="width:auto;">
            <option value="USD">Totalizar en USD</option>
            <option value="ARS">Totalizar en ARS</option>
          </select>
          <MonthRangePicker v-model="count" @update:modelValue="loadData" />
        </div>
      </div>

      <div class="section" style="overflow-x:auto;">
        <div v-if="loading" style="padding:24px;color:#7f8c8d;">Cargando...</div>
        <table v-else>
          <MonthRateHeader :months="periods" :leadingCols="1" />
          <tbody v-for="account in accounts" :key="account.id">
            <tr>
              <td style="font-weight:600;">
                {{ account.name }}
                <span style="font-size:11px;color:#7f8c8d;margin-left:6px;">{{ account.currency }}</span>
                <span v-if="account.expectedAnnualReturnPct != null" style="font-size:11px;color:#8e44ad;margin-left:4px;">{{ account.expectedAnnualReturnPct }}% TNA</span>
              </td>
              <td v-for="p in periods" :key="p.key" style="text-align:right;">
                <template v-if="getMonth(account.id, p)?.exists">
                  {{ fmt(getMonth(account.id, p).balance, account.currency) }}
                </template>
                <button v-else-if="isLatest(p)" class="btn-small secondary" @click="openInit(account, p)">Definir</button>
              </td>
            </tr>
            <tr class="tx-row income-row">
              <td style="padding-left:24px;font-size:13px;color:#27ae60;">Ingresos</td>
              <td v-for="p in periods" :key="p.key" style="text-align:right;">
                <template v-if="getMonth(account.id, p)?.exists">
                  <span v-if="getMonth(account.id, p).income > 0" style="font-size:13px;color:#27ae60;margin-right:6px;">
                    + {{ fmt(getMonth(account.id, p).income, account.currency) }}
                  </span>
                  <button v-if="isLatest(p)" class="btn-tx income" @click="openTx(account, p, 'income')">+</button>
                </template>
              </td>
            </tr>
            <tr class="tx-row expense-row">
              <td style="padding-left:24px;font-size:13px;color:#e74c3c;">Egresos</td>
              <td v-for="p in periods" :key="p.key" style="text-align:right;">
                <template v-if="getMonth(account.id, p)?.exists">
                  <span v-if="getMonth(account.id, p).expenses > 0" style="font-size:13px;color:#e74c3c;margin-right:6px;">
                    - {{ fmt(getMonth(account.id, p).expenses, account.currency) }}
                  </span>
                  <button v-if="isLatest(p)" class="btn-tx expense" @click="openTx(account, p, 'expenses')">−</button>
                </template>
              </td>
            </tr>
          </tbody>
          <tfoot>
            <tr class="total-row">
              <td style="font-weight:700;">Total</td>
              <td v-for="p in periods" :key="p.key" style="text-align:right;font-weight:700;">
                {{ periodTotal(p) }}
              </td>
            </tr>
          </tfoot>
        </table>
      </div>
    </div>

    <!-- Init / edit balance modal -->
    <div v-if="initForm.show" class="modal" @click.self="initForm.show = false">
      <div class="modal-content">
        <div class="modal-header">
          <span class="modal-title">Balance inicial — {{ initForm.accountName }}</span>
          <button class="modal-close" @click="initForm.show = false">×</button>
        </div>
        <div class="form-group">
          <label>Balance</label>
          <input v-model.number="initForm.balance" type="number" step="0.01" />
        </div>
        <div class="form-actions">
          <button class="secondary" @click="initForm.show = false">Cancelar</button>
          <button @click="submitInit">Guardar</button>
        </div>
      </div>
    </div>

    <!-- Income / expense modal -->
    <div v-if="txForm.show" class="modal" @click.self="txForm.show = false">
      <div class="modal-content">
        <div class="modal-header">
          <span class="modal-title">{{ txForm.field === 'income' ? 'Ingreso' : 'Egreso' }} — {{ txForm.accountName }}</span>
          <button class="modal-close" @click="txForm.show = false">×</button>
        </div>
        <div class="form-group">
          <label>Importe</label>
          <input v-model.number="txForm.amount" type="number" min="0" step="0.01" />
        </div>
        <div class="form-actions">
          <button class="secondary" @click="txForm.show = false">Cancelar</button>
          <button @click="submitTx">Guardar</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import AppNav from '../components/AppNav.vue'
import MonthRateHeader from '../components/MonthRateHeader.vue'
import MonthRangePicker from '../components/MonthRangePicker.vue'
import { useMonthStore } from '../stores/month'
import { useInvestmentAccountStore } from '../stores/investmentAccount'

const store = useInvestmentAccountStore()
const monthStore = useMonthStore()
const count = ref(3)
const loading = ref(false)
const accounts = ref([])
const months = ref([])
const totalizarEn = ref('USD')

const latestMonth = computed(() => {
  const sorted = [...monthStore.months].sort((a, b) => a.year !== b.year ? b.year - a.year : b.monthNumber - a.monthNumber)
  return sorted[0]
})

const periods = computed(() => {
  if (!latestMonth.value) return []
  return Array.from({ length: count.value }, (_, i) => {
    const d = new Date(latestMonth.value.year, latestMonth.value.monthNumber - 1 - i)
    return { month: d.getMonth() + 1, year: d.getFullYear(), monthNumber: d.getMonth() + 1, key: `${d.getFullYear()}-${d.getMonth() + 1}`, rate: i === 0 ? latestMonth.value.rate : monthStore.months.find(m => m.year === d.getFullYear() && m.monthNumber === d.getMonth() + 1)?.rate }
  }).reverse()
})

function isLatest(p) { return latestMonth.value && p.month === latestMonth.value.monthNumber && p.year === latestMonth.value.year }

function getMonth(accountId, p) {
  return months.value.find(m => m.investmentAccountId === accountId && m.month === p.month && m.year === p.year)
}

function periodTotal(p) {
  const monthData = monthStore.months.find(m => m.monthNumber === p.month && m.year === p.year)
  const rate = monthData?.rate ?? 1
  const total = accounts.value.reduce((sum, a) => {
    const m = getMonth(a.id, p)
    if (!m?.exists) return sum
    const inUsd = a.currency === 'USD' ? m.balance : m.balance / rate
    return sum + inUsd
  }, 0)
  if (totalizarEn.value === 'ARS')
    return (total * rate).toLocaleString('es-AR', { style: 'currency', currency: 'ARS', maximumFractionDigits: 0 })
  return 'U$S ' + total.toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

function fmt(n, currency = 'USD') {
  if (currency === 'ARS') return Number(n).toLocaleString('es-AR', { style: 'currency', currency: 'ARS', maximumFractionDigits: 2 })
  return 'U$S ' + Number(n).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}


// Init modal
const initForm = ref({ show: false, accountId: null, accountName: '', period: null, balance: 0 })

function openInit(account, p) {
  const existing = getMonth(account.id, p)
  initForm.value = { show: true, accountId: account.id, accountName: account.name, period: p, balance: existing?.balance ?? 0 }
}

async function submitInit() {
  const { accountId, period, balance } = initForm.value
  const existing = getMonth(accountId, period)
  await store.upsertMonth(accountId, period.month, period.year, {
    balance,
    income: existing?.income ?? 0,
    expenses: existing?.expenses ?? 0
  })
  await reloadMonths()
  initForm.value.show = false
}

// Tx modal
const txForm = ref({ show: false, accountId: null, accountName: '', period: null, field: 'income', amount: null })

function openTx(account, p, field) {
  txForm.value = { show: true, accountId: account.id, accountName: account.name, period: p, field, amount: null }
}

async function submitTx() {
  if (!txForm.value.amount) return
  const { accountId, period, field, amount } = txForm.value
  const existing = getMonth(accountId, period)
  if (!existing) return
  const delta = Math.abs(amount)
  const newIncome = field === 'income' ? existing.income + delta : existing.income
  const newExpenses = field === 'expenses' ? existing.expenses + delta : existing.expenses
  const newBalance = existing.balance + (field === 'income' ? delta : -delta)
  await store.upsertMonth(accountId, period.month, period.year, { balance: newBalance, income: newIncome, expenses: newExpenses })
  await reloadMonths()
  txForm.value.show = false
}

async function reloadMonths() {
  if (!latestMonth.value) return
  months.value = await store.fetchMonths(latestMonth.value.monthNumber, latestMonth.value.year, count.value)
}


async function loadData() {
  loading.value = true
  try {
    await Promise.all([store.fetchAccounts(), monthStore.fetchMonths(count.value)])
    accounts.value = store.accounts.filter(a => a.isActive)
    await reloadMonths()
  } finally {
    loading.value = false
  }
}

onMounted(loadData)
</script>

<style scoped>
.tx-row td { padding: 4px 12px; border-bottom: none; }
.tx-row:last-child td { border-bottom: 1px solid #f0f0f0; }
.btn-tx { padding: 2px 8px; font-size: 13px; border-radius: 4px; border: none; cursor: pointer; }
.btn-tx.income { background: #eafaf1; color: #27ae60; }
.btn-tx.income:hover { background: #27ae60; color: white; }
.btn-tx.expense { background: #fdf0f0; color: #e74c3c; }
.btn-tx.expense:hover { background: #e74c3c; color: white; }
.total-row td { background: #e8f4f8; border-top: 2px solid #b8d4e0; }
</style>
