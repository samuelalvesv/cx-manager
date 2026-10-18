import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

const apiTarget =
  process.env['services__cxmanager-api__https__0'] ??
  process.env['services__cxmanager-api__http__0'] ??
  'http://localhost:5037'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: apiTarget,
        changeOrigin: true,
      },
    },
  },
})
