import { Alert, DialogContentText, Typography } from '@mui/material'
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

  const responsesCount = survey?.responsesCount ?? 0
  const hasResponses = responsesCount > 0

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
      <DialogContentText sx={{ mb: hasResponses ? 1.5 : 1 }}>
        Tem certeza de que deseja desativar a pesquisa{' '}
        <strong>{survey?.name ?? ''}</strong>?
      </DialogContentText>

      {hasResponses ? (
        <Alert severity="warning" sx={{ mt: 1 }}>
          <Typography variant="body2" sx={{ fontWeight: 600, mb: 0.5 }}>
            Atenção: Esta pesquisa possui {responsesCount}{' '}
            {responsesCount === 1 ? 'avaliação' : 'avaliações'}.
          </Typography>
          <Typography variant="body2">
            Ao confirmar a desativação, você{' '}
            <strong>
              perderá o acesso a esta pesquisa e a todas as suas {responsesCount}{' '}
              {responsesCount === 1 ? 'avaliação' : 'avaliações'}
            </strong>
            , e esses resultados deixarão de compor as métricas e relatórios do Dashboard.
          </Typography>
        </Alert>
      ) : (
        <DialogContentText variant="caption" color="text.secondary" sx={{ display: 'block', mt: 1 }}>
          A pesquisa será desativada e deixará imediatamente de constar nas listagens e de receber novas avaliações.
        </DialogContentText>
      )}
    </ConfirmDeleteDialog>
  )
}
