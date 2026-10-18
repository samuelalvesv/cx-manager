import BusinessIcon from '@mui/icons-material/Business'
import InsightsIcon from '@mui/icons-material/Insights'
import {
  AppBar as MuiAppBar,
  Box,
  CircularProgress,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  type SelectChangeEvent,
  Skeleton,
  Toolbar,
  Typography,
} from '@mui/material'
import { useCompany } from '../context/useCompany.ts'

export function AppBar() {
  const { companies, selectedCompanyId, selectCompany, loading } = useCompany()

  const handleCompanyChange = (event: SelectChangeEvent<number>) => {
    const value = event.target.value
    if (typeof value === 'number' && value > 0) {
      selectCompany(value)
    }
  }

  return (
    <MuiAppBar position="static" color="inherit">
      <Toolbar sx={{ justifyContent: 'space-between', gap: 2 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
          <InsightsIcon sx={{ color: 'primary.main', fontSize: 28 }} />
          <Box>
            <Typography variant="h6" component="div" sx={{ fontWeight: 700, lineHeight: 1.2 }}>
              CxManager
            </Typography>
            <Typography variant="caption" color="text.secondary">
              Gestão de Experiência do Cliente
            </Typography>
          </Box>
        </Box>

        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, minWidth: 260 }}>
          {loading ? (
            <Skeleton variant="rounded" width={260} height={40} />
          ) : (
            <FormControl size="small" fullWidth>
              <InputLabel id="company-select-label" sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                <BusinessIcon fontSize="small" /> Empresa
              </InputLabel>
              <Select<number>
                labelId="company-select-label"
                id="company-select"
                value={selectedCompanyId ?? ''}
                label="Empresa"
                onChange={handleCompanyChange}
                disabled={companies.length === 0}
              >
                {companies.map((company) => (
                  <MenuItem key={company.id} value={company.id}>
                    {company.name}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          )}
          {loading && <CircularProgress size={20} color="primary" />}
        </Box>
      </Toolbar>
    </MuiAppBar>
  )
}
