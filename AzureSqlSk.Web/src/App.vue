<template>
  <div class="chat-container">
    <div class="chat-header">
      <h1>Insurance Chat Assistant</h1>
      <div class="chat-controls">
        <button @click="showHistory" class="btn btn-secondary">View History</button>
        <button @click="clearHistory" class="btn btn-danger">Clear History</button>
      </div>
    </div>
    
    <div class="chat-messages" ref="messagesContainer">
      <div v-for="(message, index) in messages" :key="index" 
           :class="['message', message.role === 'user' ? 'user-message' : 'assistant-message']">
        <div v-if="message.role === 'assistant'" class="message-icon">
          <span class="assistant-icon">🤖</span>
        </div>
        <div class="message-content">
          <div v-if="message.role === 'assistant'" class="message-role">Assistant</div>
          <div v-html="renderMarkdown(message.content)"></div>
        </div>
      </div>
    </div>

    <div class="chat-input">
      <input v-model="userInput" 
             @keyup.enter="sendMessage" 
             placeholder="Type your message..."
             :disabled="isLoading">
      <button @click="sendMessage" 
              :disabled="isLoading || !userInput.trim()"
              class="btn btn-primary">
        Send
      </button>
    </div>

    <div v-if="isLoading" class="loading">
      <div class="spinner"></div>
      <span>Thinking...</span>
    </div>

    <ChatHistoryModal 
      :is-open="isHistoryModalOpen"
      :history="chatHistory"
      @close="closeHistoryModal"
    />

    <Toast 
      v-if="toastMessage"
      :message="toastMessage"
      :type="toastType"
      :duration="3000"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, nextTick } from 'vue'
import { marked } from 'marked'
import ChatHistoryModal from './components/ChatHistoryModal.vue'
import Toast from './components/Toast.vue'

interface ChatMessage {
  role: 'system' | 'user' | 'assistant' | 'tool'
  content: string
}

const messages = ref<ChatMessage[]>([])
const userInput = ref('')
const isLoading = ref(false)
const messagesContainer = ref<HTMLElement | null>(null)
const API = "/api/chat"
const HEADERS = { 'Accept': 'application/json', 'Content-Type': 'application/json;charset=utf-8' }

// Modal state
const isHistoryModalOpen = ref(false)
const chatHistory = ref<ChatMessage[]>([])

// Toast state
const toastMessage = ref('')
const toastType = ref<'success' | 'error'>('success')

const showToast = (message: string, type: 'success' | 'error') => {
  toastMessage.value = message
  toastType.value = type
  setTimeout(() => {
    toastMessage.value = ''
  }, 3000)
}

const renderMarkdown = (content: string) => {
  return marked(content, {
    breaks: true,
    gfm: true
  })
}

const scrollToBottom = async () => {
  await nextTick()
  if (messagesContainer.value) {
    messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight
  }
}

const sendMessage = async () => {
  if (!userInput.value.trim() || isLoading.value) return

  const userMessage = userInput.value.trim()
  messages.value.push({ role: 'user', content: userMessage })
  userInput.value = ''
  await scrollToBottom()

  isLoading.value = true
  try {
    const response = await fetch(API + '/chat', {
      method: 'POST',
      headers: HEADERS,
      body: JSON.stringify({ message: userMessage }),
    })

    if (!response.ok) {
      throw new Error('Failed to get response')
    }

    const data = await response.json()
    messages.value.push({ role: 'assistant', content: data.message })
  } catch (error) {
    console.error('Error:', error)
    messages.value.push({ 
      role: 'assistant', 
      content: 'Sorry, I encountered an error. Please try again.' 
    })
  } finally {
    isLoading.value = false
    await scrollToBottom()
  }
}

const showHistory = async () => {
  try {
    const response = await fetch(API + '/history', {
      method: 'GET',
      headers: HEADERS
    })
    if (!response.ok) {
      throw new Error('Failed to get history')
    }
    const data = await response.json()
    chatHistory.value = data
    isHistoryModalOpen.value = true
  } catch (error) {
    console.error('Error:', error)
    showToast('Failed to load chat history', 'error')
  }
}

const closeHistoryModal = () => {
  isHistoryModalOpen.value = false
}

const clearHistory = async () => {
  try {
    const response = await fetch(API + '/clear', {
      method: 'POST',
      headers: HEADERS
    })
    if (!response.ok) {
      throw new Error('Failed to clear history')
    }
    messages.value = []
    showToast('Chat history cleared successfully', 'success')
  } catch (error) {
    console.error('Error:', error)
    showToast('Failed to clear chat history', 'error')
  }
}

onMounted(() => {
  scrollToBottom()
})
</script>

<style scoped>
.chat-container {
  max-width: 800px;
  margin: 0 auto;
  padding: 20px;
  height: 100vh;
  display: flex;
  flex-direction: column;
  background-color: #ffffff;
  box-sizing: border-box;
}

.chat-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  padding-bottom: 15px;
  border-bottom: 1px solid #e9ecef;
  flex-shrink: 0;
}

.chat-header h1 {
  margin: 0;
  font-size: 1.75rem;
  color: #2c3e50;
  font-weight: 600;
}

.chat-controls {
  display: flex;
  gap: 10px;
}

.chat-messages {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
  background: #f8f9fa;
  border-radius: 12px;
  margin-bottom: 20px;
  scrollbar-width: thin;
  scrollbar-color: #cbd5e0 #f8f9fa;
  min-height: 0; /* This is important for Firefox */
}

.chat-messages::-webkit-scrollbar {
  width: 6px;
}

.chat-messages::-webkit-scrollbar-track {
  background: #f8f9fa;
  border-radius: 3px;
}

.chat-messages::-webkit-scrollbar-thumb {
  background-color: #cbd5e0;
  border-radius: 3px;
}

.message {
  margin-bottom: 20px;
  max-width: 80%;
  display: flex;
  align-items: flex-start;
  gap: 8px;
}

.message.user-message {
  margin-left: auto;
  flex-direction: row-reverse;
}

.message.assistant-message {
  margin-right: auto;
}

.message-icon {
  width: 24px;
  height: 24px;
  color: #4a5568;
  flex-shrink: 0;
  margin-top: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
}

.assistant-icon {
  line-height: 1;
}

.message-content {
  padding: 12px 16px;
  border-radius: 12px;
  display: inline-block;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
}

.user-message .message-content {
  background: #2b6cb0;
  color: white;
  border-bottom-right-radius: 4px;
}

.assistant-message .message-content {
  background: white;
  color: #2d3748;
  border: 1px solid #e2e8f0;
  border-bottom-left-radius: 4px;
}

.chat-input {
  display: flex;
  gap: 12px;
  padding: 16px;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
  flex-shrink: 0;
}

input {
  flex: 1;
  padding: 12px 16px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  font-size: 16px;
  color: #2d3748;
  background: #f8fafc;
  transition: border-color 0.2s;
}

input:focus {
  outline: none;
  border-color: #2b6cb0;
  background: white;
}

input:disabled {
  background: #f1f5f9;
  cursor: not-allowed;
}

button {
  padding: 12px 24px;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 16px;
  font-weight: 500;
  transition: all 0.2s;
}

.btn-primary {
  background: #2b6cb0;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #2c5282;
}

.btn-secondary {
  background: #4a5568;
  color: white;
}

.btn-secondary:hover:not(:disabled) {
  background: #2d3748;
}

.btn-danger {
  background: #e53e3e;
  color: white;
}

.btn-danger:hover:not(:disabled) {
  background: #c53030;
}

button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.loading {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-top: 12px;
  color: #4a5568;
  flex-shrink: 0;
}

.spinner {
  width: 20px;
  height: 20px;
  border: 2px solid #e2e8f0;
  border-top: 2px solid #2b6cb0;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* Markdown styles */
.message-content :deep(p) {
  margin: 0 0 10px 0;
  line-height: 1.5;
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
  line-height: 1.5;
}

.message-content :deep(strong) {
  font-weight: 600;
  color: inherit;
}

.message-content :deep(code) {
  background: rgba(0, 0, 0, 0.05);
  padding: 2px 4px;
  border-radius: 4px;
  font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace;
  font-size: 0.9em;
}

.message-content :deep(pre) {
  background: rgba(0, 0, 0, 0.05);
  padding: 12px;
  border-radius: 6px;
  overflow-x: auto;
  margin: 10px 0;
}

.message-content :deep(pre code) {
  background: none;
  padding: 0;
  border-radius: 0;
}

.message-content :deep(blockquote) {
  border-left: 4px solid #e2e8f0;
  margin: 10px 0;
  padding-left: 12px;
  color: #4a5568;
}

.message-role {
  font-weight: 600;
  color: #2d3748;
  margin-bottom: 4px;
  font-size: 0.9em;
}
</style> 