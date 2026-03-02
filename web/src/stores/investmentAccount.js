import { defineStore } from 'pinia'
import { ref } from 'vue'

const API = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5080'

export const useInvestmentAccountStore = defineStore('investmentAccount', () => {
  const accounts = ref([])
  const loading = ref(false)

  async function fetchAccounts() {
    loading.value = true
    try {
      const res = await fetch(`${API}/api/investment-accounts`)
      accounts.value = await res.json()
    } finally {
      loading.value = false
    }
  }

  async function createAccount(data) {
    const res = await fetch(`${API}/api/investment-accounts`, {
      method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(data)
    })
    if (!res.ok) { const e = await res.json(); throw new Error(e.error) }
    const account = await res.json()
    accounts.value.push(account)
    return account
  }

  async function updateAccount(id, data) {
    const res = await fetch(`${API}/api/investment-accounts/${id}`, {
      method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(data)
    })
    if (!res.ok) { const e = await res.json(); throw new Error(e.error) }
    const updated = await res.json()
    const idx = accounts.value.findIndex(a => a.id === id)
    if (idx !== -1) accounts.value[idx] = updated
    return updated
  }

  async function deleteAccount(id) {
    await fetch(`${API}/api/investment-accounts/${id}`, { method: 'DELETE' })
    accounts.value = accounts.value.filter(a => a.id !== id)
  }

  async function fetchMonths(month, year, count = 3) {
    const res = await fetch(`${API}/api/investment-account-months?month=${month}&year=${year}&count=${count}`)
    return res.json()
  }

  async function upsertMonth(investmentAccountId, month, year, data) {
    await fetch(`${API}/api/investment-account-months?investmentAccountId=${investmentAccountId}&month=${month}&year=${year}`, {
      method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(data)
    })
  }

  return { accounts, loading, fetchAccounts, createAccount, updateAccount, deleteAccount, fetchMonths, upsertMonth }
})
