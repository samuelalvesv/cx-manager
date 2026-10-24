import {
  Alert,
  Button,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
} from '@mui/material'
import { useState, type ReactNode } from 'react'

export interface ConfirmDeleteDialogProps {
  readonly open: boolean
  readonly title: string
  readonly confirmLabel: string
  readonly submittingLabel: string
  readonly defaultErrorMessage: string
  readonly children: ReactNode
  readonly onClose: () => void
  readonly onConfirm: () => Promise<void>
}

export function ConfirmDeleteDialog({
  open,
  title,
  confirmLabel,
  submittingLabel,
  defaultErrorMessage,
  children,
  onClose,
  onConfirm,
}: ConfirmDeleteDialogProps) {
  const [submitting, setSubmitting] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const handleConfirm = async () => {
    if (submitting) {
      return
    }

    setSubmitting(true)
    setErrorMessage(null)

    try {
      await onConfirm()
      onClose()
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : defaultErrorMessage
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
    <Dialog open={open} onClose={handleClose} maxWidth="xs" fullWidth>
      <DialogTitle sx={{ fontWeight: 700 }}>{title}</DialogTitle>
      <DialogContent dividers>
        {errorMessage !== null && (
          <Alert severity="error" sx={{ mb: 2 }} onClose={() => setErrorMessage(null)}>
            {errorMessage}
          </Alert>
        )}
        {children}
      </DialogContent>
      <DialogActions sx={{ px: 3, py: 2 }}>
        <Button onClick={handleClose} disabled={submitting} color="inherit">
          Cancelar
        </Button>
        <Button
          onClick={() => { void handleConfirm() }}
          variant="contained"
          color="error"
          disabled={submitting}
          startIcon={submitting ? <CircularProgress size={18} color="inherit" /> : null}
        >
          {submitting ? submittingLabel : confirmLabel}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
