<template>
  <Chat />
</template>

<script setup lang="ts">
import Chat from './components/Chat.vue';
</script>

<style>
body {
  margin: 0;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  background-color: #f7fafc;
  color: #2d3748;
}

* {
  box-sizing: border-box;
}

.button {
  padding: 8px 16px;
  background: #2b6cb0;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 0.9rem;
  cursor: pointer;
  transition: background-color 0.2s;
}

.button:hover {
  background: #2c5282;
}
</style>
