import { DialogContentText } from '@mui/material'
import { deleteContact } from '../services/contactService.ts'
import type { Contact } from '../types/contact.ts'
import { ConfirmDeleteDialog } from './ConfirmDeleteDialog.tsx'

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
  const handleConfirm = async () => {
    if (contact === null) {
      return
    }
    await deleteContact(contact.id)
    onSuccess(contact.id)
  }

  return (
    <ConfirmDeleteDialog
      open={open}
      title="Excluir Contato"
      confirmLabel="Excluir Contato"
      submittingLabel="Excluindo..."
      defaultErrorMessage="Falha ao excluir o contato."
      onClose={onClose}
      onConfirm={handleConfirm}
    >
      <DialogContentText>
        Tem certeza de que deseja excluir o contato{' '}
        <strong>{contact?.name ?? ''}</strong> ({contact?.email ?? ''})?
      </DialogContentText>
      <DialogContentText variant="caption" color="text.secondary" sx={{ display: 'block', mt: 1 }}>
        O contato será desativado e deixará imediatamente de constar nas listagens ativas. O endereço de e-mail ficará disponível para novos cadastros futuros.
      </DialogContentText>
    </ConfirmDeleteDialog>
  )
}
