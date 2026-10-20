import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import EditOutlinedIcon from '@mui/icons-material/EditOutlined'
import RefreshIcon from '@mui/icons-material/Refresh'
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Container,
  CssBaseline,
  Divider,
  Skeleton,
  ThemeProvider,
  Typography,
} from '@mui/material'
import { useState } from 'react'
import { AppBar, type ViewType } from './components/AppBar.tsx'
import { CompanyDialog } from './components/CompanyDialog.tsx'
import { CompanyProvider } from './context/CompanyContext.tsx'
import { useCompany } from './context/useCompany.ts'
import { SurveysPage } from './pages/SurveysPage.tsx'
import { theme } from './theme/theme.ts'

function DashboardContent() {
  const { selectedCompany, loading, error, refreshCompanies } = useCompany()
  const [editDialogOpen, setEditDialogOpen] = useState(false)

  if (loading) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Skeleton variant="rounded" height={140} sx={{ mb: 3 }} />
        <Box sx={{ display: 'flex', gap: 3 }}>
          <Skeleton variant="rounded" width="50%" height={200} />
          <Skeleton variant="rounded" width="50%" height={200} />
        </Box>
      </Container>
    )
  }

  if (error !== null) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Alert
          severity="error"
          action={
            <Button
              color="inherit"
              size="small"
              startIcon={<RefreshIcon />}
              onClick={() => {
                void refreshCompanies()
              }}
            >
              Tentar novamente
            </Button>
          }
          sx={{ mb: 3 }}
        >
          {error}
        </Alert>
      </Container>
    )
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Card sx={{ mb: 3, border: '1px solid #e2e8f0', boxShadow: 'none' }}>
        <CardContent sx={{ p: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
            <Typography variant="overline" color="primary.main" sx={{ fontWeight: 700, letterSpacing: 1.2 }}>
              Organização Ativa
            </Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
              <CheckCircleIcon color="success" fontSize="small" />
              <Typography variant="caption" color="text.secondary">
                Contexto multi-empresa ativo
              </Typography>
            </Box>
          </Box>
          <Typography variant="h4" component="h1" sx={{ fontWeight: 700, mb: 1 }}>
            {selectedCompany?.name ?? 'Nenhuma empresa selecionada'}
          </Typography>
          <Typography variant="body1" color="text.secondary" sx={{ mb: 2 }}>
            {selectedCompany?.description ?? 'Selecione uma empresa no cabeçalho superior.'}
          </Typography>
          {selectedCompany !== null && (
            <Button
              variant="outlined"
              size="small"
              startIcon={<EditOutlinedIcon />}
              onClick={() => setEditDialogOpen(true)}
            >
              Editar Dados da Empresa
            </Button>
          )}
        </CardContent>
        <Divider />
        <CardContent sx={{ bgcolor: 'background.default', py: 2 }}>
          <Typography variant="body2" color="text.secondary">
            ID de Contexto: <strong>{selectedCompany?.id ?? '—'}</strong> • Todas as operações de contatos, pesquisas e
            respostas são isoladas para este identificador.
          </Typography>
        </CardContent>
      </Card>

      <CompanyDialog
        open={editDialogOpen}
        mode="edit"
        company={selectedCompany}
        onClose={() => setEditDialogOpen(false)}
        onSuccess={(saved) => {
          void refreshCompanies(saved.id)
        }}
      />
    </Container>
  )
}

function MainContent({ currentView }: { readonly currentView: ViewType }) {
  if (currentView === 'surveys') {
    return <SurveysPage />
  }

  return <DashboardContent />
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
