/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Pages/**/*.cshtml",
    "./wwwroot/js/**/*.js",
  ],
  // Tailwind can't see class names built dynamically at runtime (e.g. string
  // concatenation in code-behind). Keep those static wherever possible; safelist
  // here only if a dynamic class truly can't be avoided.
  safelist: [],
  theme: {
    extend: {
      keyframes: {
        "fade-in-up": {
          "0%": { opacity: "0", transform: "translateY(14px)" },
          "100%": { opacity: "1", transform: "translateY(0)" },
        },
      },
      animation: {
        "fade-in-up": "fade-in-up 0.6s cubic-bezier(0.16, 1, 0.3, 1) both",
      },
    },
  },
  plugins: [],
};
