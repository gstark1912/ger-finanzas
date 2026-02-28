<template>
  <div>
    <AppNav />

    <div class="container">
      <div class="header" style="display:flex;justify-content:space-between;align-items:center;">
        <h1>Gastos Fijos</h1>
        <div style="display:flex;gap:12px;align-items:center;">
          <select v-model="totalizarEn" style="width:auto;">
            <option value="ARS">Totalizar en ARS</option>
            <option value="USD">Totalizar en USD</option>
          </select>
          <button @click="showForm = true">+ Nueva definición</button>
        </div>
      </div>

      <div v-if="store.loading">Cargando...</div>

      <div v-else class="section">
        <table>
          <thead>
            <tr>
              <th style="width:32px;"></th>
              <th style="width:10%;max-width:10%;">Concepto</th>
              <th v-for="m in months" :key="m.id" style="text-align:center;">
                {{ formatMonth(m.year, m.monthNumber) }}
              </th>
            </tr>
          </thead>
          <tbody v-for="group in groupedDefinitions" :key="group.accountId">
            <tr class="group-header-row">
              <td :colspan="3 + months.length">{{ group.accountName }}</td>
            </tr>
            <tr class="group-total-row">
              <td></td>
              <td style="font-weight:600;color:#555;">Total</td>
              <td v-for="m in months" :key="m.id" style="text-align:center;font-weight:600;">
                {{ formatTotal(accountTotal(group, m.id), totalizarEn) }}
              </td>
            </tr>
            <tr v-for="def in group.definitions" :key="def.id">
              <td>
                <button class="btn-icon" title="Ocultar" @click="hide(def.id)">✕</button>
              </td>
              <td>
                <span v-if="def.expireDay && !getEntry(def.id, latestMonthId)" :class="expireClass(def.expireDay)"
                  style="margin-right:6px;">●</span>
                <span class="def-name" :class="!getEntry(def.id, latestMonthId) ? expireClass(def.expireDay) : ''"
                  @click="openEdit(def)">{{ def.name }} - {{ def.currency }}</span>
              </td>
              <td v-for="m in months" :key="m.id" style="text-align:center;min-width:120px;">
                <CellEditor :entry="getEntry(def.id, m.id)" :isLatestMonth="m.id === latestMonthId" :currency="def.currency"
                  @pay="(amount) => pay(def.id, m.id, amount)" @update="(id, amount) => update(id, amount)"
                  @remove="(id) => remove(id)" />
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Edit definition modal -->
    <div v-if="editingDef" class="modal" @click.self="editingDef = null">
      <div class="modal-content">
        <div class="modal-header">
          <span class="modal-title">Editar definición</span>
          <button class="modal-close" @click="editingDef = null">×</button>
        </div>
        <div class="form-group">
          <label>Nombre</label>
          <input v-model="editForm.name" type="text" />
        </div>
        <div class="form-group">
          <label>Cuenta</label>
          <select v-model="editForm.expenseAccountId">
            <option v-for="a in accountStore.accounts" :key="a.id" :value="a.id">{{ a.name }}</option>
          </select>
        </div>
        <div class="form-group">
          <label>Moneda</label>
          <select v-model="editForm.currency">
            <option value="ARS">ARS</option>
            <option value="USD">USD</option>
          </select>
        </div>
        <div class="form-group">
          <label>Día de vencimiento (opcional)</label>
          <input v-model.number="editForm.expireDay" type="number" min="1" max="31" />
        </div>
        <div class="form-actions">
          <button class="secondary" @click="editingDef = null">Cancelar</button>
          <button @click="submitEdit">Guardar</button>
        </div>
      </div>
    </div>

    <!-- New definition form modal -->
    <div v-if="showForm" class="modal" @click.self="showForm = false">
      <div class="modal-content">
        <div class="modal-header">
          <span class="modal-title">Nueva definición</span>
          <button class="modal-close" @click="showForm = false">×</button>
        </div>
        <div class="form-group">
          <label>Nombre</label>
          <input v-model="form.name" type="text" />
        </div>
        <div class="form-group">
          <label>Cuenta</label>
          <select v-model="form.expenseAccountId">
            <option value="">Seleccionar...</option>
            <option v-for="a in accountStore.accounts" :key="a.id" :value="a.id">{{ a.name }}</option>
          </select>
        </div>
        <div class="form-group">
          <label>Moneda</label>
          <select v-model="form.currency">
            <option value="ARS">ARS</option>
            <option value="USD">USD</option>
          </select>
        </div>
        <div class="form-group">
          <label>Día de vencimiento (opcional)</label>
          <input v-model.number="form.expireDay" type="number" min="1" max="31" />
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
import { ref, computed, onMounted, defineComponent, h, nextTick } from 'vue'
import AppNav from '../components/AppNav.vue'
import { useFixedExpenseStore } from '../stores/fixedExpense'
import { useExpenseAccountStore } from '../stores/expenseAccount'
import { useMonthStore } from '../stores/month'
import Notification from '../components/Notification.vue'

const store = useFixedExpenseStore()
const accountStore = useExpenseAccountStore()
const monthStore = useMonthStore()

const showForm = ref(false)
const editingDef = ref(null)
const editForm = ref({ name: '', expenseAccountId: '', currency: 'ARS', expireDay: null })
const notification = ref({ message: '', type: 'success' })
const form = ref({ name: '', expenseAccountId: '', currency: 'ARS', expireDay: null })
const totalizarEn = ref('ARS')

function openEdit(def) {
  editingDef.value = def
  editForm.value = { name: def.name, expenseAccountId: def.expenseAccountId, currency: def.currency, expireDay: def.expireDay }
}

async function submitEdit() {
  if (!editForm.value.name) { notify('Nombre requerido', 'error'); return }
  try {
    await store.updateDefinition(editingDef.value.id, editForm.value)
    editingDef.value = null
    notify('Definición actualizada', 'success')
  } catch (e) {
    notify(e.message, 'error')
  }
}

onMounted(async () => {
  await Promise.all([store.fetchAll(), accountStore.fetchAccounts(), monthStore.fetchMonths()])
})

// months in ascending order (last 3)
const months = computed(() =>
  [...monthStore.months].sort((a, b) => a.year !== b.year ? a.year - b.year : a.monthNumber - b.monthNumber)
)

const latestMonthId = computed(() => months.value[months.value.length - 1]?.id)

const groupedDefinitions = computed(() => {
  const map = {}
  for (const def of store.definitions) {
    if (!map[def.expenseAccountId]) {
      const acc = accountStore.accounts.find(a => a.id === def.expenseAccountId)
      map[def.expenseAccountId] = { accountId: def.expenseAccountId, accountName: acc?.name ?? def.expenseAccountId, definitions: [] }
    }
    map[def.expenseAccountId].definitions.push(def)
  }
  return Object.values(map)
})

function getEntry(defId, monthId) {
  return store.entries.find(e => e.fixedExpenseDefinitionId === defId && e.monthId === monthId) ?? null
}

function accountTotal(group, monthId) {
  const month = monthStore.months.find(m => m.id === monthId)
  return group.definitions.reduce((sum, def) => {
    const entry = getEntry(def.id, monthId)
    if (!entry) return sum
    if (def.currency === totalizarEn.value) return sum + entry.amount
    if (totalizarEn.value === 'ARS') return sum + entry.amount * (month?.rate ?? 0)
    return sum + entry.amount / (month?.rate ?? 1)
  }, 0)
}

function formatTotal(n, currency) {
  if (n === 0) return '—'
  return currency === 'USD'
    ? 'U$S ' + n.toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
    : n.toLocaleString('es-AR', { style: 'currency', currency: 'ARS', maximumFractionDigits: 0 })
}

function formatMonth(year, month) {
  return new Date(year, month - 1).toLocaleString('es-AR', { month: 'short', year: '2-digit' })
}

function expireClass(day) {
  const today = new Date().getDate()
  const diff = day - today
  if (diff < 3) return 'expire-red'
  if (diff < 5) return 'expire-yellow'
  return ''
}

async function hide(id) {
  if (!confirm('¿Ocultar esta definición?')) return
  await store.hideDefinition(id)
}

async function pay(defId, monthId, amount) {
  try {
    await store.payEntry(defId, monthId, amount)
  } catch (e) {
    notify(e.message, 'error')
  }
}

async function update(id, amount) {
  try {
    await store.updateEntry(id, amount)
  } catch (e) {
    notify(e.message, 'error')
  }
}

async function remove(id) {
  await store.deleteEntry(id)
}

async function submitForm() {
  if (!form.value.name || !form.value.expenseAccountId) {
    notify('Nombre y cuenta son requeridos', 'error')
    return
  }
  try {
    await store.createDefinition({
      name: form.value.name,
      expenseAccountId: form.value.expenseAccountId,
      currency: form.value.currency,
      expireDay: form.value.expireDay || null
    })
    showForm.value = false
    form.value = { name: '', expenseAccountId: '', currency: 'ARS', expireDay: null }
    notify('Definición creada', 'success')
  } catch (e) {
    notify(e.message, 'error')
  }
}

function notify(message, type) {
  notification.value = { message, type }
}

// Inline cell editor component
const CellEditor = defineComponent({
  props: { entry: Object, isLatestMonth: Boolean, currency: String },
  emits: ['pay', 'update', 'remove'],
  setup(props, { emit }) {
    const editing = ref(false)
    const inputVal = ref('')

    const inputRef = ref(null)

    function startEdit() {
      inputVal.value = props.entry ? String(props.entry.amount) : ''
      editing.value = true
      nextTick(() => inputRef.value?.focus())
    }

    function confirm() {
      const amount = parseFloat(inputVal.value)
      if (isNaN(amount) || amount < 0) { editing.value = false; return }
      if (props.entry) {
        emit('update', props.entry.id, amount)
      } else {
        emit('pay', amount)
      }
      editing.value = false
    }

    function cancel() { editing.value = false }

    return () => {
      if (editing.value) {
        return h('div', { style: 'display:flex;gap:4px;justify-content:center;' }, [
          h('input', {
            ref: inputRef,
            type: 'number', value: inputVal.value,
            style: 'width:80px;padding:4px;',
            onInput: e => inputVal.value = e.target.value,
            onKeydown: e => { if (e.key === 'Enter') confirm(); if (e.key === 'Escape') cancel() }
          }),
          h('button', { class: 'btn-small', onClick: confirm }, '✓'),
          h('button', { class: 'btn-small secondary', onClick: cancel }, '✕')
        ])
      }

      if (props.entry) {
        const paidAt = new Date(props.entry.paidAt).toLocaleString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' })
        const formatted = props.currency === 'USD'
          ? 'U$S ' + props.entry.amount.toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
          : props.entry.amount.toLocaleString('es-AR', { style: 'currency', currency: 'ARS' })
        return h('div', { style: 'display:flex;gap:4px;justify-content:center;align-items:center;' }, [
          h('span', { style: 'color:#27ae60;font-weight:500;', title: `Pagado: ${paidAt}` }, formatted),
          props.isLatestMonth
            ? h('button', { class: 'btn-small secondary', style: 'padding:2px 6px;font-size:11px;', onClick: startEdit }, '✎')
            : null,
          props.isLatestMonth
            ? h('button', { class: 'btn-small btn-delete', style: 'padding:2px 6px;font-size:11px;', onClick: () => emit('remove', props.entry.id) }, '✕')
            : null
        ])
      }

      return h('button', { class: 'btn-small secondary', onClick: startEdit }, 'Pagar')
    }
  }
})
</script>

<style scoped>
.group-total-row td {
  background: #f0f4f8;
  font-size: 13px;
  padding: 8px 12px;
  border-top: 1px solid #dde3ea;
}

.group-header-row td {
  background: #ecf0f1;
  font-weight: 600;
  font-size: 13px;
  color: #34495e;
  padding: 8px 12px;
}

.btn-icon {
  background: none;
  border: none;
  color: #bdc3c7;
  cursor: pointer;
  padding: 2px 4px;
  font-size: 12px;
}

.btn-icon:hover {
  color: #e74c3c;
  background: none;
}

.expire-red {
  color: #e74c3c;
  font-weight: 700;
}

.expire-yellow {
  color: #f39c12;
}

.def-name {
  cursor: pointer;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  display: inline-block;
  max-width: 100%;
}

.def-name:hover {
  text-decoration: underline;
  color: #3498db;
}
</style>
