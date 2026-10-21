import {
  Alert,
  Button,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
} from '@mui/material'
import { useState } from 'react'
import { deleteSurvey } from '../services/surveyService.ts'
import type { Survey } from '../types/survey.ts'

export interface DeleteSurveyDialogProps {
  readonly open: boolean
  readonly survey: Survey | null
  readonly onClose: () => void
  readonly onSuccess: (deletedId: number) => void
}

export function DeleteSurveyDialog({
  open,
  survey,
  onClose,
  onSuccess,
}: DeleteSurveyDialogProps) {
  const [submitting, setSubmitting] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const handleDelete = async () => {
    if (survey === null || submitting) {
      return
    }

    setSubmitting(true)
    setErrorMessage(null)

    try {
      await deleteSurvey(survey.id)
      onSuccess(survey.id)
      onClose()
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Falha ao desativar a pesquisa.'
      setErrorMessage(message)
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="xs" fullWidth>
      <DialogTitle sx={{ fontWeight: 700 }}>Desativar Pesquisa</DialogTitle>
      <DialogContent dividers>
        {errorMessage !== null && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {errorMessage}
          </Alert>
        )}
        <DialogContentText>
          Tem certeza de que deseja desativar a pesquisa{' '}
          <strong>{survey?.name ?? ''}</strong>?
        </DialogContentText>
        <DialogContentText variant="caption" color="text.secondary" sx={{ display: 'block', mt: 1 }}>
          A pesquisa será desativada e deixará imediatamente de receber novas respostas e de constar nas listagens ativas.
        </DialogContentText>
      </DialogContent>
      <DialogActions sx={{ px: 3, py: 2 }}>
        <Button onClick={onClose} disabled={submitting} color="inherit">
          Cancelar
        </Button>
        <Button
          onClick={() => void handleDelete()}
          variant="contained"
          color="error"
          disabled={submitting}
          startIcon={submitting ? <CircularProgress size={18} color="inherit" /> : null}
        >
          {submitting ? 'Desativando...' : 'Desativar Pesquisa'}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
