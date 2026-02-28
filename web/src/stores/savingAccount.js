import { defineStore } from 'pinia'
import { ref } from 'vue'

const API = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5080'

export const useSavingAccountStore = defineStore('savingAccount', () => {
  const accounts = ref([])
  const loading = ref(false)

  async function fetchAccounts() {
    loading.value = true
    try {
      const res = await fetch(`${API}/api/saving-accounts`)
      accounts.value = await res.json()
    } finally {
      loading.value = false
    }
  }

  async function createAccount(data) {
    const res = await fetch(`${API}/api/saving-accounts`, {
      method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(data)
    })
    if (!res.ok) { const e = await res.json(); throw new Error(e.error) }
    const account = await res.json()
    accounts.value.push(account)
    return account
  }

  async function updateAccount(id, data) {
    const res = await fetch(`${API}/api/saving-accounts/${id}`, {
      method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(data)
    })
    if (!res.ok) { const e = await res.json(); throw new Error(e.error) }
    const updated = await res.json()
    const idx = accounts.value.findIndex(a => a.id === id)
    if (idx !== -1) accounts.value[idx] = updated
    return updated
  }

  async function deleteAccount(id) {
    await fetch(`${API}/api/saving-accounts/${id}`, { method: 'DELETE' })
    accounts.value = accounts.value.filter(a => a.id !== id)
  }

  return { accounts, loading, fetchAccounts, createAccount, updateAccount, deleteAccount }
})
