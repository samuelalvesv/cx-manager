import ClearIcon from '@mui/icons-material/Clear'
import PollOutlinedIcon from '@mui/icons-material/PollOutlined'
import RefreshIcon from '@mui/icons-material/Refresh'
import {
  Alert,
  Box,
  Button,
  Container,
  Grid,
  Paper,
  Skeleton,
} from '@mui/material'
import { useEffect, useState } from 'react'
import { AnalyticsFilterBar } from '../components/dashboard/AnalyticsFilterBar.tsx'
import { AnalyticsKpiCards } from '../components/dashboard/AnalyticsKpiCards.tsx'
import { MonthlyNpsCard } from '../components/dashboard/MonthlyNpsCard.tsx'
import { NpsDistributionCard } from '../components/dashboard/NpsDistributionCard.tsx'
import { OrganizationSummaryCard } from '../components/dashboard/OrganizationSummaryCard.tsx'
import { SegmentNpsCard } from '../components/dashboard/SegmentNpsCard.tsx'
import { EmptyState } from '../components/EmptyState.tsx'
import { useCompany } from '../context/useCompany.ts'
import {
  getAnalyticsSummary,
  getNpsByMonth,
  getNpsBySegment,
} from '../services/analyticsService.ts'
import { downloadBlob, exportResponsesCsv } from '../services/feedbackService.ts'
import { getSurveys } from '../services/surveyService.ts'
import type {
  AnalyticsFilter,
  AnalyticsSummary,
  MonthlyNps,
  SegmentNps,
} from '../types/analytics.ts'
import type { Survey } from '../types/survey.ts'

export function DashboardPage() {
  const { selectedCompany, selectedCompanyId, loading: companyLoading, error: companyError, refreshCompanies } = useCompany()

  const [fromDate, setFromDate] = useState('')
  const [toDate, setToDate] = useState('')
  const [selectedSurveyId, setSelectedSurveyId] = useState<number>(0)
  const [surveys, setSurveys] = useState<readonly Survey[]>([])

  const [summary, setSummary] = useState<AnalyticsSummary | null>(null)
  const [monthlyNps, setMonthlyNps] = useState<readonly MonthlyNps[]>([])
  const [segmentNps, setSegmentNps] = useState<readonly SegmentNps[]>([])
  const [analyticsLoading, setAnalyticsLoading] = useState(true)
  const [analyticsError, setAnalyticsError] = useState<string | null>(null)
  const [retryTrigger, setRetryTrigger] = useState(0)
  const [exportLoading, setExportLoading] = useState(false)
  const [exportError, setExportError] = useState<string | null>(null)

  useEffect(() => {
    let active = true
    const controller = new AbortController()

    async function fetchSurveys() {
      if (selectedCompanyId === null) {
        if (active) {
          setSurveys([])
        }
        return
      }

      try {
        const data = await getSurveys(controller.signal)
        if (active) {
          setSurveys(data)
        }
      } catch (err: unknown) {
        if (!active || (err instanceof Error && err.name === 'AbortError')) return
      }
    }

    void fetchSurveys()

    return () => {
      active = false
      controller.abort()
    }
  }, [selectedCompanyId])

  useEffect(() => {
    let active = true
    const controller = new AbortController()
    const currentRetry = retryTrigger

    async function fetchAnalytics() {
      if (currentRetry < 0) return
      if (selectedCompanyId === null) {
        if (active) {
          setSummary(null)
          setMonthlyNps([])
          setSegmentNps([])
          setAnalyticsLoading(false)
        }
        return
      }

      setAnalyticsLoading(true)
      setAnalyticsError(null)

      const filter: AnalyticsFilter = {
        from: fromDate.length > 0 ? fromDate : undefined,
        to: toDate.length > 0 ? toDate : undefined,
        surveyId: selectedSurveyId > 0 ? selectedSurveyId : undefined,
      }

      try {
        const [summaryData, monthlyData, segmentData] = await Promise.all([
          getAnalyticsSummary(filter, controller.signal),
          getNpsByMonth(filter, controller.signal),
          getNpsBySegment(filter, controller.signal),
        ])

        if (!active) return

        setSummary(summaryData)
        setMonthlyNps(monthlyData)
        setSegmentNps(segmentData)
        setAnalyticsLoading(false)
      } catch (err: unknown) {
        if (!active || (err instanceof Error && err.name === 'AbortError')) return
        const message = err instanceof Error ? err.message : 'Falha ao carregar indicadores analíticos.'
        setAnalyticsError(message)
        setAnalyticsLoading(false)
      }
    }

    void fetchAnalytics()

    return () => {
      active = false
      controller.abort()
    }
  }, [selectedCompanyId, fromDate, toDate, selectedSurveyId, retryTrigger])

  const handleClearFilters = () => {
    setFromDate('')
    setToDate('')
    setSelectedSurveyId(0)
  }

  const handleRetry = () => {
    setRetryTrigger((prev) => prev + 1)
  }

  const handleExportCsv = async () => {
    if (exportLoading || selectedCompanyId === null) return
    setExportLoading(true)
    setExportError(null)

    try {
      const blob = await exportResponsesCsv({
        from: fromDate.length > 0 ? fromDate : undefined,
        to: toDate.length > 0 ? toDate : undefined,
        surveyId: selectedSurveyId > 0 ? selectedSurveyId : undefined,
      })
      downloadBlob(blob, 'respostas.csv')
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Falha ao exportar respostas em CSV.'
      setExportError(message)
    } finally {
      setExportLoading(false)
    }
  }

  if (companyLoading) {
    return (
      <Container maxWidth="lg" sx={{ py: { xs: 2.5, sm: 3, md: 4 } }}>
        <Skeleton variant="rounded" height={140} sx={{ mb: 3 }} />
        <Grid container spacing={3} sx={{ mb: 3 }}>
          <Grid size={{ xs: 12, md: 4 }}>
            <Skeleton variant="rounded" height={160} />
          </Grid>
          <Grid size={{ xs: 12, md: 4 }}>
            <Skeleton variant="rounded" height={160} />
          </Grid>
          <Grid size={{ xs: 12, md: 4 }}>
            <Skeleton variant="rounded" height={160} />
          </Grid>
        </Grid>
        <Skeleton variant="rounded" height={120} sx={{ mb: 3 }} />
        <Skeleton variant="rounded" height={260} />
      </Container>
    )
  }

  if (companyError !== null) {
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
                void refreshCompanies()
              }}
            >
              Tentar novamente
            </Button>
          }
          sx={{ mb: 3 }}
        >
          {companyError}
        </Alert>
      </Container>
    )
  }

  if (selectedCompanyId === null) {
    return (
      <Container maxWidth="lg" sx={{ py: { xs: 2.5, sm: 3, md: 4 } }}>
        <Alert severity="info">
          Selecione uma empresa no cabeçalho superior para visualizar os indicadores de satisfação.
        </Alert>
      </Container>
    )
  }

  const hasActiveFilters = fromDate.length > 0 || toDate.length > 0 || selectedSurveyId > 0

  return (
    <Container maxWidth="lg" sx={{ py: { xs: 2.5, sm: 3, md: 4 } }}>
      <OrganizationSummaryCard company={selectedCompany} />

      <AnalyticsFilterBar
        fromDate={fromDate}
        toDate={toDate}
        selectedSurveyId={selectedSurveyId}
        surveys={surveys}
        exportLoading={exportLoading}
        exportError={exportError}
        onFromDateChange={setFromDate}
        onToDateChange={setToDate}
        onSurveyChange={setSelectedSurveyId}
        onClearFilters={handleClearFilters}
        onExportCsv={() => {
          void handleExportCsv()
        }}
        onDismissExportError={() => setExportError(null)}
      />

      {analyticsError !== null && (
        <Alert
          severity="error"
          action={
            <Button
              color="inherit"
              size="small"
              startIcon={<RefreshIcon />}
              onClick={handleRetry}
            >
              Tentar novamente
            </Button>
          }
          sx={{ mb: 3 }}
        >
          {analyticsError}
        </Alert>
      )}

      {analyticsLoading && (
        <Box sx={{ mb: 3 }}>
          <Grid container spacing={3} sx={{ mb: 3 }}>
            <Grid size={{ xs: 12, md: 4 }}>
              <Skeleton variant="rounded" height={150} />
            </Grid>
            <Grid size={{ xs: 12, md: 4 }}>
              <Skeleton variant="rounded" height={150} />
            </Grid>
            <Grid size={{ xs: 12, md: 4 }}>
              <Skeleton variant="rounded" height={150} />
            </Grid>
          </Grid>
          <Skeleton variant="rounded" height={140} sx={{ mb: 3 }} />
          <Skeleton variant="rounded" height={280} />
        </Box>
      )}

      {!analyticsLoading && summary !== null && (
        <>
          {summary.responsesCount === 0 ? (
            <Paper sx={{ p: { xs: 2.5, sm: 4 }, border: 1, borderColor: 'divider', boxShadow: 'none' }}>
              <EmptyState
                icon={<PollOutlinedIcon sx={{ fontSize: 48 }} />}
                title="Nenhuma avaliação encontrada no período"
                description="Não há avaliações registradas para o intervalo de datas ou pesquisa selecionada. Ajuste os filtros acima para visualizar os indicadores."
                action={
                  hasActiveFilters
                    ? {
                        label: 'Limpar Filtros',
                        icon: <ClearIcon />,
                        onClick: handleClearFilters,
                      }
                    : undefined
                }
              />
            </Paper>
          ) : (
            <>
              <AnalyticsKpiCards summary={summary} />
              <NpsDistributionCard summary={summary} />
              <Grid container spacing={3}>
                <Grid size={{ xs: 12, md: 7 }}>
                  <MonthlyNpsCard monthlyNps={monthlyNps} />
                </Grid>
                <Grid size={{ xs: 12, md: 5 }}>
                  <SegmentNpsCard segmentNps={segmentNps} />
                </Grid>
              </Grid>
            </>
          )}
        </>
      )}
    </Container>
  )
}
