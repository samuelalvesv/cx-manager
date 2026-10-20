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
  FormControlLabel,
  FormHelperText,
  FormLabel,
  Radio,
  RadioGroup,
  TextField,
  Typography,
} from '@mui/material'
import { useState, type ChangeEvent, type SubmitEvent } from 'react'
import { createSurvey, updateSurvey } from '../services/surveyService.ts'
import type { Survey, SurveyType } from '../types/survey.ts'

export interface SurveyDialogProps {
  readonly open: boolean
  readonly mode: 'create' | 'edit'
  readonly survey?: Survey | null
  readonly onClose: () => void
  readonly onSuccess: (savedSurvey: Survey) => void
}

interface SurveyFormProps {
  readonly mode: 'create' | 'edit'
  readonly survey: Survey | null
  readonly onClose: () => void
  readonly onSuccess: (savedSurvey: Survey) => void
}

function SurveyForm({ mode, survey, onClose, onSuccess }: SurveyFormProps) {
  const [name, setName] = useState(() => (mode === 'edit' && survey !== null ? survey.name : ''))
  const [type, setType] = useState<SurveyType>(() => (mode === 'edit' && survey !== null ? survey.type : 'NPS'))
  const [submitting, setSubmitting] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const isNameValid = name.trim().length >= 2 && name.trim().length <= 100
  const isFormValid = isNameValid

  const handleNameChange = (e: ChangeEvent<HTMLInputElement>) => {
    setName(e.target.value)
  }

  const handleTypeChange = (e: ChangeEvent<HTMLInputElement>) => {
    const val = e.target.value
    if (val === 'NPS' || val === 'CSAT') {
      setType(val)
    }
  }

  const handleSubmit = async (e: SubmitEvent<HTMLFormElement>) => {
    e.preventDefault()
    if (!isFormValid || submitting) {
      return
    }

    setSubmitting(true)
    setErrorMessage(null)

    try {
      const trimmedName = name.trim()

      if (mode === 'create') {
        const created = await createSurvey({
          name: trimmedName,
          type,
        })
        onSuccess(created)
        onClose()
      } else if (survey !== null) {
        const updated = await updateSurvey(survey.id, {
          name: trimmedName,
        })
        onSuccess(updated)
        onClose()
      }
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Falha ao salvar a pesquisa.'
      setErrorMessage(message)
    } finally {
      setSubmitting(false)
    }
  }

  const title = mode === 'create' ? 'Nova Pesquisa' : 'Editar Pesquisa'

  return (
    <form onSubmit={(e) => void handleSubmit(e)}>
      <DialogTitle sx={{ fontWeight: 700 }}>{title}</DialogTitle>
      <DialogContent dividers>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2.5, pt: 1 }}>
          {errorMessage !== null && (
            <Alert severity="error" sx={{ mb: 1 }}>
              {errorMessage}
            </Alert>
          )}

          <TextField
            label="Nome da Pesquisa"
            value={name}
            onChange={handleNameChange}
            required
            fullWidth
            size="small"
            error={name.length > 0 && !isNameValid}
            helperText={
              name.length > 0 && !isNameValid
                ? 'O nome deve ter entre 2 e 100 caracteres.'
                : 'Identificação da pesquisa (ex: NPS Pós-Treino, CSAT Atendimento).'
            }
            disabled={submitting}
          />

          {mode === 'create' ? (
            <FormControl component="fieldset" disabled={submitting}>
              <FormLabel component="legend" sx={{ fontSize: '0.875rem', fontWeight: 600, mb: 1 }}>
                Tipo de Métrica e Escala
              </FormLabel>
              <RadioGroup value={type} onChange={handleTypeChange}>
                <Box sx={{ mb: 1.5, p: 1.5, borderRadius: 1, border: '1px solid', borderColor: type === 'NPS' ? 'primary.main' : 'divider' }}>
                  <FormControlLabel
                    value="NPS"
                    control={<Radio size="small" />}
                    label={
                      <Box>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <Typography variant="body2" sx={{ fontWeight: 600 }}>
                            NPS — Net Promoter Score
                          </Typography>
                          <Chip label="Escala 0 a 10" size="small" color="primary" variant="outlined" />
                        </Box>
                        <Typography variant="caption" color="text.secondary">
                          Avaliação de lealdade com classificação em Detratores (0–6), Neutros (7–8) e Promotores (9–10).
                        </Typography>
                      </Box>
                    }
                  />
                </Box>
                <Box sx={{ p: 1.5, borderRadius: 1, border: '1px solid', borderColor: type === 'CSAT' ? 'secondary.main' : 'divider' }}>
                  <FormControlLabel
                    value="CSAT"
                    control={<Radio size="small" />}
                    label={
                      <Box>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <Typography variant="body2" sx={{ fontWeight: 600 }}>
                            CSAT — Customer Satisfaction
                          </Typography>
                          <Chip label="Escala 1 a 5" size="small" color="secondary" variant="outlined" />
                        </Box>
                        <Typography variant="caption" color="text.secondary">
                          Satisfação pontual baseada na média aritmética simples das notas de 1 a 5.
                        </Typography>
                      </Box>
                    }
                  />
                </Box>
              </RadioGroup>
              <FormHelperText>O tipo de métrica define a escala permitida para as respostas e não pode ser alterado após a criação.</FormHelperText>
            </FormControl>
          ) : (
            <Box sx={{ p: 1.5, bgcolor: 'background.default', borderRadius: 1 }}>
              <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 0.5 }}>
                Tipo de Métrica (fixo)
              </Typography>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <Chip
                  label={survey?.type === 'NPS' ? 'NPS (Escala 0 a 10)' : 'CSAT (Escala 1 a 5)'}
                  color={survey?.type === 'NPS' ? 'primary' : 'secondary'}
                  size="small"
                />
                <Typography variant="caption" color="text.secondary">
                  {survey?.type === 'NPS'
                    ? 'Net Promoter Score com classificação de 0 a 10.'
                    : 'Customer Satisfaction Score com média de 1 a 5.'}
                </Typography>
              </Box>
            </Box>
          )}
        </Box>
      </DialogContent>
      <DialogActions sx={{ px: 3, py: 2 }}>
        <Button onClick={onClose} disabled={submitting} color="inherit">
          Cancelar
        </Button>
        <Button
          type="submit"
          variant="contained"
          disabled={!isFormValid || submitting}
          startIcon={submitting ? <CircularProgress size={18} color="inherit" /> : null}
        >
          {submitting ? 'Salvando...' : 'Salvar'}
        </Button>
      </DialogActions>
    </form>
  )
}

export function SurveyDialog({
  open,
  mode,
  survey = null,
  onClose,
  onSuccess,
}: SurveyDialogProps) {
  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      {open && (
        <SurveyForm
          key={`${mode}-${survey?.id ?? 0}`}
          mode={mode}
          survey={survey}
          onClose={onClose}
          onSuccess={onSuccess}
        />
      )}
    </Dialog>
  )
}
