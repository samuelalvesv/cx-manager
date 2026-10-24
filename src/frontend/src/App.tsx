import {
  Box,
  Container,
  CssBaseline,
  Skeleton,
  ThemeProvider,
} from '@mui/material'
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import dayjs from 'dayjs'
import ptBrLocale from 'dayjs/locale/pt-br'
import { lazy, Suspense, useState } from 'react'
import { AppBar, type ViewType } from './components/AppBar.tsx'
import { Footer } from './components/Footer.tsx'
import { CompanyProvider } from './context/CompanyContext.tsx'
import { theme } from './theme/theme.ts'

dayjs.locale(ptBrLocale)

const ContactsPage = lazy(() => import('./pages/ContactsPage.tsx').then((m) => ({ default: m.ContactsPage })))
const SurveysPage = lazy(() => import('./pages/SurveysPage.tsx').then((m) => ({ default: m.SurveysPage })))
const DashboardPage = lazy(() => import('./pages/DashboardPage.tsx').then((m) => ({ default: m.DashboardPage })))

function PageLoadingFallback() {
  return (
    <Container maxWidth="lg" sx={{ py: { xs: 2.5, sm: 3, md: 4 } }}>
      <Skeleton variant="rounded" height={60} sx={{ mb: 3 }} />
      <Skeleton variant="rounded" height={280} />
    </Container>
  )
}

function MainContent({ currentView }: { readonly currentView: ViewType }) {
  if (currentView === 'surveys') {
    return (
      <Suspense fallback={<PageLoadingFallback />}>
        <SurveysPage />
      </Suspense>
    )
  }

  if (currentView === 'contacts') {
    return (
      <Suspense fallback={<PageLoadingFallback />}>
        <ContactsPage />
      </Suspense>
    )
  }

  return (
    <Suspense fallback={<PageLoadingFallback />}>
      <DashboardPage />
    </Suspense>
  )
}

function App() {
  const [currentView, setCurrentView] = useState<ViewType>('organization')

  return (
    <ThemeProvider theme={theme} defaultMode="system" noSsr>
      <CssBaseline />
      <LocalizationProvider dateAdapter={AdapterDayjs} adapterLocale="pt-br">
        <CompanyProvider>
          <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', display: 'flex', flexDirection: 'column' }}>
            <AppBar currentView={currentView} onViewChange={setCurrentView} />
            <Box component="main" sx={{ flexGrow: 1 }}>
              <MainContent currentView={currentView} />
            </Box>
            <Footer />
          </Box>
        </CompanyProvider>
      </LocalizationProvider>
    </ThemeProvider>
  )
}

export default App
