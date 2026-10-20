import AddBusinessIcon from '@mui/icons-material/AddBusiness'
import BusinessIcon from '@mui/icons-material/Business'
import EditOutlinedIcon from '@mui/icons-material/EditOutlined'
import InsightsIcon from '@mui/icons-material/Insights'
import {
  AppBar as MuiAppBar,
  Box,
  CircularProgress,
  FormControl,
  IconButton,
  InputLabel,
  MenuItem,
  Select,
  type SelectChangeEvent,
  Skeleton,
  Toolbar,
  Tooltip,
  Typography,
} from '@mui/material'
import { useState } from 'react'
import { useCompany } from '../context/useCompany.ts'
import { CompanyDialog } from './CompanyDialog.tsx'

export function AppBar() {
  const { companies, selectedCompany, selectedCompanyId, selectCompany, refreshCompanies, loading } = useCompany()
  const [dialogOpen, setDialogOpen] = useState(false)
  const [dialogMode, setDialogMode] = useState<'create' | 'edit'>('create')

  const handleCompanyChange = (event: SelectChangeEvent<number>) => {
    const value = event.target.value
    if (typeof value === 'number' && value > 0) {
      selectCompany(value)
    }
  }

  const handleOpenCreate = () => {
    setDialogMode('create')
    setDialogOpen(true)
  }

  const handleOpenEdit = () => {
    setDialogMode('edit')
    setDialogOpen(true)
  }

  const handleDialogSuccess = (savedCompany: { readonly id: number }) => {
    void refreshCompanies(savedCompany.id)
  }

  return (
    <>
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

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, minWidth: 320 }}>
            {loading ? (
              <Skeleton variant="rounded" width={260} height={40} />
            ) : (
              <>
                <FormControl size="small" sx={{ minWidth: 200, flexGrow: 1 }}>
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

                <Tooltip title="Cadastrar nova empresa">
                  <IconButton
                    color="primary"
                    onClick={handleOpenCreate}
                    size="small"
                    aria-label="Cadastrar nova empresa"
                  >
                    <AddBusinessIcon fontSize="small" />
                  </IconButton>
                </Tooltip>

                <Tooltip title="Editar dados da empresa ativa">
                  <span>
                    <IconButton
                      color="primary"
                      onClick={handleOpenEdit}
                      disabled={selectedCompany === null}
                      size="small"
                      aria-label="Editar dados da empresa ativa"
                    >
                      <EditOutlinedIcon fontSize="small" />
                    </IconButton>
                  </span>
                </Tooltip>
              </>
            )}
            {loading && <CircularProgress size={20} color="primary" />}
          </Box>
        </Toolbar>
      </MuiAppBar>

      <CompanyDialog
        open={dialogOpen}
        mode={dialogMode}
        company={dialogMode === 'edit' ? selectedCompany : null}
        onClose={() => setDialogOpen(false)}
        onSuccess={handleDialogSuccess}
      />
    </>
  )
}
