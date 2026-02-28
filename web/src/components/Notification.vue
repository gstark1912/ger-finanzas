<template>
  <div v-if="visible" :class="['notification', type]">
    {{ message }}
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'

const props = defineProps({
  message: String,
  type: {
    type: String,
    default: 'success'
  },
  duration: {
    type: Number,
    default: 3000
  }
})

const visible = ref(false)

watch(() => props.message, (newMessage) => {
  if (newMessage) {
    visible.value = true
    setTimeout(() => {
      visible.value = false
    }, props.duration)
  }
})
</script>
