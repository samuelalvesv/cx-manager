import ClearIcon from '@mui/icons-material/Clear'
import DownloadIcon from '@mui/icons-material/Download'
import FilterListIcon from '@mui/icons-material/FilterList'
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  FormControl,
  Grid,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Typography,
  type SelectChangeEvent,
} from '@mui/material'
import type { Survey } from '../../types/survey.ts'
import { DatePickerField } from '../DatePickerField.tsx'

export interface AnalyticsFilterBarProps {
  readonly fromDate: string
  readonly toDate: string
  readonly selectedSurveyId: number
  readonly surveys: readonly Survey[]
  readonly exportLoading: boolean
  readonly exportError: string | null
  readonly onFromDateChange: (newDate: string) => void
  readonly onToDateChange: (newDate: string) => void
  readonly onSurveyChange: (newSurveyId: number) => void
  readonly onClearFilters: () => void
  readonly onExportCsv: () => void
  readonly onDismissExportError: () => void
}

export function AnalyticsFilterBar({
  fromDate,
  toDate,
  selectedSurveyId,
  surveys,
  exportLoading,
  exportError,
  onFromDateChange,
  onToDateChange,
  onSurveyChange,
  onClearFilters,
  onExportCsv,
  onDismissExportError,
}: AnalyticsFilterBarProps) {
  const hasActiveFilters = fromDate.length > 0 || toDate.length > 0 || selectedSurveyId > 0

  const handleSurveySelect = (event: SelectChangeEvent<number>) => {
    const value = event.target.value
    onSurveyChange(typeof value === 'number' ? value : Number(value))
  }

  return (
    <Paper sx={{ p: { xs: 2, sm: 2.5 }, mb: 3, border: 1, borderColor: 'divider', boxShadow: 'none' }}>
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          mb: 2,
          flexWrap: 'wrap',
          gap: 1.5,
        }}
      >
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <FilterListIcon color="primary" fontSize="small" />
          <Typography variant="subtitle2" sx={{ fontWeight: 700 }}>
            Filtros do Painel Analítico
          </Typography>
        </Box>
        <Button
          variant="contained"
          color="primary"
          size="small"
          disabled={exportLoading}
          startIcon={
            exportLoading ? (
              <CircularProgress size={16} color="inherit" />
            ) : (
              <DownloadIcon fontSize="small" />
            )
          }
          onClick={onExportCsv}
          sx={{ fontWeight: 600, width: { xs: '100%', sm: 'auto' } }}
        >
          {exportLoading ? 'Exportando planilha...' : 'Exportar Planilha'}
        </Button>
      </Box>

      {exportError !== null && (
        <Alert severity="error" onClose={onDismissExportError} sx={{ mb: 2 }}>
          {exportError}
        </Alert>
      )}

      <Grid container spacing={2} sx={{ alignItems: 'center' }}>
        <Grid size={{ xs: 12, sm: 4, md: 3 }}>
          <DatePickerField
            label="Data Inicial"
            value={fromDate}
            onChange={onFromDateChange}
            maxDate={toDate.length > 0 ? toDate : undefined}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 4, md: 3 }}>
          <DatePickerField
            label="Data Final"
            value={toDate}
            onChange={onToDateChange}
            minDate={fromDate.length > 0 ? fromDate : undefined}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 4, md: 4 }}>
          <FormControl size="small" fullWidth>
            <InputLabel id="survey-filter-label">Pesquisa</InputLabel>
            <Select
              labelId="survey-filter-label"
              value={selectedSurveyId}
              label="Pesquisa"
              onChange={handleSurveySelect}
            >
              <MenuItem value={0}>Todas as pesquisas</MenuItem>
              {surveys.map((survey) => (
                <MenuItem key={survey.id} value={survey.id}>
                  {survey.name} ({survey.type === 'NPS' ? 'Recomendação' : 'Satisfação'})
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Grid>
        <Grid size={{ xs: 12, sm: 12, md: 2 }}>
          <Button
            variant="outlined"
            size="small"
            fullWidth
            disabled={!hasActiveFilters}
            startIcon={<ClearIcon />}
            onClick={onClearFilters}
            sx={{ height: 40, fontWeight: 600 }}
          >
            Limpar
          </Button>
        </Grid>
      </Grid>
    </Paper>
  )
}
