import CalendarMonthIcon from '@mui/icons-material/CalendarMonth'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import ClearIcon from '@mui/icons-material/Clear'
import EditOutlinedIcon from '@mui/icons-material/EditOutlined'
import FilterListIcon from '@mui/icons-material/FilterList'
import InsightsIcon from '@mui/icons-material/Insights'
import PollOutlinedIcon from '@mui/icons-material/PollOutlined'
import RateReviewOutlinedIcon from '@mui/icons-material/RateReviewOutlined'
import RefreshIcon from '@mui/icons-material/Refresh'
import StarBorderOutlinedIcon from '@mui/icons-material/StarBorderOutlined'
import TrendingUpIcon from '@mui/icons-material/TrendingUp'
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Container,
  Divider,
  FormControl,
  Grid,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Skeleton,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material'
import type { SelectChangeEvent } from '@mui/material/Select'
import { useEffect, useState } from 'react'
import { CompanyDialog } from '../components/CompanyDialog.tsx'
import { EmptyState } from '../components/EmptyState.tsx'
import { useCompany } from '../context/useCompany.ts'
import {
  getAnalyticsSummary,
  getNpsByMonth,
  getNpsBySegment,
} from '../services/analyticsService.ts'
import { getSurveys } from '../services/surveyService.ts'
import type {
  AnalyticsFilter,
  AnalyticsSummary,
  MonthlyNps,
  SegmentNps,
} from '../types/analytics.ts'
import type { Survey } from '../types/survey.ts'

function getNpsScoreColor(score: number): 'success' | 'warning' | 'error' | 'info' {
  if (score >= 50) return 'success'
  if (score >= 0) return 'info'
  return 'error'
}

function getNpsZoneLabel(score: number): string {
  if (score >= 75) return 'Zona de Excelência'
  if (score >= 50) return 'Zona de Qualidade'
  if (score >= 0) return 'Zona de Aperfeiçoamento'
  return 'Zona Crítica'
}

export function DashboardPage() {
  const { selectedCompany, selectedCompanyId, loading: companyLoading, error: companyError, refreshCompanies } = useCompany()
  const [editDialogOpen, setEditDialogOpen] = useState(false)

  // Filtros
  const [fromDate, setFromDate] = useState('')
  const [toDate, setToDate] = useState('')
  const [selectedSurveyId, setSelectedSurveyId] = useState<number>(0)
  const [surveys, setSurveys] = useState<readonly Survey[]>([])
  // Dados analíticos
  const [summary, setSummary] = useState<AnalyticsSummary | null>(null)
  const [monthlyNps, setMonthlyNps] = useState<readonly MonthlyNps[]>([])
  const [segmentNps, setSegmentNps] = useState<readonly SegmentNps[]>([])
  const [analyticsLoading, setAnalyticsLoading] = useState(true)
  const [analyticsError, setAnalyticsError] = useState<string | null>(null)
  const [retryTrigger, setRetryTrigger] = useState(0)

  // Carrega lista de pesquisas ativas para o filtro
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

  // Carrega indicadores analíticos
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

  const handleSurveyChange = (event: SelectChangeEvent<number>) => {
    const value = event.target.value
    setSelectedSurveyId(typeof value === 'number' ? value : Number(value))
  }

  const handleClearFilters = () => {
    setFromDate('')
    setToDate('')
    setSelectedSurveyId(0)
  }

  const handleRetry = () => {
    setRetryTrigger((prev) => prev + 1)
  }

  if (companyLoading) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
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
          {companyError}
        </Alert>
      </Container>
    )
  }

  if (selectedCompanyId === null) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Alert severity="info">
          Selecione uma empresa no cabeçalho superior para visualizar os indicadores de satisfação.
        </Alert>
      </Container>
    )
  }

  const hasActiveFilters = fromDate.length > 0 || toDate.length > 0 || selectedSurveyId > 0

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      {/* Card da Organização Ativa */}
      <Card sx={{ mb: 3, border: 1, borderColor: 'divider', boxShadow: 'none' }}>
        <CardContent sx={{ p: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
            <Typography variant="overline" color="primary.main" sx={{ fontWeight: 700, letterSpacing: 1.2 }}>
              Painel de Satisfação
            </Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
              <CheckCircleIcon color="success" fontSize="small" />
              <Typography variant="caption" color="text.secondary">
                {selectedCompany?.name ?? 'Organização conectada'}
              </Typography>
            </Box>
          </Box>
          <Typography variant="h4" component="h1" sx={{ fontWeight: 700, mb: 1 }}>
            Indicadores de Customer Experience
          </Typography>
          <Typography variant="body1" color="text.secondary" sx={{ mb: 2 }}>
            Acompanhe métricas consolidadas de NPS e CSAT em tempo real com filtros de período e pesquisa.
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
            Identificador da Empresa: <strong>{selectedCompany?.id ?? '—'}</strong> • Resumo agregado e séries temporais com invalidação reativa de cache.
          </Typography>
        </CardContent>
      </Card>

      {/* Barra de Filtros */}
      <Paper sx={{ p: 2.5, mb: 3, border: 1, borderColor: 'divider', boxShadow: 'none' }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
          <FilterListIcon color="primary" fontSize="small" />
          <Typography variant="subtitle2" sx={{ fontWeight: 700 }}>
            Filtros do Painel Analítico
          </Typography>
        </Box>
        <Grid container spacing={2} sx={{ alignItems: 'center' }}>
          <Grid size={{ xs: 12, sm: 4, md: 3 }}>
            <TextField
              label="Data Inicial"
              type="date"
              size="small"
              fullWidth
              value={fromDate}
              onChange={(e) => setFromDate(e.target.value)}
              slotProps={{ inputLabel: { shrink: true } }}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 4, md: 3 }}>
            <TextField
              label="Data Final"
              type="date"
              size="small"
              fullWidth
              value={toDate}
              onChange={(e) => setToDate(e.target.value)}
              slotProps={{ inputLabel: { shrink: true } }}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 4, md: 4 }}>
            <FormControl size="small" fullWidth>
              <InputLabel id="survey-filter-label">Pesquisa</InputLabel>
              <Select
                labelId="survey-filter-label"
                value={selectedSurveyId}
                label="Pesquisa"
                onChange={handleSurveyChange}
              >
                <MenuItem value={0}>Todas as pesquisas</MenuItem>
                {surveys.map((survey) => (
                  <MenuItem key={survey.id} value={survey.id}>
                    {survey.name} ({survey.type})
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          <Grid size={{ xs: 12, sm: 12, md: 2 }}>
            <Button
              variant="outlined"
              size="medium"
              fullWidth
              disabled={!hasActiveFilters}
              startIcon={<ClearIcon />}
              onClick={handleClearFilters}
            >
              Limpar
            </Button>
          </Grid>
        </Grid>
      </Paper>

      {/* Erro de Analytics */}
      {analyticsError !== null && (
        <Alert
          severity="error"
          action={
            <Button
              color="inherit"
              size="small"
              startIcon={<RefreshIcon />}
              onClick={() => {
                handleRetry()
              }}
            >
              Tentar novamente
            </Button>
          }
          sx={{ mb: 3 }}
        >
          {analyticsError}
        </Alert>
      )}

      {/* Skeletons de Carregamento */}
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

      {/* Conteúdo Analítico */}
      {!analyticsLoading && summary !== null && (
        <>
          {summary.responsesCount === 0 ? (
            <Paper sx={{ p: 4, border: 1, borderColor: 'divider', boxShadow: 'none' }}>
              <EmptyState
                icon={<PollOutlinedIcon sx={{ fontSize: 48 }} />}
                title="Nenhuma resposta encontrada para o período"
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
              {/* Cards de Métricas Principais (KPIs) */}
              <Grid container spacing={3} sx={{ mb: 3 }}>
                {/* Card NPS Score */}
                <Grid size={{ xs: 12, md: 4 }}>
                  <Card sx={{ border: 1, borderColor: 'divider', boxShadow: 'none', height: '100%' }}>
                    <CardContent sx={{ p: 3 }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
                        <Typography variant="overline" color="text.secondary" sx={{ fontWeight: 700 }}>
                          Score NPS Geral
                        </Typography>
                        <TrendingUpIcon color={getNpsScoreColor(summary.npsScore)} fontSize="small" />
                      </Box>
                      <Box sx={{ display: 'flex', alignItems: 'baseline', gap: 1.5, mb: 1 }}>
                        <Typography variant="h3" component="div" sx={{ fontWeight: 800 }}>
                          {summary.npsResponses > 0
                            ? summary.npsScore > 0
                              ? `+${summary.npsScore}`
                              : summary.npsScore
                            : '—'}
                        </Typography>
                        {summary.npsResponses > 0 && (
                          <Chip
                            label={getNpsZoneLabel(summary.npsScore)}
                            color={getNpsScoreColor(summary.npsScore)}
                            size="small"
                            sx={{ fontWeight: 600 }}
                          />
                        )}
                      </Box>
                      <Typography variant="caption" color="text.secondary">
                        Baseado em <strong>{summary.npsResponses}</strong> respostas de pesquisas NPS.
                      </Typography>
                    </CardContent>
                  </Card>
                </Grid>

                {/* Card CSAT */}
                <Grid size={{ xs: 12, md: 4 }}>
                  <Card sx={{ border: 1, borderColor: 'divider', boxShadow: 'none', height: '100%' }}>
                    <CardContent sx={{ p: 3 }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
                        <Typography variant="overline" color="text.secondary" sx={{ fontWeight: 700 }}>
                          Média CSAT
                        </Typography>
                        <StarBorderOutlinedIcon color="primary" fontSize="small" />
                      </Box>
                      <Box sx={{ display: 'flex', alignItems: 'baseline', gap: 1, mb: 1 }}>
                        <Typography variant="h3" component="div" sx={{ fontWeight: 800 }}>
                          {summary.csatAvg !== null ? summary.csatAvg.toFixed(2) : '—'}
                        </Typography>
                        <Typography variant="subtitle1" color="text.secondary" sx={{ fontWeight: 600 }}>
                          / 5.0
                        </Typography>
                      </Box>
                      <Typography variant="caption" color="text.secondary">
                        {summary.csatAvg !== null
                          ? 'Média aritmética sobre pesquisas de satisfação CSAT.'
                          : 'Nenhuma resposta CSAT no recorte filtrado.'}
                      </Typography>
                    </CardContent>
                  </Card>
                </Grid>

                {/* Card Total de Respostas */}
                <Grid size={{ xs: 12, md: 4 }}>
                  <Card sx={{ border: 1, borderColor: 'divider', boxShadow: 'none', height: '100%' }}>
                    <CardContent sx={{ p: 3 }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
                        <Typography variant="overline" color="text.secondary" sx={{ fontWeight: 700 }}>
                          Total de Respostas
                        </Typography>
                        <RateReviewOutlinedIcon color="secondary" fontSize="small" />
                      </Box>
                      <Box sx={{ display: 'flex', alignItems: 'baseline', gap: 1, mb: 1 }}>
                        <Typography variant="h3" component="div" sx={{ fontWeight: 800 }}>
                          {summary.responsesCount}
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          respostas ativas
                        </Typography>
                      </Box>
                      <Typography variant="caption" color="text.secondary">
                        Volume total de avaliações coletadas (NPS e CSAT).
                      </Typography>
                    </CardContent>
                  </Card>
                </Grid>
              </Grid>

              {/* Barra de Distribuição de Satisfação NPS */}
              {summary.npsResponses > 0 && (
                <Card sx={{ mb: 3, border: 1, borderColor: 'divider', boxShadow: 'none' }}>
                  <CardContent sx={{ p: 3 }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
                      <Typography variant="subtitle1" sx={{ fontWeight: 700 }}>
                        Distribuição das Notas NPS
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {summary.npsResponses} respostas NPS válidas
                      </Typography>
                    </Box>

                    {/* Barra de Proporção Empilhada */}
                    <Box
                      aria-hidden="true"
                      sx={{
                        display: 'flex',
                        height: 20,
                        borderRadius: 1.5,
                        overflow: 'hidden',
                        mb: 2.5,
                        bgcolor: 'action.hover',
                      }}
                    >
                      <Box
                        sx={{
                          width: `${summary.promoters.pct}%`,
                          bgcolor: 'success.main',
                          transition: 'width 0.4s ease',
                        }}
                      />
                      <Box
                        sx={{
                          width: `${summary.neutrals.pct}%`,
                          bgcolor: 'warning.main',
                          transition: 'width 0.4s ease',
                        }}
                      />
                      <Box
                        sx={{
                          width: `${summary.detractors.pct}%`,
                          bgcolor: 'error.main',
                          transition: 'width 0.4s ease',
                        }}
                      />
                    </Box>

                    {/* Legenda de Distribuição */}
                    <Grid container spacing={2}>
                      <Grid size={{ xs: 12, sm: 4 }}>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <Box sx={{ width: 12, height: 12, borderRadius: '50%', bgcolor: 'success.main' }} />
                          <Typography variant="body2" sx={{ fontWeight: 600 }}>
                            Promotores (9–10):
                          </Typography>
                          <Typography variant="body2" color="text.secondary">
                            <strong>{summary.promoters.count}</strong> ({summary.promoters.pct.toFixed(1)}%)
                          </Typography>
                        </Box>
                      </Grid>
                      <Grid size={{ xs: 12, sm: 4 }}>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <Box sx={{ width: 12, height: 12, borderRadius: '50%', bgcolor: 'warning.main' }} />
                          <Typography variant="body2" sx={{ fontWeight: 600 }}>
                            Neutros (7–8):
                          </Typography>
                          <Typography variant="body2" color="text.secondary">
                            <strong>{summary.neutrals.count}</strong> ({summary.neutrals.pct.toFixed(1)}%)
                          </Typography>
                        </Box>
                      </Grid>
                      <Grid size={{ xs: 12, sm: 4 }}>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <Box sx={{ width: 12, height: 12, borderRadius: '50%', bgcolor: 'error.main' }} />
                          <Typography variant="body2" sx={{ fontWeight: 600 }}>
                            Detratores (0–6):
                          </Typography>
                          <Typography variant="body2" color="text.secondary">
                            <strong>{summary.detractors.count}</strong> ({summary.detractors.pct.toFixed(1)}%)
                          </Typography>
                        </Box>
                      </Grid>
                    </Grid>
                  </CardContent>
                </Card>
              )}

              {/* Tabela de Evolução Mensal e Ranking por Segmento */}
              <Grid container spacing={3}>
                {/* Série Mensal */}
                <Grid size={{ xs: 12, md: 7 }}>
                  <Card sx={{ border: 1, borderColor: 'divider', boxShadow: 'none', height: '100%' }}>
                    <CardContent sx={{ p: 3 }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                        <CalendarMonthIcon color="primary" fontSize="small" />
                        <Typography variant="subtitle1" sx={{ fontWeight: 700 }}>
                          Evolução Mensal do NPS
                        </Typography>
                      </Box>
                      {monthlyNps.length === 0 ? (
                        <Typography variant="body2" color="text.secondary" sx={{ py: 3, textAlign: 'center' }}>
                          Nenhuma série mensal encontrada para o recorte.
                        </Typography>
                      ) : (
                        <TableContainer component={Paper} elevation={0} sx={{ border: 1, borderColor: 'divider' }}>
                          <Table size="small" aria-label="Evolução Mensal do NPS">
                            <TableHead sx={{ bgcolor: 'background.default' }}>
                              <TableRow>
                                <TableCell sx={{ fontWeight: 700 }}>Mês</TableCell>
                                <TableCell align="right" sx={{ fontWeight: 700 }}>Respostas</TableCell>
                                <TableCell align="right" sx={{ fontWeight: 700 }}>NPS</TableCell>
                              </TableRow>
                            </TableHead>
                            <TableBody>
                              {monthlyNps.map((row) => (
                                <TableRow key={row.month} hover>
                                  <TableCell sx={{ fontWeight: 600 }}>{row.month}</TableCell>
                                  <TableCell align="right">{row.responses}</TableCell>
                                  <TableCell align="right">
                                    <Chip
                                      label={row.npsScore > 0 ? `+${row.npsScore}` : row.npsScore}
                                      color={getNpsScoreColor(row.npsScore)}
                                      size="small"
                                      sx={{ fontWeight: 700 }}
                                    />
                                  </TableCell>
                                </TableRow>
                              ))}
                            </TableBody>
                          </Table>
                        </TableContainer>
                      )}
                    </CardContent>
                  </Card>
                </Grid>

                {/* Ranking por Segmento */}
                <Grid size={{ xs: 12, md: 5 }}>
                  <Card sx={{ border: 1, borderColor: 'divider', boxShadow: 'none', height: '100%' }}>
                    <CardContent sx={{ p: 3 }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                        <InsightsIcon color="secondary" fontSize="small" />
                        <Typography variant="subtitle1" sx={{ fontWeight: 700 }}>
                          Ranking por Segmento
                        </Typography>
                      </Box>
                      {segmentNps.length === 0 ? (
                        <Typography variant="body2" color="text.secondary" sx={{ py: 3, textAlign: 'center' }}>
                          Nenhum segmento encontrado para o recorte.
                        </Typography>
                      ) : (
                        <TableContainer component={Paper} elevation={0} sx={{ border: 1, borderColor: 'divider' }}>
                          <Table size="small" aria-label="Ranking de NPS por Segmento">
                            <TableHead sx={{ bgcolor: 'background.default' }}>
                              <TableRow>
                                <TableCell sx={{ fontWeight: 700 }}>Segmento</TableCell>
                                <TableCell align="right" sx={{ fontWeight: 700 }}>Respostas</TableCell>
                                <TableCell align="right" sx={{ fontWeight: 700 }}>NPS</TableCell>
                              </TableRow>
                            </TableHead>
                            <TableBody>
                              {segmentNps.map((row) => (
                                <TableRow key={row.segment} hover>
                                  <TableCell sx={{ fontWeight: 600 }}>{row.segment}</TableCell>
                                  <TableCell align="right">{row.responses}</TableCell>
                                  <TableCell align="right">
                                    <Chip
                                      label={row.npsScore > 0 ? `+${row.npsScore}` : row.npsScore}
                                      color={getNpsScoreColor(row.npsScore)}
                                      size="small"
                                      sx={{ fontWeight: 700 }}
                                    />
                                  </TableCell>
                                </TableRow>
                              ))}
                            </TableBody>
                          </Table>
                        </TableContainer>
                      )}
                    </CardContent>
                  </Card>
                </Grid>
              </Grid>
            </>
          )}
        </>
      )}

      {/* Diálogo de Edição de Empresa */}
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
