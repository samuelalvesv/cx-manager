import {
  Box,
  Container,
  CssBaseline,
  Skeleton,
  ThemeProvider,
} from '@mui/material'
import { lazy, Suspense, useState } from 'react'
import { AppBar, type ViewType } from './components/AppBar.tsx'
import { CompanyProvider } from './context/CompanyContext.tsx'
import { theme } from './theme/theme.ts'

const ContactsPage = lazy(() => import('./pages/ContactsPage.tsx').then((m) => ({ default: m.ContactsPage })))
const SurveysPage = lazy(() => import('./pages/SurveysPage.tsx').then((m) => ({ default: m.SurveysPage })))
const DashboardPage = lazy(() => import('./pages/DashboardPage.tsx').then((m) => ({ default: m.DashboardPage })))

function PageLoadingFallback() {
  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
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
  const [currentView, setCurrentView] = useState<ViewType>('surveys')

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <CompanyProvider>
        <Box sx={{ minHeight: '100vh', bgcolor: 'background.default' }}>
          <AppBar currentView={currentView} onViewChange={setCurrentView} />
          <MainContent currentView={currentView} />
        </Box>
      </CompanyProvider>
    </ThemeProvider>
  )
}

export default App
