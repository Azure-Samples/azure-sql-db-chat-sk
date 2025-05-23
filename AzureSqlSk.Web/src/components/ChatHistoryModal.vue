<template>
  <div v-if="isOpen" class="modal-overlay" @click="close">
    <div class="modal-content" @click.stop>
      <div class="modal-header">
        <h2>Chat History</h2>
        <button class="close-button" @click="close">&times;</button>
      </div>
      <div class="modal-body">
        <div v-for="(message, index) in history" :key="index" 
             :class="['message', getMessageClass(message.role)]">
          <div class="message-role">{{ message.role }}</div>
          <div class="message-content" v-html="renderMessage(message)"></div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { marked } from 'marked'

interface ChatMessage {
  role: 'system' | 'user' | 'assistant' | 'tool'
  content: string
}

const props = defineProps<{
  isOpen: boolean
  history: ChatMessage[]
}>()

const emit = defineEmits<{
  (e: 'close'): void
}>()

const close = () => {
  emit('close')
}

const getMessageClass = (role: string) => {
  switch (role) {
    case 'user':
      return 'user-message'
    case 'assistant':
      return 'assistant-message'
    case 'system':
      return 'system-message'
    case 'tool':
      return 'tool-message'
    default:
      return ''
  }
}

const renderMessage = (message: ChatMessage) => {
  if (message.role === 'tool') {
    try {
      // Try to parse and format JSON
      const jsonContent = JSON.parse(message.content)
      return marked('```json\n' + JSON.stringify(jsonContent, null, 2) + '\n```', {
        breaks: true,
        gfm: true
      })
    } catch {
      // If not valid JSON, render as regular markdown
      return renderMarkdown(message.content)
    }
  }
  return renderMarkdown(message.content)
}

const renderMarkdown = (content: string) => {
  return marked(content, {
    breaks: true,
    gfm: true
  })
}
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal-content {
  background: white;
  border-radius: 8px;
  width: 90%;
  max-width: 800px;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.modal-header {
  padding: 15px 20px;
  border-bottom: 1px solid #dee2e6;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.modal-header h2 {
  margin: 0;
  font-size: 1.5rem;
}

.close-button {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  padding: 0;
  color: #6c757d;
}

.close-button:hover {
  color: #343a40;
}

.modal-body {
  padding: 20px;
  overflow-y: auto;
  flex: 1;
}

.message {
  margin-bottom: 20px;
  max-width: 100%;
}

.message-role {
  font-size: 0.8rem;
  font-weight: 600;
  margin-bottom: 4px;
  color: #6c757d;
}

.user-message {
  margin-left: auto;
  max-width: 80%;
}

.assistant-message {
  margin-right: auto;
  max-width: 80%;
}

.system-message {
  margin: 0 auto;
  max-width: 90%;
  background-color: #f8f9fa;
  border-radius: 8px;
  padding: 10px;
}

.tool-message {
  margin: 0 auto;
  max-width: 90%;
  background-color: #f8f9fa;
  border-radius: 8px;
  padding: 10px;
}

.message-content {
  padding: 10px 15px;
  border-radius: 15px;
  display: inline-block;
  width: 100%;
}

.user-message .message-content {
  background: #007bff;
  color: white;
}

.assistant-message .message-content {
  background: white;
  color: #333;
  border: 1px solid #dee2e6;
}

.system-message .message-content,
.tool-message .message-content {
  background: #f8f9fa;
  color: #333;
  border: 1px solid #dee2e6;
}

/* Markdown styles */
.message-content :deep(p) {
  margin: 0 0 10px 0;
}

.message-content :deep(p:last-child) {
  margin-bottom: 0;
}

.message-content :deep(ul), 
.message-content :deep(ol) {
  margin: 10px 0;
  padding-left: 20px;
}

.message-content :deep(li) {
  margin: 5px 0;
}

.message-content :deep(strong) {
  font-weight: 600;
  color: #0056b3;
}

.message-content :deep(code) {
  background: #f8f9fa;
  padding: 2px 4px;
  border-radius: 4px;
  font-family: monospace;
}

.message-content :deep(pre) {
  background: #f8f9fa;
  padding: 10px;
  border-radius: 4px;
  overflow-x: auto;
  margin: 10px 0;
}

.message-content :deep(pre code) {
  background: none;
  padding: 0;
  border-radius: 0;
}

.message-content :deep(blockquote) {
  border-left: 4px solid #dee2e6;
  margin: 10px 0;
  padding-left: 10px;
  color: #6c757d;
}
</style> 