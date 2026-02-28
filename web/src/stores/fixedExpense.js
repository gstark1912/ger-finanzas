import { defineStore } from 'pinia'
import { ref } from 'vue'

const API = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5080'

export const useFixedExpenseStore = defineStore('fixedExpense', () => {
  const definitions = ref([])
  const entries = ref([])
  const loading = ref(false)

  async function fetchAll(count = 3) {
    loading.value = true
    try {
      const [defsRes, entriesRes] = await Promise.all([
        fetch(`${API}/api/fixed-expense-definitions`),
        fetch(`${API}/api/fixed-expense-entries?count=${count}`)
      ])
      definitions.value = await defsRes.json()
      entries.value = await entriesRes.json()
    } finally {
      loading.value = false
    }
  }

  async function createDefinition(data) {
    const res = await fetch(`${API}/api/fixed-expense-definitions`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data)
    })
    if (!res.ok) { const e = await res.json(); throw new Error(e.error) }
    const def = await res.json()
    definitions.value.push(def)
    return def
  }

  async function updateDefinition(id, data) {
    const res = await fetch(`${API}/api/fixed-expense-definitions/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data)
    })
    if (!res.ok) { const e = await res.json(); throw new Error(e.error) }
    const updated = await res.json()
    const idx = definitions.value.findIndex(d => d.id === id)
    if (idx !== -1) definitions.value[idx] = updated
    return updated
  }

  async function hideDefinition(id) {
    await fetch(`${API}/api/fixed-expense-definitions/${id}`, { method: 'DELETE' })
    definitions.value = definitions.value.filter(d => d.id !== id)
    entries.value = entries.value.filter(e => e.fixedExpenseDefinitionId !== id)
  }

  async function payEntry(definitionId, monthId, amount) {
    const res = await fetch(
      `${API}/api/fixed-expense-entries?definitionId=${definitionId}&monthId=${monthId}`,
      { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ amount }) }
    )
    if (!res.ok) { const e = await res.json(); throw new Error(e.error) }
    const entry = await res.json()
    entries.value.push(entry)
    return entry
  }

  async function updateEntry(id, amount) {
    const res = await fetch(`${API}/api/fixed-expense-entries/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ amount })
    })
    if (!res.ok) throw new Error('Error al actualizar')
    const updated = await res.json()
    const idx = entries.value.findIndex(e => e.id === id)
    if (idx !== -1) entries.value[idx] = updated
    return updated
  }

  async function deleteEntry(id) {
    await fetch(`${API}/api/fixed-expense-entries/${id}`, { method: 'DELETE' })
    entries.value = entries.value.filter(e => e.id !== id)
  }

  return { definitions, entries, loading, fetchAll, createDefinition, updateDefinition, hideDefinition, payEntry, updateEntry, deleteEntry }
})
