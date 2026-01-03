import { defineConfig } from "vite";
import react from "@vitejs/plugin-react-swc";

export default {
  server: {
    host: true,
    port: 5173,
    proxy: {
      "/api": {
        target: "http://api:8080",
        changeOrigin: true,
      },
    },
  },
}