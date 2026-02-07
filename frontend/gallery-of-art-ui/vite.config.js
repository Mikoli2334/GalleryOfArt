export default {
  server: {
    proxy: {
      "/api": {
        target: "http://localhost:5010",
        changeOrigin: true,
      },
    },
  },
};