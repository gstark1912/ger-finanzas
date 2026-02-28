<template>
  <select :value="modelValue" @change="$emit('update:modelValue', +$event.target.value)" style="width:auto;">
    <option :value="3">Últimos 3 meses</option>
    <option :value="6">Últimos 6 meses</option>
    <option :value="12">Últimos 12 meses</option>
  </select>
</template>

<script setup>
defineProps({ modelValue: { type: Number, default: 3 } })
defineEmits(['update:modelValue'])
</script>
