import { defineStore } from 'pinia'
import { ref } from 'vue'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5080'

export const useMonthStore = defineStore('month', () => {
  const months = ref([])
  const loading = ref(false)

  async function fetchMonths(count = 6) {
    loading.value = true
    try {
      const res = await fetch(`${API_BASE_URL}/api/months?count=${count}`)
      if (!res.ok) throw new Error('Failed to fetch months')
      months.value = await res.json()
    } finally {
      loading.value = false
    }
  }

  async function updateFxRate(monthId, rate) {
    const res = await fetch(`${API_BASE_URL}/api/months/${monthId}/fx-rate`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ rate: parseFloat(rate) })
    })
    if (!res.ok) throw new Error('Failed to update fx rate')
    const month = months.value.find(m => m.id === monthId)
    if (month) month.rate = parseFloat(rate)
  }

  async function createMonth(year, month) {
    const res = await fetch(`${API_BASE_URL}/api/months`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ year, month })
    })
    if (!res.ok) throw new Error('Failed to create month')
    return res.json()
  }

  return { months, loading, fetchMonths, updateFxRate, createMonth }
})
