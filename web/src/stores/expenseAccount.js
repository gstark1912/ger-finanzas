import { defineStore } from 'pinia'
import { ref } from 'vue'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5080'

export const useExpenseAccountStore = defineStore('expenseAccount', () => {
  const accounts = ref([])
  const loading = ref(false)
  const error = ref(null)

  async function fetchAccounts(activeOnly = false) {
    loading.value = true
    error.value = null
    try {
      const url = `${API_BASE_URL}/api/expense-accounts${activeOnly ? '?activeOnly=true' : ''}`
      const response = await fetch(url)
      if (!response.ok) throw new Error('Failed to fetch accounts')
      accounts.value = await response.json()
    } catch (e) {
      error.value = e.message
      throw e
    } finally {
      loading.value = false
    }
  }

  async function createAccount(data) {
    loading.value = true
    error.value = null
    try {
      const response = await fetch(`${API_BASE_URL}/api/expense-accounts`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
      })
      if (!response.ok) {
        const err = await response.json()
        throw new Error(err.error || 'Failed to create account')
      }
      const newAccount = await response.json()
      accounts.value.push(newAccount)
      return newAccount
    } catch (e) {
      error.value = e.message
      throw e
    } finally {
      loading.value = false
    }
  }

  async function updateAccount(id, data) {
    loading.value = true
    error.value = null
    try {
      const response = await fetch(`${API_BASE_URL}/api/expense-accounts/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
      })
      if (!response.ok) {
        const err = await response.json()
        throw new Error(err.error || 'Failed to update account')
      }
      const updated = await response.json()
      const index = accounts.value.findIndex(a => a.id === id)
      if (index !== -1) accounts.value[index] = updated
      return updated
    } catch (e) {
      error.value = e.message
      throw e
    } finally {
      loading.value = false
    }
  }

  async function deleteAccount(id) {
    loading.value = true
    error.value = null
    try {
      const response = await fetch(`${API_BASE_URL}/api/expense-accounts/${id}`, {
        method: 'DELETE'
      })
      if (!response.ok) throw new Error('Failed to delete account')
      accounts.value = accounts.value.filter(a => a.id !== id)
    } catch (e) {
      error.value = e.message
      throw e
    } finally {
      loading.value = false
    }
  }

  return {
    accounts,
    loading,
    error,
    fetchAccounts,
    createAccount,
    updateAccount,
    deleteAccount
  }
})
