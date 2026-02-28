<template>
  <div class="modal" @click.self="$emit('close')">
    <div class="modal-content">
      <div class="modal-header">
        <h2 class="modal-title">{{ account ? 'Editar cuenta' : 'Nueva cuenta' }}</h2>
        <button class="modal-close" @click="$emit('close')">&times;</button>
      </div>
      <form @submit.prevent="handleSubmit">
        <div class="form-group">
          <label>Nombre *</label>
          <input v-model="form.name" type="text" required maxlength="120" />
        </div>
        <div class="form-group">
          <label>Tipo *</label>
          <select v-model="form.type" required>
            <option value="Bank">Banco</option>
            <option value="Cash">Efectivo</option>
            <option value="CC">Tarjeta de Crédito</option>
          </select>
        </div>
        <div class="form-group">
          <label>Moneda *</label>
          <select v-model="form.currency" required>
            <option value="USD">USD</option>
            <option value="ARS">ARS</option>
          </select>
        </div>
        <template v-if="form.type === 'CC'">
          <div class="form-group">
            <label>Fecha de Cierre *</label>
            <input v-model="form.fechaCierre" type="date" :required="form.type === 'CC'" />
          </div>
          <div class="form-group">
            <label>Fecha de Vencimiento *</label>
            <input v-model="form.fechaVencimiento" type="date" :required="form.type === 'CC'" />
          </div>
        </template>
        <div class="form-group" v-if="account">
          <label>
            <input type="checkbox" v-model="form.isActive" />
            Activa
          </label>
        </div>
        <div class="form-actions">
          <button type="button" class="secondary" @click="$emit('close')">Cancelar</button>
          <button type="submit">{{ account ? 'Guardar' : 'Crear' }}</button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'

const props = defineProps({
  account: Object
})

const emit = defineEmits(['close', 'submit'])

const form = ref({
  name: '',
  type: 'Bank',
  currency: 'USD',
  isActive: true,
  fechaCierre: null,
  fechaVencimiento: null
})

watch(() => props.account, (newAccount) => {
  if (newAccount) {
    form.value = {
      name: newAccount.name,
      type: newAccount.type,
      currency: newAccount.currency,
      isActive: newAccount.isActive,
      fechaCierre: newAccount.fechaCierre ?? null,
      fechaVencimiento: newAccount.fechaVencimiento ?? null
    }
  }
}, { immediate: true })

function handleSubmit() {
  emit('submit', form.value)
}
</script>
