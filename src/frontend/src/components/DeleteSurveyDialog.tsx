import { DialogContentText } from '@mui/material'
import { deleteSurvey } from '../services/surveyService.ts'
import type { Survey } from '../types/survey.ts'
import { ConfirmDeleteDialog } from './ConfirmDeleteDialog.tsx'

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
  const handleConfirm = async () => {
    if (survey === null) {
      return
    }
    await deleteSurvey(survey.id)
    onSuccess(survey.id)
  }

  return (
    <ConfirmDeleteDialog
      open={open}
      title="Desativar Pesquisa"
      confirmLabel="Desativar Pesquisa"
      submittingLabel="Desativando..."
      defaultErrorMessage="Falha ao desativar a pesquisa."
      onClose={onClose}
      onConfirm={handleConfirm}
    >
      <DialogContentText>
        Tem certeza de que deseja desativar a pesquisa{' '}
        <strong>{survey?.name ?? ''}</strong>?
      </DialogContentText>
      <DialogContentText variant="caption" color="text.secondary" sx={{ display: 'block', mt: 1 }}>
        A pesquisa será desativada e deixará imediatamente de receber novas respostas e de constar nas listagens ativas.
      </DialogContentText>
    </ConfirmDeleteDialog>
  )
}
