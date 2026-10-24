import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  TextField,
  ToggleButton,
  ToggleButtonGroup,
  Typography,
  type SelectChangeEvent,
} from '@mui/material'
import { useEffect, useState, type ChangeEvent, type MouseEvent, type SubmitEvent } from 'react'
import { createFeedback, updateFeedback } from '../services/feedbackService.ts'
import { getSurveys } from '../services/surveyService.ts'
import type { FeedbackResponse } from '../types/feedback.ts'
import type { Survey } from '../types/survey.ts'
import { CHANNEL_OPTIONS, formatChannelLabel, getChannelIcon } from './feedbacks/feedbackFormatters.tsx'
import { ScoreSelectorField } from './feedbacks/ScoreSelectorField.tsx'

export interface FeedbackDialogProps {
  readonly open: boolean
  readonly mode: 'create' | 'edit'
  readonly contactId: number
  readonly feedbackToEdit?: FeedbackResponse | null
  readonly onClose: () => void
  readonly onSuccess: (feedback: FeedbackResponse) => void
}

interface FeedbackFormProps {
  readonly mode: 'create' | 'edit'
  readonly contactId: number
  readonly feedbackToEdit: FeedbackResponse | null
  readonly onClose: () => void
  readonly onSuccess: (feedback: FeedbackResponse) => void
}

function FeedbackForm({ mode, contactId, feedbackToEdit, onClose, onSuccess }: FeedbackFormProps) {
  const [surveys, setSurveys] = useState<readonly Survey[]>([])
  const [loadingSurveys, setLoadingSurveys] = useState(mode === 'create')
  const [selectedSurveyId, setSelectedSurveyId] = useState<number>(() => {
    if (mode === 'edit' && feedbackToEdit !== null) {
      return feedbackToEdit.surveyId
    }
    return 0
  })

  const [score, setScore] = useState<number>(() => {
    if (mode === 'edit' && feedbackToEdit !== null) {
      return feedbackToEdit.score
    }
    return 10
  })

  const [channel, setChannel] = useState<string>(() => {
    if (mode === 'edit' && feedbackToEdit !== null) {
      return feedbackToEdit.channel
    }
    return 'whatsapp'
  })

  const [comment, setComment] = useState<string>(() => {
    if (mode === 'edit' && feedbackToEdit !== null && feedbackToEdit.comment !== null) {
      return feedbackToEdit.comment
    }
    return ''
  })

  const [submitting, setSubmitting] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  useEffect(() => {
    let active = true
    const controller = new AbortController()

    async function loadSurveys() {
      try {
        const list = await getSurveys(controller.signal)
        if (!active) {
          return
        }
        setSurveys(list)
        if (list.length > 0 && list[0] !== undefined) {
          setSelectedSurveyId(list[0].id)
          setScore(list[0].type === 'CSAT' ? 5 : 10)
        }
        setLoadingSurveys(false)
      } catch (err: unknown) {
        if (!active) {
          return
        }
        const message = err instanceof Error ? err.message : 'Falha ao carregar pesquisas disponíveis.'
        setErrorMessage(message)
        setLoadingSurveys(false)
      }
    }

    if (mode === 'create') {
      void loadSurveys()
    }

    return () => {
      active = false
      controller.abort()
    }
  }, [mode])

  const selectedSurvey = surveys.find((s) => s.id === selectedSurveyId)
  const surveyType: 'NPS' | 'CSAT' = mode === 'edit' && feedbackToEdit !== null
    ? (feedbackToEdit.surveyType === 'CSAT' ? 'CSAT' : 'NPS')
    : (selectedSurvey?.type === 'CSAT' ? 'CSAT' : 'NPS')

  const minScore = surveyType === 'CSAT' ? 1 : 0
  const maxScore = surveyType === 'CSAT' ? 5 : 10

  const handleSurveyChange = (e: SelectChangeEvent<number>) => {
    const newSurveyId = e.target.value
    setSelectedSurveyId(newSurveyId)
    const targetSurvey = surveys.find((s) => s.id === newSurveyId)
    if (targetSurvey?.type === 'CSAT') {
      if (score < 1 || score > 5) {
        setScore(5)
      }
    } else if (score < 0 || score > 10) {
      setScore(10)
    }
  }

  const handleChannelToggle = (_: MouseEvent<HTMLElement>, nextChannel: string | null) => {
    if (nextChannel !== null) {
      setChannel(nextChannel)
    }
  }

  const handleCommentChange = (e: ChangeEvent<HTMLInputElement>) => {
    setComment(e.target.value)
  }

  const isCommentValid = comment.trim().length <= 1000
  const isScoreValid = score >= minScore && score <= maxScore
  const isSurveyValid = mode === 'edit' || selectedSurveyId > 0
  const isFormValid = isCommentValid && isScoreValid && isSurveyValid

  const handleSubmit = async (e: SubmitEvent<HTMLFormElement>) => {
    e.preventDefault()
    if (!isFormValid || submitting) {
      return
    }

    setSubmitting(true)
    setErrorMessage(null)

    try {
      const trimmedComment = comment.trim().length > 0 ? comment.trim() : null

      if (mode === 'create') {
        const created = await createFeedback({
          surveyId: selectedSurveyId,
          contactId,
          score,
          channel,
          comment: trimmedComment,
        })
        onSuccess(created)
        onClose()
      } else if (feedbackToEdit !== null) {
        const updated = await updateFeedback(feedbackToEdit.id, {
          score,
          comment: trimmedComment,
        })
        onSuccess(updated)
        onClose()
      }
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Falha ao salvar a avaliação.'
      setErrorMessage(message)
      setSubmitting(false)
    }
  }

  return (
    <Box component="form" onSubmit={(e) => { void handleSubmit(e) }} noValidate>
      <DialogTitle id="feedback-dialog-title" sx={{ fontWeight: 700 }}>
        {mode === 'create' ? 'Registrar Nova Avaliação' : 'Editar Avaliação'}
      </DialogTitle>

      <DialogContent dividers>
        <Stack spacing={2.5} sx={{ pt: 1.5 }}>
          {errorMessage !== null && (
            <Alert severity="error" onClose={() => setErrorMessage(null)}>
              {errorMessage}
            </Alert>
          )}

          {mode === 'create' ? (
            loadingSurveys ? (
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, py: 1 }}>
                <CircularProgress size={20} />
                <Typography variant="body2" color="text.secondary">
                  Carregando pesquisas ativas...
                </Typography>
              </Box>
            ) : surveys.length === 0 ? (
              <Alert severity="warning">
                Nenhuma pesquisa ativa encontrada para esta empresa. Cadastre uma pesquisa antes de registrar avaliações.
              </Alert>
            ) : (
              <FormControl fullWidth size="small">
                <InputLabel id="survey-select-label">Pesquisa de Satisfação</InputLabel>
                <Select
                  labelId="survey-select-label"
                  id="survey-select"
                  value={selectedSurveyId}
                  label="Pesquisa de Satisfação"
                  onChange={handleSurveyChange}
                  disabled={submitting}
                >
                  {surveys.map((s) => (
                    <MenuItem key={s.id} value={s.id}>
                      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', width: '100%', gap: 1 }}>
                        <span>{s.name}</span>
                        <Chip
                          label={s.type === 'NPS' ? 'Recomendação (NPS)' : 'Satisfação (CSAT)'}
                          size="small"
                          color={s.type === 'NPS' ? 'primary' : 'secondary'}
                          variant="outlined"
                          sx={{ height: 20, fontSize: '0.75rem' }}
                        />
                      </Box>
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            )
          ) : (
            <Box sx={{ p: 1.5, bgcolor: 'action.hover', borderRadius: 1, border: 1, borderColor: 'divider' }}>
              <Typography variant="caption" color="text.secondary" sx={{ display: 'block' }}>
                Pesquisa Vinculada
              </Typography>
              <Box sx={{ display: 'flex', alignItems: 'center', flexWrap: 'wrap', gap: 1, mt: 0.5 }}>
                <Typography variant="body2" sx={{ fontWeight: 600 }}>
                  {feedbackToEdit?.surveyName}
                </Typography>
                <Chip
                  label={feedbackToEdit?.surveyType === 'NPS' ? 'Recomendação (NPS)' : 'Satisfação (CSAT)'}
                  size="small"
                  color={feedbackToEdit?.surveyType === 'NPS' ? 'primary' : 'secondary'}
                  variant="outlined"
                  sx={{ height: 20, fontSize: '0.75rem' }}
                />
                <Chip
                  icon={getChannelIcon(channel)}
                  label={formatChannelLabel(channel)}
                  size="small"
                  variant="outlined"
                  sx={{ height: 20, fontSize: '0.75rem' }}
                />
              </Box>
            </Box>
          )}

          <ScoreSelectorField
            surveyType={surveyType}
            score={score}
            disabled={submitting}
            onChange={setScore}
          />

          {mode === 'create' && (
            <Box>
              <Typography variant="caption" color="text.secondary" sx={{ display: 'block', fontWeight: 600, mb: 0.75 }}>
                Canal de Resposta
              </Typography>
              <ToggleButtonGroup
                value={channel}
                exclusive
                fullWidth
                size="small"
                disabled={submitting}
                onChange={handleChannelToggle}
                aria-label="Canal de resposta"
              >
                {CHANNEL_OPTIONS.map((c) => (
                  <ToggleButton
                    key={c.value}
                    value={c.value}
                    color="primary"
                    sx={{ gap: 0.75, fontWeight: 600, textTransform: 'none' }}
                  >
                    {getChannelIcon(c.value)}
                    {c.label}
                  </ToggleButton>
                ))}
              </ToggleButtonGroup>
            </Box>
          )}

          <TextField
            label="Comentário do Contato (Opcional)"
            value={comment}
            onChange={handleCommentChange}
            multiline
            rows={3}
            fullWidth
            size="small"
            placeholder="Relato ou observação enviada pelo contato..."
            disabled={submitting}
            helperText={`${String(comment.length)}/1000 caracteres`}
            error={!isCommentValid}
          />
        </Stack>
      </DialogContent>

      <DialogActions sx={{ px: 3, py: 2 }}>
        <Button onClick={onClose} disabled={submitting} color="inherit">
          Cancelar
        </Button>
        <Button
          type="submit"
          variant="contained"
          disabled={!isFormValid || submitting || (mode === 'create' && surveys.length === 0)}
          startIcon={submitting ? <CircularProgress size={18} color="inherit" /> : null}
        >
          {submitting
            ? 'Salvando...'
            : mode === 'create'
              ? 'Registrar Avaliação'
              : 'Salvar Alterações'}
        </Button>
      </DialogActions>
    </Box>
  )
}

export function FeedbackDialog({
  open,
  mode,
  contactId,
  feedbackToEdit = null,
  onClose,
  onSuccess,
}: FeedbackDialogProps) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="sm"
      fullWidth
      aria-labelledby="feedback-dialog-title"
    >
      <FeedbackForm
        key={`${mode}-${String(contactId)}-${String(feedbackToEdit?.id ?? 0)}`}
        mode={mode}
        contactId={contactId}
        feedbackToEdit={feedbackToEdit}
        onClose={onClose}
        onSuccess={onSuccess}
      />
    </Dialog>
  )
}
