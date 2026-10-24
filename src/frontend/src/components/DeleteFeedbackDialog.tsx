import { DialogContentText } from '@mui/material'
import { deleteFeedback } from '../services/feedbackService.ts'
import type { FeedbackResponse } from '../types/feedback.ts'
import { ConfirmDeleteDialog } from './ConfirmDeleteDialog.tsx'

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
  const handleConfirm = async () => {
    if (feedbackItem === null) {
      return
    }
    await deleteFeedback(feedbackItem.id)
    onSuccess(feedbackItem.id)
  }

  return (
    <ConfirmDeleteDialog
      open={open}
      title="Excluir Avaliação?"
      confirmLabel="Confirmar Exclusão"
      submittingLabel="Excluindo..."
      defaultErrorMessage="Falha ao excluir a avaliação."
      onClose={onClose}
      onConfirm={handleConfirm}
    >
      <DialogContentText>
        Tem certeza de que deseja remover a avaliação da pesquisa{' '}
        <strong>{feedbackItem?.surveyName}</strong> (Nota: {feedbackItem?.score})? Esta avaliação
        será desativada e deixará de ser contabilizada nos indicadores e relatórios de satisfação.
      </DialogContentText>
    </ConfirmDeleteDialog>
  )
}
