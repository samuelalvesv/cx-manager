import CalendarMonthIcon from '@mui/icons-material/CalendarMonth'
import ShowChartIcon from '@mui/icons-material/ShowChart'
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
import { LineChart } from '@mui/x-charts/LineChart'
import { useState, type MouseEvent } from 'react'
import type { MonthlyNps } from '../../types/analytics.ts'
import { formatMonthYear, getNpsScoreColor } from './npsHelpers.ts'

export interface MonthlyNpsCardProps {
  readonly monthlyNps: readonly MonthlyNps[]
}

export function MonthlyNpsCard({ monthlyNps }: MonthlyNpsCardProps) {
  const [viewMode, setViewMode] = useState<'chart' | 'table'>('chart')

  const handleViewChange = (_: MouseEvent<HTMLElement>, nextView: 'chart' | 'table' | null) => {
    if (nextView !== null) {
      setViewMode(nextView)
    }
  }

  const xLabels = monthlyNps.map((item) => formatMonthYear(item.month))
  const npsSeriesData = monthlyNps.map((item) => item.npsScore)
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
            <CalendarMonthIcon color="primary" fontSize="small" />
            <Typography variant="subtitle1" sx={{ fontWeight: 700 }}>
              Evolução Mensal do NPS
            </Typography>
          </Box>

          {monthlyNps.length > 0 && (
            <ToggleButtonGroup
              size="small"
              value={viewMode}
              exclusive
              onChange={handleViewChange}
              aria-label="Modo de visualização da evolução mensal"
            >
              <ToggleButton value="chart" aria-label="Visualizar gráfico mensal">
                <Tooltip title="Gráfico">
                  <ShowChartIcon fontSize="small" />
                </Tooltip>
              </ToggleButton>
              <ToggleButton value="table" aria-label="Visualizar tabela mensal">
                <Tooltip title="Tabela">
                  <TableChartOutlinedIcon fontSize="small" />
                </Tooltip>
              </ToggleButton>
            </ToggleButtonGroup>
          )}
        </Box>

        {monthlyNps.length === 0 ? (
          <Typography variant="body2" color="text.secondary" sx={{ py: 3, textAlign: 'center' }}>
            Nenhuma avaliação registrada nos meses do período selecionado.
          </Typography>
        ) : viewMode === 'chart' ? (
          <Box sx={{ width: '100%' }}>
            <LineChart
              height={250}
              margin={{ left: 44, right: 28, top: 24, bottom: 28 }}
              xAxis={[{ scaleType: 'point', data: xLabels }]}
              yAxis={[{ min: yAxisMin, max: 100 }]}
              series={[
                {
                  data: npsSeriesData,
                  label: 'Índice NPS',
                  area: true,
                  color: '#0f766e',
                },
              ]}
              grid={{ horizontal: true }}
            />
          </Box>
        ) : (
          <TableContainer component={Paper} elevation={0} sx={{ border: 1, borderColor: 'divider' }}>
            <Table size="small" aria-label="Evolução Mensal do NPS">
              <TableHead sx={{ bgcolor: 'action.hover' }}>
                <TableRow>
                  <TableCell sx={{ fontWeight: 700 }}>Mês</TableCell>
                  <TableCell align="right" sx={{ fontWeight: 700 }}>Avaliações</TableCell>
                  <TableCell align="right" sx={{ fontWeight: 700 }}>Índice NPS</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {monthlyNps.map((row) => (
                  <TableRow key={row.month} hover>
                    <TableCell sx={{ fontWeight: 600 }}>{formatMonthYear(row.month)}</TableCell>
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
