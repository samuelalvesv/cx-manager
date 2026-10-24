import BarChartIcon from '@mui/icons-material/BarChart'
import InsightsIcon from '@mui/icons-material/Insights'
import TableChartOutlinedIcon from '@mui/icons-material/TableChartOutlined'
import {
  Box,
  Card,
  CardContent,
  Chip,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  ToggleButton,
  ToggleButtonGroup,
  Tooltip,
  Typography,
} from '@mui/material'
import { BarChart } from '@mui/x-charts/BarChart'
import { useState, type MouseEvent } from 'react'
import type { SegmentNps } from '../../types/analytics.ts'
import { getNpsScoreColor } from './npsHelpers.ts'

export interface SegmentNpsCardProps {
  readonly segmentNps: readonly SegmentNps[]
}

export function SegmentNpsCard({ segmentNps }: SegmentNpsCardProps) {
  const [viewMode, setViewMode] = useState<'chart' | 'table'>('chart')

  const handleViewChange = (_: MouseEvent<HTMLElement>, nextView: 'chart' | 'table' | null) => {
    if (nextView !== null) {
      setViewMode(nextView)
    }
  }

  const xLabels = segmentNps.map((item) => item.segment)
  const npsSeriesData = segmentNps.map((item) => item.npsScore)
  const yAxisMin = npsSeriesData.some((score) => score < 0) ? -100 : 0

  return (
    <Card sx={{ border: 1, borderColor: 'divider', boxShadow: 'none', height: '100%' }}>
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
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <InsightsIcon color="secondary" fontSize="small" />
            <Typography variant="subtitle1" sx={{ fontWeight: 700 }}>
              Desempenho por Segmento
            </Typography>
          </Box>

          {segmentNps.length > 0 && (
            <ToggleButtonGroup
              size="small"
              value={viewMode}
              exclusive
              onChange={handleViewChange}
              aria-label="Modo de visualização do desempenho por segmento"
            >
              <ToggleButton value="chart" aria-label="Visualizar gráfico por segmento">
                <Tooltip title="Gráfico">
                  <BarChartIcon fontSize="small" />
                </Tooltip>
              </ToggleButton>
              <ToggleButton value="table" aria-label="Visualizar tabela por segmento">
                <Tooltip title="Tabela">
                  <TableChartOutlinedIcon fontSize="small" />
                </Tooltip>
              </ToggleButton>
            </ToggleButtonGroup>
          )}
        </Box>

        {segmentNps.length === 0 ? (
          <Typography variant="body2" color="text.secondary" sx={{ py: 3, textAlign: 'center' }}>
            Nenhuma avaliação por segmento no período selecionado.
          </Typography>
        ) : viewMode === 'chart' ? (
          <Box sx={{ width: '100%' }}>
            <BarChart
              height={250}
              borderRadius={6}
              margin={{ left: 44, right: 24, top: 24, bottom: 28 }}
              xAxis={[{ scaleType: 'band', data: xLabels }]}
              yAxis={[{ min: yAxisMin, max: 100 }]}
              series={[
                {
                  data: npsSeriesData,
                  label: 'Índice NPS',
                  color: '#6366f1',
                },
              ]}
              grid={{ horizontal: true }}
            />
          </Box>
        ) : (
          <TableContainer component={Paper} elevation={0} sx={{ border: 1, borderColor: 'divider' }}>
            <Table size="small" aria-label="Desempenho de NPS por Segmento">
              <TableHead sx={{ bgcolor: 'action.hover' }}>
                <TableRow>
                  <TableCell sx={{ fontWeight: 700 }}>Segmento</TableCell>
                  <TableCell align="right" sx={{ fontWeight: 700 }}>Avaliações</TableCell>
                  <TableCell align="right" sx={{ fontWeight: 700 }}>Índice NPS</TableCell>
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
  )
}
