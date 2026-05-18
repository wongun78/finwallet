/** @type {import('tailwindcss').Config} */
export default {
  content: ["./index.html", "./src/**/*.{ts,tsx}"],
  theme: {
    extend: {
      colors: {
        ink: "#0f172a",
        slate: "#1f2937",
        mist: "#e2e8f0",
        mint: "#22c55e",
        gold: "#f59e0b",
        sky: "#0ea5e9"
      },
      fontFamily: {
        display: ["Space Grotesk", "sans-serif"]
      },
      boxShadow: {
        soft: "0 20px 60px rgba(15, 23, 42, 0.18)"
      }
    }
  },
  plugins: []
};
