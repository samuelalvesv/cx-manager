import AddIcon from '@mui/icons-material/Add'
import DeleteOutlinedIcon from '@mui/icons-material/DeleteOutlined'
import EditOutlinedIcon from '@mui/icons-material/EditOutlined'
import PollOutlinedIcon from '@mui/icons-material/PollOutlined'
import RefreshIcon from '@mui/icons-material/Refresh'
import StarBorderOutlinedIcon from '@mui/icons-material/StarBorderOutlined'
import TrendingUpIcon from '@mui/icons-material/TrendingUp'
import {
  Alert,
  Box,
  Button,
  Card,
  CardActions,
  CardContent,
  Chip,
  Container,
  Grid,
  Skeleton,
  Stack,
  Tooltip,
  Typography,
} from '@mui/material'
import { useEffect, useState } from 'react'
import { DeleteSurveyDialog } from '../components/DeleteSurveyDialog.tsx'
import { EmptyState } from '../components/EmptyState.tsx'
import { PageHeader } from '../components/PageHeader.tsx'
import { SurveyDialog } from '../components/SurveyDialog.tsx'
import { useCompany } from '../context/useCompany.ts'
import { getSurveys } from '../services/surveyService.ts'
import type { Survey } from '../types/survey.ts'

export function SurveysPage() {
  const { selectedCompany, selectedCompanyId } = useCompany()
  const [surveys, setSurveys] = useState<readonly Survey[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // Modals state
  const [dialogOpen, setDialogOpen] = useState(false)
  const [dialogMode, setDialogMode] = useState<'create' | 'edit'>('create')
  const [selectedSurvey, setSelectedSurvey] = useState<Survey | null>(null)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [surveyToDelete, setSurveyToDelete] = useState<Survey | null>(null)

  useEffect(() => {
    let active = true
    const controller = new AbortController()

    async function fetchSurveys() {
      if (selectedCompanyId === null) {
        setLoading(false)
        return
      }
      try {
        const data = await getSurveys(controller.signal)
        if (!active) {
          return
        }
        setSurveys(data)
        setError(null)
        setLoading(false)
      } catch (err: unknown) {
        if (!active || (err instanceof Error && err.name === 'AbortError')) {
          return
        }
        const message = err instanceof Error ? err.message : 'Falha ao carregar as pesquisas.'
        setError(message)
        setLoading(false)
      }
    }

    void fetchSurveys()

    return () => {
      active = false
      controller.abort()
    }
  }, [selectedCompanyId])

  const handleRetry = async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await getSurveys()
      setSurveys(data)
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Falha ao carregar as pesquisas.'
      setError(message)
    } finally {
      setLoading(false)
    }
  }

  const handleOpenCreate = () => {
    setSelectedSurvey(null)
    setDialogMode('create')
    setDialogOpen(true)
  }

  const handleOpenEdit = (survey: Survey) => {
    setSelectedSurvey(survey)
    setDialogMode('edit')
    setDialogOpen(true)
  }

  const handleOpenDelete = (survey: Survey) => {
    setSurveyToDelete(survey)
    setDeleteDialogOpen(true)
  }

  const handleDialogSuccess = (saved: Survey) => {
    setSurveys((prev) => {
      const index = prev.findIndex((s) => s.id === saved.id)
      if (index >= 0) {
        const updated = [...prev]
        updated[index] = saved
        return updated
      }
      return [...prev, saved]
    })
  }

  const handleDeleteSuccess = (deletedId: number) => {
    setSurveys((prev) => prev.filter((s) => s.id !== deletedId))
  }

  if (selectedCompanyId === null) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Alert severity="info">
          Selecione uma empresa no cabeçalho superior para gerenciar seus questionários de pesquisa.
        </Alert>
      </Container>
    )
  }

  if (loading) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Box>
            <Skeleton variant="text" width={220} height={40} />
            <Skeleton variant="text" width={320} height={24} />
          </Box>
          <Skeleton variant="rounded" width={160} height={40} />
        </Box>
        <Grid container spacing={3}>
          {[1, 2, 3].map((key) => (
            <Grid key={key} size={{ xs: 12, md: 6, lg: 4 }}>
              <Skeleton variant="rounded" height={180} />
            </Grid>
          ))}
        </Grid>
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
                void handleRetry()
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

  const npsCount = surveys.filter((s) => s.type === 'NPS').length
  const csatCount = surveys.filter((s) => s.type === 'CSAT').length

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      {/* Top Header */}
      <PageHeader
        title="Gestão de Pesquisas"
        subtitle={
          <>
            Questionários de satisfação da organização <strong>{selectedCompany?.name ?? ''}</strong> para monitoramento de métricas e relacionamento.
          </>
        }
        badges={
          surveys.length > 0 ? (
            <Stack direction="row" spacing={1}>
              <Chip label={`${surveys.length} total`} size="small" variant="outlined" />
              <Chip label={`${npsCount} NPS`} size="small" color="primary" variant="outlined" />
              <Chip label={`${csatCount} CSAT`} size="small" color="secondary" variant="outlined" />
            </Stack>
          ) : undefined
        }
        action={
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={handleOpenCreate}
            sx={{ fontWeight: 600 }}
          >
            Nova Pesquisa
          </Button>
        }
      />

      {/* Empty State */}
      {surveys.length === 0 ? (
        <Card sx={{ border: 1, borderColor: 'divider', bgcolor: 'transparent', boxShadow: 'none' }}>
          <EmptyState
            icon={<PollOutlinedIcon sx={{ fontSize: 48 }} />}
            title="Nenhuma pesquisa cadastrada"
            description="Esta organização ainda não possui questionários de satisfação ativos. Cadastre sua primeira pesquisa NPS ou CSAT para iniciar a coleta e mensuração."
            action={{
              label: 'Cadastrar Primeira Pesquisa',
              icon: <AddIcon />,
              onClick: handleOpenCreate,
            }}
          />
        </Card>
      ) : (
        /* Surveys Grid */
        <Grid container spacing={3}>
          {surveys.map((survey) => {
            const isNps = survey.type === 'NPS'
            const formattedDate = new Date(survey.createdAt).toLocaleDateString('pt-BR')

            return (
              <Grid key={survey.id} size={{ xs: 12, md: 6, lg: 4 }}>
                <Card sx={{ height: '100%', display: 'flex', flexDirection: 'column', border: 1, borderColor: 'divider', boxShadow: 'none' }}>
                  <CardContent sx={{ flexGrow: 1, p: 2.5 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', gap: 1, mb: 1.5 }}>
                      <Chip
                        icon={isNps ? <TrendingUpIcon fontSize="small" /> : <StarBorderOutlinedIcon fontSize="small" />}
                        label={isNps ? 'NPS (Escala 0–10)' : 'CSAT (Escala 1–5)'}
                        color={isNps ? 'primary' : 'secondary'}
                        size="small"
                        sx={{ fontWeight: 600 }}
                      />
                      <Typography variant="caption" color="text.secondary">
                        ID: {survey.id}
                      </Typography>
                    </Box>

                    <Typography variant="h6" component="h2" sx={{ fontWeight: 600, mb: 1, fontSize: '1.05rem', lineHeight: 1.3 }}>
                      {survey.name}
                    </Typography>

                    <Typography variant="caption" color="text.secondary" sx={{ display: 'block' }}>
                      Criada em: {formattedDate}
                    </Typography>
                  </CardContent>

                  <CardActions sx={{ px: 2, py: 1.5, borderTop: '1px solid #f1f5f9', justifyContent: 'flex-end', gap: 1 }}>
                    <Tooltip title="Editar nome da pesquisa">
                      <Button
                        size="small"
                        startIcon={<EditOutlinedIcon />}
                        onClick={() => handleOpenEdit(survey)}
                      >
                        Editar
                      </Button>
                    </Tooltip>
                    <Tooltip title="Desativar pesquisa (soft delete)">
                      <Button
                        size="small"
                        color="error"
                        startIcon={<DeleteOutlinedIcon />}
                        onClick={() => handleOpenDelete(survey)}
                      >
                        Desativar
                      </Button>
                    </Tooltip>
                  </CardActions>
                </Card>
              </Grid>
            )
          })}
        </Grid>
      )}

      {/* Dialogs */}
      <SurveyDialog
        open={dialogOpen}
        mode={dialogMode}
        survey={selectedSurvey}
        onClose={() => setDialogOpen(false)}
        onSuccess={handleDialogSuccess}
      />

      <DeleteSurveyDialog
        open={deleteDialogOpen}
        survey={surveyToDelete}
        onClose={() => setDeleteDialogOpen(false)}
        onSuccess={handleDeleteSuccess}
      />
    </Container>
  )
}
