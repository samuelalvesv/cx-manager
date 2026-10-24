import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  TextField,
} from '@mui/material'
import { useState, type ChangeEvent, type SubmitEvent } from 'react'
import { createCompany, updateCompany } from '../services/companyService.ts'
import type { Company } from '../types/company.ts'

export interface CompanyDialogProps {
  readonly open: boolean
  readonly mode: 'create' | 'edit'
  readonly company?: Company | null
  readonly onClose: () => void
  readonly onSuccess: (savedCompany: Company) => void
}

interface CompanyFormProps {
  readonly mode: 'create' | 'edit'
  readonly company: Company | null
  readonly onClose: () => void
  readonly onSuccess: (savedCompany: Company) => void
}

function CompanyForm({ mode, company, onClose, onSuccess }: CompanyFormProps) {
  const [name, setName] = useState(() => (mode === 'edit' && company !== null ? company.name : ''))
  const [description, setDescription] = useState(() => (mode === 'edit' && company !== null ? company.description : ''))
  const [submitting, setSubmitting] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const isNameValid = name.trim().length >= 2 && name.trim().length <= 100
  const isDescriptionValid = description.trim().length >= 2 && description.trim().length <= 500
  const isFormValid = isNameValid && isDescriptionValid

  const handleNameChange = (e: ChangeEvent<HTMLInputElement>) => {
    setName(e.target.value)
  }

  const handleDescriptionChange = (e: ChangeEvent<HTMLInputElement>) => {
    setDescription(e.target.value)
  }

  const handleSubmit = async (e: SubmitEvent<HTMLFormElement>) => {
    e.preventDefault()
    if (!isFormValid || submitting) {
      return
    }

    setSubmitting(true)
    setErrorMessage(null)

    try {
      const trimmedName = name.trim()
      const trimmedDescription = description.trim()

      if (mode === 'create') {
        const created = await createCompany({
          name: trimmedName,
          description: trimmedDescription,
        })
        onSuccess(created)
        onClose()
      } else if (company !== null) {
        const updated = await updateCompany(company.id, {
          name: trimmedName,
          description: trimmedDescription,
        })
        onSuccess(updated)
        onClose()
      }
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Falha ao salvar os dados da empresa.'
      setErrorMessage(message)
    } finally {
      setSubmitting(false)
    }
  }

  const title = mode === 'create' ? 'Cadastrar Nova Empresa' : 'Editar Empresa'

  return (
    <form onSubmit={(e) => void handleSubmit(e)}>
      <DialogTitle sx={{ fontWeight: 700 }}>{title}</DialogTitle>
      <DialogContent dividers>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2.5, pt: 1.5 }}>
          {errorMessage !== null && (
            <Alert severity="error" sx={{ mb: 1 }}>
              {errorMessage}
            </Alert>
          )}

          <TextField
            label="Nome da Empresa"
            value={name}
            onChange={handleNameChange}
            required
            fullWidth
            size="small"
            error={name.length > 0 && !isNameValid}
            helperText={
              name.length > 0 && !isNameValid
                ? 'O nome deve ter entre 2 e 100 caracteres.'
                : 'Identificação principal da rede ou unidade.'
            }
            disabled={submitting}
          />

          <TextField
            label="Descrição"
            value={description}
            onChange={handleDescriptionChange}
            required
            fullWidth
            multiline
            rows={3}
            size="small"
            error={description.length > 0 && !isDescriptionValid}
            helperText={
              description.length > 0 && !isDescriptionValid
                ? 'A descrição deve ter entre 2 e 500 caracteres.'
                : 'Resumo sobre a atuação ou modalidade da empresa.'
            }
            disabled={submitting}
          />
        </Box>
      </DialogContent>
      <DialogActions sx={{ px: 3, py: 2 }}>
        <Button onClick={onClose} disabled={submitting} color="inherit">
          Cancelar
        </Button>
        <Button
          type="submit"
          variant="contained"
          disabled={!isFormValid || submitting}
          startIcon={submitting ? <CircularProgress size={18} color="inherit" /> : null}
        >
          {submitting ? 'Salvando...' : 'Salvar'}
        </Button>
      </DialogActions>
    </form>
  )
}

export function CompanyDialog({
  open,
  mode,
  company = null,
  onClose,
  onSuccess,
}: CompanyDialogProps) {
  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <CompanyForm
        key={`${mode}-${company?.id ?? 0}`}
        mode={mode}
        company={company}
        onClose={onClose}
        onSuccess={onSuccess}
      />
    </Dialog>
  )
}
