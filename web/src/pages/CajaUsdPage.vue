<template>
  <div>
    <AppNav />
    <div class="container" style="max-width:1400px;">
      <div class="header" style="display:flex;justify-content:space-between;align-items:center;">
        <h1>Caja USD</h1>
        <MonthRangePicker v-model="count" @update:modelValue="loadData" />
      </div>

      <div class="section" style="overflow-x:auto;">
        <div v-if="loading" style="padding:24px;color:#7f8c8d;">Cargando...</div>
        <table v-else>
          <thead>
            <tr>
              <th style="min-width:160px;">Cuenta</th>
              <th v-for="m in months" :key="m.id" style="text-align:right;min-width:130px;">
                {{ formatMonth(m.year, m.monthNumber) }}
              </th>
            </tr>
          </thead>
          <tbody v-for="account in accounts" :key="account.id">
            <!-- Balance row -->
            <tr>
              <td style="font-weight:600;">{{ account.name }}</td>
              <td v-for="m in months" :key="m.id" style="text-align:right;">
                {{ getBalance(account.id, m.id) }}
              </td>
            </tr>
            <!-- Ingresos row -->
            <tr class="tx-row income-row">
              <td style="padding-left:24px;font-size:13px;color:#27ae60;">Ingresos</td>
              <td v-for="m in months" :key="m.id" style="text-align:right;">
                <span
                  v-if="incomeTotal(account.id, m.id) > 0"
                  class="tx-total income"
                >
                  + {{ fmt(incomeTotal(account.id, m.id)) }}
                  <span class="tooltip">{{ txTooltip(account.id, m.id, true) }}</span>
                </span>
                <button v-if="m.id === latestMonthId" class="btn-tx income" @click="openForm(account, m, true)">+</button>
                <button v-if="m.id === latestMonthId && incomeTotal(account.id, m.id) > 0" class="btn-tx clear" @click="clearTxs(account.id, m.id, true)">✕</button>
              </td>
            </tr>
            <!-- Egresos row -->
            <tr class="tx-row expense-row">
              <td style="padding-left:24px;font-size:13px;color:#e74c3c;">Egresos</td>
              <td v-for="m in months" :key="m.id" style="text-align:right;">
                <span
                  v-if="expenseTotal(account.id, m.id) < 0"
                  class="tx-total expense"
                >
                  {{ fmt(expenseTotal(account.id, m.id)) }}
                  <span class="tooltip">{{ txTooltip(account.id, m.id, false) }}</span>
                </span>
                <button v-if="m.id === latestMonthId" class="btn-tx expense" @click="openForm(account, m, false)">−</button>
                <button v-if="m.id === latestMonthId && expenseTotal(account.id, m.id) < 0" class="btn-tx clear" @click="clearTxs(account.id, m.id, false)">✕</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Transaction form modal -->
    <div v-if="form.show" class="modal" @click.self="form.show = false">
      <div class="modal-content">
        <div class="modal-header">
          <span class="modal-title">{{ form.isIncome ? 'Ingreso' : 'Egreso' }} — {{ form.accountName }}</span>
          <button class="modal-close" @click="form.show = false">×</button>
        </div>
        <div class="form-group">
          <label>Importe (USD)</label>
          <input v-model.number="form.amount" type="number" min="0" step="0.01" />
        </div>
        <div class="form-group">
          <label>Fecha</label>
          <input v-model="form.date" type="date" />
        </div>
        <div class="form-group">
          <label>Descripción (opcional)</label>
          <input v-model="form.description" type="text" />
        </div>
        <div class="form-actions">
          <button class="secondary" @click="form.show = false">Cancelar</button>
          <button @click="submitTx">Guardar</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import AppNav from '../components/AppNav.vue'
import MonthRangePicker from '../components/MonthRangePicker.vue'
import { useMonthStore } from '../stores/month'
import { useSavingAccountStore } from '../stores/savingAccount'

const API = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5080'
const monthStore = useMonthStore()
const savingStore = useSavingAccountStore()

const count = ref(3)
const loading = ref(false)
const months = ref([])
const accounts = ref([])
const balances = ref([])
const transactions = ref([])

const latestMonthId = computed(() => months.value[months.value.length - 1]?.id)

const form = ref({ show: false, isIncome: true, accountName: '', samId: null, amount: null, date: new Date().toISOString().slice(0, 10), description: '' })

function getSam(accountId, monthId) {
  return balances.value.find(b => b.savingAccountId === accountId && b.monthId === monthId)
}

function getBalance(accountId, monthId) {
  const b = getSam(accountId, monthId)
  return b != null ? fmt(b.balance) : '—'
}

function txsFor(accountId, monthId) {
  const sam = getSam(accountId, monthId)
  if (!sam) return []
  return transactions.value.filter(t => t.savingAccountMonthId === sam.id)
}

function incomeTotal(accountId, monthId) {
  return txsFor(accountId, monthId).filter(t => t.amount > 0).reduce((s, t) => s + t.amount, 0)
}

function expenseTotal(accountId, monthId) {
  return txsFor(accountId, monthId).filter(t => t.amount < 0).reduce((s, t) => s + t.amount, 0)
}

function txTooltip(accountId, monthId, isIncome) {
  return txsFor(accountId, monthId)
    .filter(t => isIncome ? t.amount > 0 : t.amount < 0)
    .map(t => `${t.date}: ${fmt(t.amount)}${t.description ? ' — ' + t.description : ''}`)
    .join('\n')
}

function fmt(n) {
  return 'U$S ' + Number(n).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

function openForm(account, month, isIncome) {
  const sam = getSam(account.id, month.id)
  if (!sam) return
  form.value = { show: true, isIncome, accountName: account.name, samId: sam.id, amount: null, date: new Date().toISOString().slice(0, 10), description: '' }
}

async function submitTx() {
  if (!form.value.amount) return
  const amount = form.value.isIncome ? Math.abs(form.value.amount) : -Math.abs(form.value.amount)
  const res = await fetch(`${API}/api/saving-account-transactions?savingAccountMonthId=${form.value.samId}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ amount, date: form.value.date, description: form.value.description || null })
  })
  if (res.ok) {
    const tx = await res.json()
    transactions.value.push(tx)
    const sam = balances.value.find(b => b.id === form.value.samId)
    if (sam) sam.balance += tx.amount
    form.value.show = false
  }
}

async function clearTxs(accountId, monthId, income) {
  const sam = getSam(accountId, monthId)
  if (!sam || !confirm(`¿Eliminar todos los ${income ? 'ingresos' : 'egresos'} de esta celda?`)) return
  const prevIncome = incomeTotal(accountId, monthId)
  const prevExpense = expenseTotal(accountId, monthId)
  await fetch(`${API}/api/saving-account-transactions?savingAccountMonthId=${sam.id}&income=${income}`, { method: 'DELETE' })
  transactions.value = transactions.value.filter(t => !(t.savingAccountMonthId === sam.id && (income ? t.amount > 0 : t.amount < 0)))
  const delta = income ? -prevIncome : -prevExpense
  const samEntry = balances.value.find(b => b.id === sam.id)
  if (samEntry) samEntry.balance += delta
}

async function loadData() {
  loading.value = true
  try {
    await Promise.all([monthStore.fetchMonths(count.value), savingStore.fetchAccounts()])
    months.value = [...monthStore.months].sort((a, b) => a.year !== b.year ? a.year - b.year : a.monthNumber - b.monthNumber)
    accounts.value = savingStore.accounts.filter(a => a.isActive)
    const [balRes, txRes] = await Promise.all([
      fetch(`${API}/api/saving-account-months?count=${count.value}`),
      fetch(`${API}/api/saving-account-transactions?count=${count.value}`)
    ])
    balances.value = await balRes.json()
    transactions.value = await txRes.json()
  } finally {
    loading.value = false
  }
}

onMounted(loadData)

function formatMonth(year, month) {
  return new Date(year, month - 1).toLocaleString('es-AR', { month: 'short', year: '2-digit' })
}
</script>

<style scoped>
.tx-row td { padding: 4px 12px; border-bottom: none; }
.tx-row:last-child td { border-bottom: 1px solid #f0f0f0; }
.tx-total { font-size: 13px; margin-right: 6px; position: relative; cursor: default; }
.tx-total.income { color: #27ae60; }
.tx-total.expense { color: #e74c3c; }
.tx-total .tooltip {
  display: none;
  position: absolute;
  right: 0;
  top: 100%;
  background: #2c3e50;
  color: white;
  font-size: 12px;
  padding: 6px 10px;
  border-radius: 6px;
  white-space: pre;
  z-index: 100;
  min-width: 180px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.2);
}
.tx-total:hover .tooltip { display: block; }
.btn-tx { padding: 2px 8px; font-size: 13px; border-radius: 4px; border: none; cursor: pointer; }
.btn-tx.income { background: #eafaf1; color: #27ae60; }
.btn-tx.income:hover { background: #27ae60; color: white; }
.btn-tx.expense { background: #fdf0f0; color: #e74c3c; }
.btn-tx.clear { background: #f5f5f5; color: #999; margin-left: 2px; }
.btn-tx.clear:hover { background: #e74c3c; color: white; }
</style>
