<template>
  <div>
    <AppNav />
    <div class="container">
      <div class="header" style="display:flex;justify-content:space-between;align-items:center;">
        <h1>Gastos Variables</h1>
        <div style="display:flex;gap:12px;align-items:center;">
          <MonthRangePicker v-model="count" @update:modelValue="reload" />
          <select v-model="totalizarEn" style="width:auto;">
            <option value="ARS">Totalizar en ARS</option>
            <option value="USD">Totalizar en USD</option>
          </select>
        </div>
      </div>

      <div v-if="loading">Cargando...</div>

      <div v-else class="section">
        <table>
          <MonthRateHeader :months="months" :leadingCols="1" />
          <tbody>
            <tr v-for="account in nonCCAccounts" :key="account.id">
              <td style="min-width:160px;font-weight:500;">{{ account.name }}</td>
              <td
                v-for="m in months" :key="m.id"
                style="text-align:center;min-width:130px;"
                :style="m.id === latestMonthId ? 'background:#e8edf2;' : ''"
              >
                <template v-if="m.id === latestMonthId">
                  <input v-if="editing" type="number" :value="getDraft(account.id, m.monthNumber, m.year)"
                    @input="setDraft(account.id, m.monthNumber, m.year, $event.target.value)"
                    style="width:100px;text-align:right;" />
                  <span v-else>{{ formatAmount(getDraft(account.id, m.monthNumber, m.year), account.currency) }}</span>
                </template>
                <span v-else>{{ formatAmount(getExpense(account.id, m.monthNumber, m.year), account.currency) }}</span>
              </td>
            </tr>
          </tbody>
        </table>

        <table style="margin-top:0;">
          <thead>
            <tr>
              <th style="min-width:160px;">Total</th>
              <th
                v-for="m in months" :key="m.id"
                style="text-align:center;min-width:130px;"
                :style="m.id === latestMonthId ? 'background:#e8edf2;' : ''"
              >
                <template v-if="m.id === latestMonthId">
                  <div style="display:flex;flex-direction:column;align-items:center;gap:6px;">
                    <span>{{ formatAmount(columnTotal(m), totalizarEn) }}</span>
                    <button @click="editing ? saveAll() : editing = true">{{ editing ? 'Guardar' : 'Editar' }}</button>
                  </div>
                </template>
                <template v-else>
                  {{ formatAmount(columnTotal(m), totalizarEn) }}
                </template>
              </th>
            </tr>
          </thead>
        </table>
      </div>
    </div>
    <Notification :message="notification.message" :type="notification.type" />

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import AppNav from '../components/AppNav.vue'
import MonthRangePicker from '../components/MonthRangePicker.vue'
import MonthRateHeader from '../components/MonthRateHeader.vue'
import Notification from '../components/Notification.vue'
import { useVariableExpenseStore } from '../stores/variableExpense'
import { useExpenseAccountStore } from '../stores/expenseAccount'
import { useMonthStore } from '../stores/month'

const store = useVariableExpenseStore()
const accountStore = useExpenseAccountStore()
const monthStore = useMonthStore()

const count = ref(3)
const totalizarEn = ref('ARS')
const notification = ref({ message: '', type: 'success' })
// draft: key = `${accountId}-${month}-${year}` => string value
const drafts = ref({})
const editing = ref(false)

const loading = computed(() => store.loading || accountStore.loading || monthStore.loading)

const months = computed(() =>
  [...monthStore.months].sort((a, b) => a.year !== b.year ? a.year - b.year : a.monthNumber - b.monthNumber)
)

const latestMonthId = computed(() => months.value[months.value.length - 1]?.id)

const nonCCAccounts = computed(() =>
  accountStore.accounts.filter(a => a.isActive && a.type !== 'CC')
)

function draftKey(accountId, month, year) {
  return `${accountId}-${month}-${year}`
}

function getExpense(accountId, month, year) {
  return store.expenses.find(e => e.expenseAccountId === accountId && e.month === month && e.year === year)?.total ?? null
}

function getDraft(accountId, month, year) {
  const key = draftKey(accountId, month, year)
  if (key in drafts.value) return drafts.value[key]
  const val = getExpense(accountId, month, year)
  return val ?? ''
}

function setDraft(accountId, month, year, value) {
  drafts.value[draftKey(accountId, month, year)] = value
}

function formatAmount(val, currency) {
  if (val === null || val === undefined || val === '') return '—'
  const n = Number(val)
  if (isNaN(n)) return '—'
  return currency === 'USD'
    ? 'U$S ' + n.toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
    : n.toLocaleString('es-AR', { style: 'currency', currency: 'ARS', maximumFractionDigits: 0 })
}

function columnTotal(m) {
  return nonCCAccounts.value.reduce((sum, account) => {
    const raw = m.id === latestMonthId.value
      ? getDraft(account.id, m.monthNumber, m.year)
      : getExpense(account.id, m.monthNumber, m.year)
    const val = parseFloat(raw)
    if (isNaN(val)) return sum
    const accCurrency = account.currency
    if (accCurrency === totalizarEn.value) return sum + val
    if (totalizarEn.value === 'ARS') return sum + val * (m.rate ?? 0)
    return sum + val / (m.rate ?? 1)
  }, 0)
}

async function saveAll() {
  const latest = months.value[months.value.length - 1]
  if (!latest) return
  const toSave = nonCCAccounts.value
    .map(account => {
      const key = draftKey(account.id, latest.monthNumber, latest.year)
      const raw = drafts.value[key]
      if (raw === undefined || raw === '') return null
      const total = parseFloat(raw)
      if (isNaN(total)) return null
      return { accountId: account.id, month: latest.monthNumber, year: latest.year, total }
    })
    .filter(Boolean)

  try {
    await Promise.all(toSave.map(({ accountId, month, year, total }) =>
      store.upsert(accountId, month, year, total)
    ))
    // clear drafts for latest month
    nonCCAccounts.value.forEach(a => {
      delete drafts.value[draftKey(a.id, latest.monthNumber, latest.year)]
    })
    notification.value = { message: 'Guardado', type: 'success' }
    editing.value = false
  } catch (e) {
    notification.value = { message: e.message, type: 'error' }
  }
}

async function reload() {
  await Promise.all([store.fetchAll(count.value), monthStore.fetchMonths(count.value)])
}

onMounted(async () => {
  await Promise.all([store.fetchAll(count.value), accountStore.fetchAccounts(true), monthStore.fetchMonths(count.value)])
})
</script>

<style scoped>
.btn-micro {
  background: none;
  border: none;
  cursor: pointer;
  font-size: 11px;
  padding: 1px 3px;
  color: #7f8c8d;
  line-height: 1;
}
.btn-micro:hover { color: #2980b9; }
.btn-micro-delete:hover { color: #e74c3c; }
</style>
