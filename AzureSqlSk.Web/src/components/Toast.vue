<template>
  <div v-if="isVisible" 
       :class="['toast', type]" 
       @click="close">
    {{ message }}
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'

const props = defineProps<{
  message: string
  type: 'success' | 'error'
  duration?: number
}>()

const isVisible = ref(true)

const close = () => {
  isVisible.value = false
}

onMounted(() => {
  if (props.duration) {
    setTimeout(close, props.duration)
  }
})
</script>

<style scoped>
.toast {
  position: fixed;
  bottom: 20px;
  right: 20px;
  padding: 12px 24px;
  border-radius: 4px;
  color: white;
  font-weight: 500;
  cursor: pointer;
  z-index: 1000;
  animation: slideIn 0.3s ease-out;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.2);
}

.toast.success {
  background-color: #28a745;
}

.toast.error {
  background-color: #dc3545;
}

@keyframes slideIn {
  from {
    transform: translateX(100%);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
}
</style> 