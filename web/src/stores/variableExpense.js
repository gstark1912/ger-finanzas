import { defineStore } from 'pinia'
import { ref } from 'vue'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5080'

export const useVariableExpenseStore = defineStore('variableExpense', () => {
  const expenses = ref([])
  const loading = ref(false)

  async function fetchAll(count = 3) {
    loading.value = true
    try {
      const res = await fetch(`${API_BASE_URL}/api/variable-expenses?count=${count}`)
      if (!res.ok) throw new Error('Failed to fetch variable expenses')
      expenses.value = await res.json()
    } finally {
      loading.value = false
    }
  }

  async function upsert(expenseAccountId, month, year, total) {
    const res = await fetch(
      `${API_BASE_URL}/api/variable-expenses?expenseAccountId=${expenseAccountId}&month=${month}&year=${year}`,
      {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ total })
      }
    )
    if (!res.ok) throw new Error('Failed to save')
    const updated = await res.json()
    const idx = expenses.value.findIndex(
      e => e.expenseAccountId === expenseAccountId && e.month === month && e.year === year
    )
    if (idx !== -1) expenses.value[idx] = updated
    else expenses.value.push(updated)
    return updated
  }

  async function remove(id) {
    const res = await fetch(`${API_BASE_URL}/api/variable-expenses/${id}`, { method: 'DELETE' })
    if (!res.ok) throw new Error('Failed to delete')
    expenses.value = expenses.value.filter(e => e.id !== id)
  }

  return { expenses, loading, fetchAll, upsert, remove }
})
