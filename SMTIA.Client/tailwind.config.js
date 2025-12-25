/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{js,jsx,ts,tsx}",
  ],
  theme: {
    extend: {
      fontFamily: {
        'pp-neue': ['PP Neue Montreal', 'system-ui', 'sans-serif'],
      },
      animation: {
        'fadeInUp': 'fadeInUp 0.8s ease-out',
        'loadingDots': 'loadingDots 1.4s infinite ease-in-out',
        'shimmer': 'shimmer 3s ease-in-out infinite',
      },
      keyframes: {
        fadeInUp: {
          'from': {
            opacity: '0',
            transform: 'translateY(20px)',
          },
          'to': {
            opacity: '1',
            transform: 'translateY(0)',
          },
        },
        loadingDots: {
          '0%, 80%, 100%': {
            opacity: '0.3',
            transform: 'scale(0.8)',
          },
          '40%': {
            opacity: '1',
            transform: 'scale(1)',
          },
        },
        shimmer: {
          '0%, 100%': {
            transform: 'translateX(-100%)',
            opacity: '0',
          },
          '50%': {
            transform: 'translateX(100%)',
            opacity: '1',
          },
        },
      },
    },
  },
  plugins: [],
}
