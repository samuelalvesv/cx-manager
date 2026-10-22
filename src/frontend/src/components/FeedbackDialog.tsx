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
  Typography,
  type SelectChangeEvent,
} from '@mui/material'
import { useEffect, useState, type ChangeEvent, type SubmitEvent } from 'react'
import { createFeedback, updateFeedback } from '../services/feedbackService.ts'
import { getSurveys } from '../services/surveyService.ts'
import type { FeedbackResponse } from '../types/feedback.ts'
import type { Survey } from '../types/survey.ts'

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

const CHANNEL_OPTIONS = [
  { value: 'whatsapp', label: 'WhatsApp' },
  { value: 'email', label: 'E-mail' },
  { value: 'link', label: 'Link Web' },
] as const

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
    } else {
      if (score < 0 || score > 10) {
        setScore(10)
      }
    }
  }

  const handleScoreChange = (e: SelectChangeEvent<number>) => {
    setScore(e.target.value)
  }

  const handleChannelChange = (e: SelectChangeEvent) => {
    setChannel(e.target.value)
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

  const availableScores = Array.from(
    { length: maxScore - minScore + 1 },
    (_, i) => maxScore - i,
  )

  return (
    <Box component="form" onSubmit={(e) => { void handleSubmit(e) }} noValidate>
      <DialogTitle sx={{ pb: 1, fontWeight: 700 }}>
        {mode === 'create' ? 'Registrar Nova Avaliação' : 'Editar Avaliação'}
      </DialogTitle>

      <DialogContent sx={{ pt: 2 }}>
        <Stack spacing={2.5}>
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
                      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', width: '100%' }}>
                        <span>{s.name}</span>
                        <Chip
                          label={s.type}
                          size="small"
                          color={s.type === 'NPS' ? 'primary' : 'secondary'}
                          variant="outlined"
                          sx={{ ml: 1, height: 20, fontSize: '0.75rem' }}
                        />
                      </Box>
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            )
          ) : (
            <Box sx={{ p: 1.5, bgcolor: 'background.default', borderRadius: 1, border: 1, borderColor: 'divider' }}>
              <Typography variant="caption" color="text.secondary" sx={{ display: 'block' }}>
                Pesquisa Vinculada
              </Typography>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 0.5 }}>
                <Typography variant="body2" sx={{ fontWeight: 600 }}>
                  {feedbackToEdit?.surveyName}
                </Typography>
                <Chip
                  label={feedbackToEdit?.surveyType}
                  size="small"
                  color={feedbackToEdit?.surveyType === 'NPS' ? 'primary' : 'secondary'}
                  variant="outlined"
                  sx={{ height: 20, fontSize: '0.75rem' }}
                />
              </Box>
            </Box>
          )}

          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
            <FormControl fullWidth size="small">
              <InputLabel id="score-select-label">Nota ({surveyType}: {minScore} a {maxScore})</InputLabel>
              <Select
                labelId="score-select-label"
                id="score-select"
                value={score}
                label={`Nota (${surveyType}: ${minScore} a {maxScore})`}
                onChange={handleScoreChange}
                disabled={submitting}
              >
                {availableScores.map((val) => (
                  <MenuItem key={val} value={val}>
                    {val} {surveyType === 'NPS' ? (val >= 9 ? '— Promotor' : val >= 7 ? '— Neutro' : '— Detrator') : ''}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>

            {mode === 'create' ? (
              <FormControl fullWidth size="small">
                <InputLabel id="channel-select-label">Canal de Resposta</InputLabel>
                <Select
                  labelId="channel-select-label"
                  id="channel-select"
                  value={channel}
                  label="Canal de Resposta"
                  onChange={handleChannelChange}
                  disabled={submitting}
                >
                  {CHANNEL_OPTIONS.map((c) => (
                    <MenuItem key={c.value} value={c.value}>
                      {c.label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            ) : (
              <FormControl fullWidth size="small" disabled>
                <InputLabel id="channel-disabled-label">Canal</InputLabel>
                <Select
                  labelId="channel-disabled-label"
                  value={channel}
                  label="Canal"
                >
                  <MenuItem value={channel}>
                    {CHANNEL_OPTIONS.find((c) => c.value === channel)?.label ?? channel}
                  </MenuItem>
                </Select>
              </FormControl>
            )}
          </Stack>

          <TextField
            label="Comentário do Aluno (Opcional)"
            value={comment}
            onChange={handleCommentChange}
            multiline
            rows={3}
            fullWidth
            size="small"
            placeholder="Feedback registrado pelo aluno..."
            disabled={submitting}
            helperText={`${comment.length}/1000 caracteres`}
            error={!isCommentValid}
          />
        </Stack>
      </DialogContent>

      <DialogActions sx={{ px: 3, pb: 2.5 }}>
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
      {open ? (
        <FeedbackForm
          mode={mode}
          contactId={contactId}
          feedbackToEdit={feedbackToEdit}
          onClose={onClose}
          onSuccess={onSuccess}
        />
      ) : null}
    </Dialog>
  )
}
