import {
  Alert,
  Autocomplete,
  Box,
  Button,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  TextField,
} from '@mui/material'
import { useState, type ChangeEvent, type SubmitEvent } from 'react'
import { createContact, updateContact } from '../services/contactService.ts'
import type { Contact } from '../types/contact.ts'

export interface ContactDialogProps {
  readonly open: boolean
  readonly mode: 'create' | 'edit'
  readonly contact?: Contact | null
  readonly onClose: () => void
  readonly onSuccess: (savedContact: Contact) => void
}

interface ContactFormProps {
  readonly mode: 'create' | 'edit'
  readonly contact: Contact | null
  readonly onClose: () => void
  readonly onSuccess: (savedContact: Contact) => void
}

const COMMON_SEGMENTS = ['Plano Black', 'Plano Fit', 'Corporativo', 'Personal']

function validateEmailFormat(email: string): boolean {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())
}

function ContactForm({ mode, contact, onClose, onSuccess }: ContactFormProps) {
  const [name, setName] = useState(() => (mode === 'edit' && contact !== null ? contact.name : ''))
  const [email, setEmail] = useState(() => (mode === 'edit' && contact !== null ? contact.email : ''))
  const [segment, setSegment] = useState(() => (mode === 'edit' && contact !== null ? (contact.segment ?? '') : ''))
  const [submitting, setSubmitting] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const isNameValid = name.trim().length >= 2 && name.trim().length <= 100
  const isEmailValid = email.trim().length >= 5 && email.trim().length <= 150 && validateEmailFormat(email)
  const isSegmentValid = segment.trim().length === 0 || (segment.trim().length >= 2 && segment.trim().length <= 50)
  const isFormValid = isNameValid && isEmailValid && isSegmentValid

  const handleNameChange = (e: ChangeEvent<HTMLInputElement>) => {
    setName(e.target.value)
  }

  const handleEmailChange = (e: ChangeEvent<HTMLInputElement>) => {
    setEmail(e.target.value)
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
      const trimmedEmail = email.trim().toLowerCase()
      const trimmedSegment = segment.trim().length > 0 ? segment.trim() : null

      if (mode === 'create') {
        const created = await createContact({
          name: trimmedName,
          email: trimmedEmail,
          segment: trimmedSegment,
        })
        onSuccess(created)
        onClose()
      } else if (contact !== null) {
        const updated = await updateContact(contact.id, {
          name: trimmedName,
          email: trimmedEmail,
          segment: trimmedSegment,
        })
        onSuccess(updated)
        onClose()
      }
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Falha ao salvar o contato.'
      setErrorMessage(message)
    } finally {
      setSubmitting(false)
    }
  }

  const title = mode === 'create' ? 'Novo Contato' : 'Editar Contato'

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
            label="Nome Completo"
            value={name}
            onChange={handleNameChange}
            required
            fullWidth
            size="small"
            error={name.length > 0 && !isNameValid}
            helperText={
              name.length > 0 && !isNameValid
                ? 'O nome deve ter entre 2 e 100 caracteres.'
                : 'Nome de exibição do contato.'
            }
            disabled={submitting}
          />

          <TextField
            label="E-mail"
            type="email"
            value={email}
            onChange={handleEmailChange}
            required
            fullWidth
            size="small"
            error={email.length > 0 && !isEmailValid}
            helperText={
              email.length > 0 && !isEmailValid
                ? 'Informe um endereço de e-mail válido (ex: usuario@empresa.com.br).'
                : 'E-mail corporativo ou pessoal para envio de pesquisas.'
            }
            disabled={submitting}
          />

          <Autocomplete
            freeSolo
            options={COMMON_SEGMENTS}
            value={segment}
            onChange={(_, newValue) => {
              setSegment(newValue ?? '')
            }}
            onInputChange={(_, newInputValue) => {
              setSegment(newInputValue)
            }}
            disabled={submitting}
            renderInput={(params) => (
              <TextField
                {...params}
                label="Plano / Segmento"
                size="small"
                error={segment.length > 0 && !isSegmentValid}
                helperText={
                  segment.length > 0 && !isSegmentValid
                    ? 'O segmento deve ter entre 2 e 50 caracteres.'
                    : 'Opcional. Classificação do aluno ou cliente (ex: Plano Black, Plano Fit, Corporativo).'
                }
              />
            )}
          />
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

export function ContactDialog({
  open,
  mode,
  contact = null,
  onClose,
  onSuccess,
}: ContactDialogProps) {
  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      {open && (
        <ContactForm
          key={`${mode}-${contact?.id ?? 0}`}
          mode={mode}
          contact={contact}
          onClose={onClose}
          onSuccess={onSuccess}
        />
      )}
    </Dialog>
  )
}
