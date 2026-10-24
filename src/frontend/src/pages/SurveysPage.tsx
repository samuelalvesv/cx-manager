import AddIcon from '@mui/icons-material/Add'
import PollOutlinedIcon from '@mui/icons-material/PollOutlined'
import RefreshIcon from '@mui/icons-material/Refresh'
import {
  Alert,
  Box,
  Button,
  Card,
  Chip,
  Container,
  Grid,
  Skeleton,
  Stack,
} from '@mui/material'
import { useEffect, useState } from 'react'
import { DeleteSurveyDialog } from '../components/DeleteSurveyDialog.tsx'
import { EmptyState } from '../components/EmptyState.tsx'
import { PageHeader } from '../components/PageHeader.tsx'
import { SurveyDialog } from '../components/SurveyDialog.tsx'
import { SurveyCard } from '../components/surveys/SurveyCard.tsx'
import { useCompany } from '../context/useCompany.ts'
import { getSurveys } from '../services/surveyService.ts'
import type { Survey } from '../types/survey.ts'

export function SurveysPage() {
  const { selectedCompany, selectedCompanyId } = useCompany()
  const [surveys, setSurveys] = useState<readonly Survey[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

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
      <Container maxWidth="lg" sx={{ py: { xs: 2.5, sm: 3, md: 4 } }}>
        <Alert severity="info">
          Selecione uma empresa no cabeçalho superior para gerenciar suas pesquisas de satisfação.
        </Alert>
      </Container>
    )
  }

  if (loading) {
    return (
      <Container maxWidth="lg" sx={{ py: { xs: 2.5, sm: 3, md: 4 } }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 2, mb: 3 }}>
          <Box sx={{ width: { xs: '100%', sm: 'auto' } }}>
            <Skeleton variant="text" width={220} height={40} />
            <Skeleton variant="text" sx={{ width: { xs: '100%', sm: 320 } }} height={24} />
          </Box>
          <Skeleton variant="rounded" sx={{ width: { xs: '100%', sm: 160 } }} height={40} />
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
      <Container maxWidth="lg" sx={{ py: { xs: 2.5, sm: 3, md: 4 } }}>
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
    <Container maxWidth="lg" sx={{ py: { xs: 2.5, sm: 3, md: 4 } }}>
      <PageHeader
        title="Gestão de Pesquisas"
        icon={<PollOutlinedIcon sx={{ color: 'primary.main', fontSize: 28 }} />}
        subtitle={
          <>
            Pesquisas de satisfação da organização <strong>{selectedCompany?.name ?? ''}</strong> para acompanhamento de lealdade e relacionamento.
          </>
        }
        badges={
          surveys.length > 0 ? (
            <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap' }} useFlexGap>
              <Chip label={`${String(surveys.length)} ativas`} size="small" variant="outlined" />
              <Chip label={`${String(npsCount)} Recomendação (NPS)`} size="small" color="primary" variant="outlined" />
              <Chip label={`${String(csatCount)} Satisfação (CSAT)`} size="small" color="secondary" variant="outlined" />
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

      {surveys.length === 0 ? (
        <Card sx={{ border: 1, borderColor: 'divider', boxShadow: 'none' }}>
          <EmptyState
            icon={<PollOutlinedIcon sx={{ fontSize: 48 }} />}
            title="Nenhuma pesquisa cadastrada"
            description="Esta organização ainda não possui pesquisas de satisfação ativas. Cadastre sua primeira pesquisa de recomendação (NPS) ou satisfação (CSAT) para iniciar a coleta de avaliações."
            action={{
              label: 'Cadastrar Primeira Pesquisa',
              icon: <AddIcon />,
              onClick: handleOpenCreate,
            }}
          />
        </Card>
      ) : (
        <Grid container spacing={3}>
          {surveys.map((survey) => (
            <Grid key={survey.id} size={{ xs: 12, md: 6, lg: 4 }}>
              <SurveyCard
                survey={survey}
                onEdit={handleOpenEdit}
                onDelete={handleOpenDelete}
              />
            </Grid>
          ))}
        </Grid>
      )}

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
