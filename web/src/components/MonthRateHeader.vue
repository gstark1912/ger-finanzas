<template>
  <thead>
    <tr>
      <th v-for="i in leadingCols" :key="i"></th>
      <th v-for="m in months" :key="m.year+'-'+m.month" style="text-align:center;min-width:110px;" :style="isCurrentMonth(m.year, m.month ?? m.monthNumber) ? 'background:#e8edf2;' : ''">
        {{ formatMonth(m.year, m.month ?? m.monthNumber) }}
      </th>
    </tr>
    <tr>
      <th v-for="i in leadingCols" :key="i" style="font-size:11px;color:#7f8c8d;font-weight:400;">{{ i === 1 ? 'Valor Dólar' : '' }}</th>
      <th v-for="m in months" :key="m.year+'-'+(m.month??m.monthNumber)" style="text-align:center;font-size:11px;color:#7f8c8d;font-weight:400;" :style="isCurrentMonth(m.year, m.month ?? m.monthNumber) ? 'background:#e8edf2;' : ''">
        {{ m.rate != null ? '$ ' + Number(m.rate).toLocaleString('es-AR') : '—' }}
      </th>
    </tr>
  </thead>
</template>

<script setup>
defineProps({
  months: Array,       // [{ year, month|monthNumber, rate }]
  leadingCols: { type: Number, default: 1 }
})

function formatMonth(year, month) {
  return new Date(year, month - 1).toLocaleString('es-AR', { month: 'short', year: '2-digit' })
}

function isCurrentMonth(year, month) {
  const now = new Date()
  return year === now.getFullYear() && month === now.getMonth() + 1
}
</script>
