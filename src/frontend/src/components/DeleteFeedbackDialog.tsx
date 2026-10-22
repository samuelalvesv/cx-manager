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
import { deleteFeedback } from '../services/feedbackService.ts'
import type { FeedbackResponse } from '../types/feedback.ts'

export interface DeleteFeedbackDialogProps {
  readonly open: boolean
  readonly feedbackItem: FeedbackResponse | null
  readonly onClose: () => void
  readonly onSuccess: (deletedId: number) => void
}

export function DeleteFeedbackDialog({
  open,
  feedbackItem,
  onClose,
  onSuccess,
}: DeleteFeedbackDialogProps) {
  const [submitting, setSubmitting] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const handleDelete = async () => {
    if (feedbackItem === null || submitting) {
      return
    }

    setSubmitting(true)
    setErrorMessage(null)

    try {
      await deleteFeedback(feedbackItem.id)
      onSuccess(feedbackItem.id)
      onClose()
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Falha ao excluir a avaliação.'
      setErrorMessage(message)
    } finally {
      setSubmitting(false)
    }
  }

  const handleClose = () => {
    if (!submitting) {
      setErrorMessage(null)
      onClose()
    }
  }

  return (
    <Dialog
      open={open}
      onClose={handleClose}
      maxWidth="xs"
      fullWidth
      aria-labelledby="delete-feedback-dialog-title"
    >
      <DialogTitle id="delete-feedback-dialog-title" sx={{ fontWeight: 700 }}>
        Excluir Avaliação?
      </DialogTitle>
      <DialogContent>
        {errorMessage !== null && (
          <Alert severity="error" sx={{ mb: 2 }} onClose={() => setErrorMessage(null)}>
            {errorMessage}
          </Alert>
        )}
        <DialogContentText>
          Tem certeza de que deseja remover a avaliação da pesquisa{' '}
          <strong>{feedbackItem?.surveyName}</strong> (Nota: {feedbackItem?.score})? Esta ação
          realizará a exclusão lógica do registro e ele não entrará mais nos relatórios analíticos.
        </DialogContentText>
      </DialogContent>
      <DialogActions sx={{ px: 3, pb: 2.5 }}>
        <Button onClick={handleClose} disabled={submitting} color="inherit">
          Cancelar
        </Button>
        <Button
          onClick={() => { void handleDelete() }}
          color="error"
          variant="contained"
          disabled={submitting}
          startIcon={submitting ? <CircularProgress size={18} color="inherit" /> : null}
        >
          {submitting ? 'Excluindo...' : 'Confirmar Exclusão'}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
