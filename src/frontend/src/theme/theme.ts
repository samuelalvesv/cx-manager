import { ptBR as corePtBR } from '@mui/material/locale'
import { createTheme } from '@mui/material/styles'
import { ptBR as pickersPtBR } from '@mui/x-date-pickers/locales'

export const theme = createTheme(
  {
    cssVariables: {
      colorSchemeSelector: 'class',
    },
  colorSchemes: {
    light: {
      palette: {
        primary: {
          main: '#0f766e',
          light: '#14b8a6',
          dark: '#0f5257',
          contrastText: '#ffffff',
        },
        secondary: {
          main: '#6366f1',
          light: '#818cf8',
          dark: '#4338ca',
          contrastText: '#ffffff',
        },
        background: {
          default: '#f8fafc',
          paper: '#ffffff',
        },
        text: {
          primary: '#0f172a',
          secondary: '#475569',
        },
        divider: '#e2e8f0',
      },
    },
    dark: {
      palette: {
        primary: {
          main: '#2dd4bf',
          light: '#5eead4',
          dark: '#0f766e',
          contrastText: '#042f2e',
        },
        secondary: {
          main: '#818cf8',
          light: '#a5b4fc',
          dark: '#4f46e5',
          contrastText: '#ffffff',
        },
        background: {
          default: '#0b1120',
          paper: '#1e293b',
        },
        text: {
          primary: '#f8fafc',
          secondary: '#94a3b8',
        },
        divider: 'rgba(148, 163, 184, 0.16)',
      },
    },
  },
  typography: {
    fontFamily: [
      '-apple-system',
      'BlinkMacSystemFont',
      '"Segoe UI"',
      'Roboto',
      '"Helvetica Neue"',
      'Arial',
      'sans-serif',
    ].join(','),
    h6: {
      fontWeight: 600,
    },
  },
  shape: {
    borderRadius: 10,
  },
  components: {
    MuiAppBar: {
      defaultProps: {
        elevation: 0,
      },
      styleOverrides: {
        root: {
          backgroundColor: 'var(--mui-palette-background-paper)',
          color: 'var(--mui-palette-text-primary)',
          borderBottom: '1px solid var(--mui-palette-divider)',
          backgroundImage: 'none',
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          backgroundImage: 'none',
        },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: {
          backgroundImage: 'none',
        },
      },
    },
    MuiDialog: {
      defaultProps: {
        transitionDuration: {
          enter: 240,
          exit: 200,
        },
      },
      styleOverrides: {
        paper: ({ theme: currentTheme }) => ({
          margin: currentTheme.spacing(2),
          width: `calc(100% - ${currentTheme.spacing(4)})`,
          backgroundImage: 'none',
        }),
      },
    },
    MuiCssBaseline: {
      styleOverrides: {
        '.MuiOutlinedInput-notchedOutline legend, .MuiPickersOutlinedInput-notchedOutline legend': {
          transition: 'none !important',
        },
      },
    },
    MuiOutlinedInput: {
      styleOverrides: {
        notchedOutline: {
          '& legend': {
            transition: 'none',
          },
        },
      },
    },
    MuiButton: {
      defaultProps: {
        disableElevation: true,
      },
    },
  },
}, corePtBR, pickersPtBR)
