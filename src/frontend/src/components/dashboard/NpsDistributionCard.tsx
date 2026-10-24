import { Box, Card, CardContent, Grid, LinearProgress, Stack, Typography } from '@mui/material'
import { PieChart } from '@mui/x-charts/PieChart'
import type { AnalyticsSummary } from '../../types/analytics.ts'

export interface NpsDistributionCardProps {
  readonly summary: AnalyticsSummary
}

export function NpsDistributionCard({ summary }: NpsDistributionCardProps) {
  if (summary.npsResponses === 0) {
    return null
  }

  const pieData = [
    {
      id: 'promoters',
      value: summary.promoters.count,
      label: `Promotores (${summary.promoters.pct.toFixed(1)}%)`,
      color: '#16a34a',
    },
    {
      id: 'neutrals',
      value: summary.neutrals.count,
      label: `Neutros (${summary.neutrals.pct.toFixed(1)}%)`,
      color: '#d97706',
    },
    {
      id: 'detractors',
      value: summary.detractors.count,
      label: `Detratores (${summary.detractors.pct.toFixed(1)}%)`,
      color: '#dc2626',
    },
  ]

  const distributionRows = [
    {
      key: 'promoters',
      title: 'Promotores (9–10)',
      count: summary.promoters.count,
      pct: summary.promoters.pct,
      color: 'success' as const,
    },
    {
      key: 'neutrals',
      title: 'Neutros (7–8)',
      count: summary.neutrals.count,
      pct: summary.neutrals.pct,
      color: 'warning' as const,
    },
    {
      key: 'detractors',
      title: 'Detratores (0–6)',
      count: summary.detractors.count,
      pct: summary.detractors.pct,
      color: 'error' as const,
    },
  ]

  return (
    <Card sx={{ mb: 3, border: 1, borderColor: 'divider', boxShadow: 'none' }}>
      <CardContent sx={{ p: { xs: 2, sm: 3 } }}>
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            flexWrap: 'wrap',
            gap: 1,
            mb: 2,
          }}
        >
          <Typography variant="subtitle1" sx={{ fontWeight: 700 }}>
            Distribuição das Avaliações de Recomendação (NPS)
          </Typography>
          <Typography variant="caption" color="text.secondary">
            {summary.npsResponses} avaliações válidas
          </Typography>
        </Box>

        <Grid container spacing={3} sx={{ alignItems: 'center' }}>
          <Grid size={{ xs: 12, md: 5 }}>
            <Box sx={{ display: 'flex', justifyContent: 'center' }}>
              <PieChart
                height={180}
                hideLegend
                series={[
                  {
                    data: pieData,
                    innerRadius: 46,
                    outerRadius: 78,
                    paddingAngle: 2,
                    cornerRadius: 4,
                  },
                ]}
              />
            </Box>
          </Grid>

          <Grid size={{ xs: 12, md: 7 }}>
            <Stack spacing={2}>
              {distributionRows.map((row) => (
                <Box key={row.key}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.75 }}>
                    <Typography variant="body2" sx={{ fontWeight: 600 }}>
                      {row.title}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      <strong>{row.count}</strong> ({row.pct.toFixed(1)}%)
                    </Typography>
                  </Box>
                  <LinearProgress
                    variant="determinate"
                    value={row.pct}
                    color={row.color}
                    sx={{ height: 10, borderRadius: 5 }}
                  />
                </Box>
              ))}
            </Stack>
          </Grid>
        </Grid>
      </CardContent>
    </Card>
  )
}
