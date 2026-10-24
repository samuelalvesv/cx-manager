import AddBusinessIcon from '@mui/icons-material/AddBusiness'
import BusinessIcon from '@mui/icons-material/Business'
import DeleteOutlinedIcon from '@mui/icons-material/DeleteOutlined'
import EditOutlinedIcon from '@mui/icons-material/EditOutlined'
import {
  CircularProgress,
  FormControl,
  IconButton,
  InputAdornment,
  InputLabel,
  MenuItem,
  Select,
  type SelectChangeEvent,
  Skeleton,
  Tooltip,
} from '@mui/material'
import { useState } from 'react'
import { useCompany } from '../context/useCompany.ts'
import { CompanyDialog } from './CompanyDialog.tsx'
import { DeleteCompanyDialog } from './DeleteCompanyDialog.tsx'

export function CompanySelectorBar() {
  const { companies, selectedCompany, selectedCompanyId, selectCompany, refreshCompanies, loading } = useCompany()
  const [dialogOpen, setDialogOpen] = useState(false)
  const [dialogMode, setDialogMode] = useState<'create' | 'edit'>('create')
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)

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

  const handleOpenDelete = () => {
    setDeleteDialogOpen(true)
  }

  const handleDialogSuccess = (savedCompany: { readonly id: number }) => {
    void refreshCompanies(savedCompany.id)
  }

  const handleDeleteSuccess = () => {
    void refreshCompanies()
  }

  return (
    <>
      {loading ? (
        <Skeleton variant="rounded" sx={{ flexGrow: 1, minWidth: 160, height: 40 }} />
      ) : (
        <>
          <FormControl size="small" sx={{ minWidth: 0, flexGrow: 1 }}>
            <InputLabel id="company-select-label">Empresa</InputLabel>
            <Select<number>
              labelId="company-select-label"
              id="company-select"
              value={selectedCompanyId ?? ''}
              label="Empresa"
              onChange={handleCompanyChange}
              disabled={companies.length === 0}
              startAdornment={
                <InputAdornment position="start">
                  <BusinessIcon fontSize="small" sx={{ color: 'primary.main' }} />
                </InputAdornment>
              }
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
              sx={{ border: 1, borderColor: 'divider' }}
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
                sx={{ border: 1, borderColor: 'divider' }}
              >
                <EditOutlinedIcon fontSize="small" />
              </IconButton>
            </span>
          </Tooltip>

          <Tooltip title="Desativar empresa ativa">
            <span>
              <IconButton
                color="error"
                onClick={handleOpenDelete}
                disabled={selectedCompany === null}
                size="small"
                aria-label="Desativar empresa ativa"
                sx={{ border: 1, borderColor: 'divider' }}
              >
                <DeleteOutlinedIcon fontSize="small" />
              </IconButton>
            </span>
          </Tooltip>
        </>
      )}
      {loading && <CircularProgress size={20} color="primary" />}

      <CompanyDialog
        open={dialogOpen}
        mode={dialogMode}
        company={dialogMode === 'edit' ? selectedCompany : null}
        onClose={() => setDialogOpen(false)}
        onSuccess={handleDialogSuccess}
      />

      <DeleteCompanyDialog
        open={deleteDialogOpen}
        company={selectedCompany}
        onClose={() => setDeleteDialogOpen(false)}
        onSuccess={handleDeleteSuccess}
      />
    </>
  )
}
