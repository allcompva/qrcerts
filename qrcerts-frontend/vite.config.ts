import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')

  // Solo usa proxy si la API_BASE_URL es relativa (desarrollo local)
  const useProxy = env.VITE_API_BASE_URL === '/' || !env.VITE_API_BASE_URL?.startsWith('http')

  return {
    plugins: [react()],
    base: '/app/',
    server: {
      port: 3001,
      proxy: useProxy ? {
        '/api': {
          target: 'http://localhost:5000',
          changeOrigin: true,
          secure: false,
        },
        '/uploads': {
          target: 'https://localhost:5001',
          changeOrigin: true,
          secure: false,
        },
      } : undefined,
    },
  }
})