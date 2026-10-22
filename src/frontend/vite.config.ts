import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

const apiTarget =
  process.env['CXMANAGER_API_HTTPS'] ??
  process.env['CXMANAGER_API_HTTP'] ??
  process.env['services__cxmanager-api__https__0'] ??
  process.env['services__cxmanager-api__http__0'] ??
  'http://localhost:5037'

const port = process.env['PORT'] ? Number(process.env['PORT']) : 5173

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port,
    proxy: {
      '/api': {
        target: apiTarget,
        changeOrigin: true,
      },
    },
  },
  build: {
    rollupOptions: {
      output: {
        manualChunks(id: string) {
          if (id.includes('node_modules')) {
            if (id.includes('@mui') || id.includes('@emotion')) {
              return 'mui'
            }
            if (id.includes('react') || id.includes('react-dom')) {
              return 'vendor'
            }
          }
          return undefined
        },
      },
    },
  },
})
