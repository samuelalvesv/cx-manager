import RateReviewOutlinedIcon from '@mui/icons-material/RateReviewOutlined'
import StarBorderOutlinedIcon from '@mui/icons-material/StarBorderOutlined'
import TrendingUpIcon from '@mui/icons-material/TrendingUp'
import { Box, Card, CardContent, Chip, Grid, Rating, Typography } from '@mui/material'
import type { AnalyticsSummary } from '../../types/analytics.ts'
import { getNpsScoreColor, getNpsZoneLabel } from './npsHelpers.ts'

export interface AnalyticsKpiCardsProps {
  readonly summary: AnalyticsSummary
}

export function AnalyticsKpiCards({ summary }: AnalyticsKpiCardsProps) {
  return (
    <Grid container spacing={3} sx={{ mb: 3 }}>
      {/* Card NPS Score */}
      <Grid size={{ xs: 12, md: 4 }}>
        <Card sx={{ border: 1, borderColor: 'divider', boxShadow: 'none', height: '100%' }}>
          <CardContent sx={{ p: { xs: 2, sm: 3 } }}>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
              <Typography variant="overline" color="text.secondary" sx={{ fontWeight: 700 }}>
                Índice de Recomendação (NPS)
              </Typography>
              <TrendingUpIcon color={getNpsScoreColor(summary.npsScore)} fontSize="small" />
            </Box>
            <Box sx={{ display: 'flex', alignItems: 'baseline', flexWrap: 'wrap', gap: 1.5, mb: 1 }}>
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
              Baseado em <strong>{summary.npsResponses}</strong> avaliações de recomendação.
            </Typography>
          </CardContent>
        </Card>
      </Grid>

      {/* Card CSAT */}
      <Grid size={{ xs: 12, md: 4 }}>
        <Card sx={{ border: 1, borderColor: 'divider', boxShadow: 'none', height: '100%' }}>
          <CardContent sx={{ p: { xs: 2, sm: 3 } }}>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
              <Typography variant="overline" color="text.secondary" sx={{ fontWeight: 700 }}>
                Média de Satisfação (CSAT)
              </Typography>
              <StarBorderOutlinedIcon color="primary" fontSize="small" />
            </Box>
            <Box sx={{ display: 'flex', alignItems: 'center', flexWrap: 'wrap', gap: 1.5, mb: 1 }}>
              <Box sx={{ display: 'flex', alignItems: 'baseline', gap: 0.75 }}>
                <Typography variant="h3" component="div" sx={{ fontWeight: 800 }}>
                  {summary.csatAvg !== null ? summary.csatAvg.toFixed(2) : '—'}
                </Typography>
                <Typography variant="subtitle1" color="text.secondary" sx={{ fontWeight: 600 }}>
                  / 5.0
                </Typography>
              </Box>
              {summary.csatAvg !== null && (
                <Rating
                  name="csat-summary-rating"
                  value={summary.csatAvg}
                  precision={0.1}
                  readOnly
                  size="small"
                />
              )}
            </Box>
            <Typography variant="caption" color="text.secondary">
              {summary.csatAvg !== null
                ? 'Média das notas de satisfação na escala de 1 a 5.'
                : 'Nenhuma avaliação de satisfação no período selecionado.'}
            </Typography>
          </CardContent>
        </Card>
      </Grid>

      {/* Card Total de Respostas */}
      <Grid size={{ xs: 12, md: 4 }}>
        <Card sx={{ border: 1, borderColor: 'divider', boxShadow: 'none', height: '100%' }}>
          <CardContent sx={{ p: { xs: 2, sm: 3 } }}>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
              <Typography variant="overline" color="text.secondary" sx={{ fontWeight: 700 }}>
                Total de Avaliações
              </Typography>
              <RateReviewOutlinedIcon color="secondary" fontSize="small" />
            </Box>
            <Box sx={{ display: 'flex', alignItems: 'baseline', gap: 1, mb: 1 }}>
              <Typography variant="h3" component="div" sx={{ fontWeight: 800 }}>
                {summary.responsesCount}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                avaliações ativas
              </Typography>
            </Box>
            <Typography variant="caption" color="text.secondary">
              Volume total de respostas coletadas em pesquisas ativas.
            </Typography>
          </CardContent>
        </Card>
      </Grid>
    </Grid>
  )
}
