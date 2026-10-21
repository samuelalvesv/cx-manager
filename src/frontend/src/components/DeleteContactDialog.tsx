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
import { deleteContact } from '../services/contactService.ts'
import type { Contact } from '../types/contact.ts'

export interface DeleteContactDialogProps {
  readonly open: boolean
  readonly contact: Contact | null
  readonly onClose: () => void
  readonly onSuccess: (deletedId: number) => void
}

export function DeleteContactDialog({
  open,
  contact,
  onClose,
  onSuccess,
}: DeleteContactDialogProps) {
  const [submitting, setSubmitting] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const handleDelete = async () => {
    if (contact === null || submitting) {
      return
    }

    setSubmitting(true)
    setErrorMessage(null)

    try {
      await deleteContact(contact.id)
      onSuccess(contact.id)
      onClose()
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Falha ao excluir o contato.'
      setErrorMessage(message)
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="xs" fullWidth>
      <DialogTitle sx={{ fontWeight: 700 }}>Excluir Contato</DialogTitle>
      <DialogContent dividers>
        {errorMessage !== null && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {errorMessage}
          </Alert>
        )}
        <DialogContentText>
          Tem certeza de que deseja excluir o contato{' '}
          <strong>{contact?.name ?? ''}</strong> ({contact?.email ?? ''})?
        </DialogContentText>
        <DialogContentText variant="caption" color="text.secondary" sx={{ display: 'block', mt: 1 }}>
          O contato será desativado e deixará imediatamente de constar nas listagens ativas. O endereço de e-mail ficará disponível para novos cadastros futuros.
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
          {submitting ? 'Excluindo...' : 'Excluir Contato'}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
