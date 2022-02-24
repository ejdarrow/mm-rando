module.exports = {
  content: ['./src/**/*.{js,jsx,ts,tsx}'],
  theme: {
    extend: {
      colors: {
        'darker-1/4': 'rgba(0, 0, 0, 0.25)',
        'darker-3/4': 'rgba(0, 0, 0, 0.75)',
        'darker-1/2': 'rgba(0, 0, 0, 0.5)',
      },
      margin: {
        '2px': '2px',
        '3px': '3px',
        '4px': '4px',
        '5px': '5px',
        '6px': '6px',
        '7px': '7px',
        '8px': '8px',
        '9px': '9px',
        '10px': '10px',
      },
      padding: {
        '2px': '2px',
        '3px': '3px',
        '4px': '4px',
        '5px': '5px',
        '6px': '6px',
        '7px': '7px',
        '8px': '8px',
        '9px': '9px',
        '10px': '10px',
      },
    },
    maxWidth: {
      '8xl': '90rem',
      '1/2': '50%',
    },
    minWidth: {
      '1/2': '50%',
    }
  },
  plugins: [],
}
