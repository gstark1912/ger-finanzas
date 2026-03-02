<template>
  <div id="app">
    <div v-if="checking" style="display:flex;align-items:center;justify-content:center;height:100vh;color:#7f8c8d;">
      Cargando...
    </div>

    <div v-else-if="showWelcome" style="display:flex;align-items:center;justify-content:center;height:100vh;background:#f5f6fa;">
      <div style="background:white;padding:48px;border-radius:16px;box-shadow:0 4px 24px rgba(0,0,0,0.08);max-width:420px;width:100%;text-align:center;">
        <h1 style="margin-bottom:8px;">👋 Bienvenido</h1>
        <p style="color:#7f8c8d;margin-bottom:32px;">Elegí el mes con el que querés comenzar a cargar datos.</p>
        <div style="display:flex;gap:12px;justify-content:center;margin-bottom:24px;">
          <button
            v-for="opt in monthOptions" :key="opt.key"
            :class="['tab', selected === opt.key ? 'active' : '']"
            @click="selected = opt.key"
            style="flex:1;padding:12px;"
          >
            {{ opt.label }}
          </button>
        </div>
        <button @click="confirm" style="width:100%;padding:12px;">Comenzar</button>
      </div>
    </div>

    <router-view v-else />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useMonthStore } from './stores/month'

const router = useRouter()
const monthStore = useMonthStore()
const checking = ref(true)
const showWelcome = ref(false)

const now = new Date()
const monthOptions = computed(() => {
  const current = { year: now.getFullYear(), month: now.getMonth() + 1 }
  const prev = new Date(now.getFullYear(), now.getMonth() - 1)
  const previous = { year: prev.getFullYear(), month: prev.getMonth() + 1 }
  return [
    { key: `${previous.year}-${previous.month}`, label: formatMonth(previous.year, previous.month), ...previous },
    { key: `${current.year}-${current.month}`, label: formatMonth(current.year, current.month), ...current }
  ]
})

const selected = ref(null)

function formatMonth(year, month) {
  return new Date(year, month - 1).toLocaleString('es-AR', { month: 'long', year: 'numeric' })
}

onMounted(async () => {
  await monthStore.fetchMonths(1)
  if (monthStore.months.length === 0) {
    selected.value = monthOptions.value[1].key
    showWelcome.value = true
  }
  checking.value = false
})

async function confirm() {
  const opt = monthOptions.value.find(o => o.key === selected.value)
  await monthStore.createMonth(opt.year, opt.month)
  showWelcome.value = false
  router.push('/configuracion?tab=fx')
}
</script>
