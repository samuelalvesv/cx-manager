import AddIcon from '@mui/icons-material/Add'
import CloseIcon from '@mui/icons-material/Close'
import DeleteOutlinedIcon from '@mui/icons-material/DeleteOutlined'
import EditOutlinedIcon from '@mui/icons-material/EditOutlined'
import EmailOutlinedIcon from '@mui/icons-material/EmailOutlined'
import LinkOutlinedIcon from '@mui/icons-material/LinkOutlined'
import RateReviewOutlinedIcon from '@mui/icons-material/RateReviewOutlined'
import RefreshIcon from '@mui/icons-material/Refresh'
import WhatsAppIcon from '@mui/icons-material/WhatsApp'
import {
  Alert,
  Box,
  Button,
  Card,
  Chip,
  Divider,
  Drawer,
  IconButton,
  Skeleton,
  Stack,
  Tooltip,
  Typography,
} from '@mui/material'
import { useEffect, useState } from 'react'
import { getContactFeedbacks } from '../services/feedbackService.ts'
import type { Contact } from '../types/contact.ts'
import type { FeedbackResponse } from '../types/feedback.ts'
import { DeleteFeedbackDialog } from './DeleteFeedbackDialog.tsx'
import { EmptyState } from './EmptyState.tsx'
import { FeedbackDialog } from './FeedbackDialog.tsx'

export interface ContactFeedbacksDrawerProps {
  readonly open: boolean
  readonly contact: Contact | null
  readonly onClose: () => void
}

function getChannelIcon(channel: string) {
  const normalized = channel.toLowerCase()
  if (normalized.includes('whatsapp')) {
    return <WhatsAppIcon sx={{ fontSize: 16 }} />
  }
  if (normalized.includes('email') || normalized.includes('e-mail')) {
    return <EmailOutlinedIcon sx={{ fontSize: 16 }} />
  }
  return <LinkOutlinedIcon sx={{ fontSize: 16 }} />
}

function getScoreChipProps(score: number, surveyType: string) {
  const isNps = surveyType.toUpperCase() === 'NPS'

  if (isNps) {
    if (score >= 9) {
      return {
        label: `${score} • Promotor`,
        color: 'success' as const,
      }
    }
    if (score >= 7) {
      return {
        label: `${score} • Neutro`,
        color: 'warning' as const,
      }
    }
    return {
      label: `${score} • Detrator`,
      color: 'error' as const,
    }
  }

  // CSAT (1-5)
  if (score >= 4) {
    return {
      label: `${score}/5 • Satisfeito`,
      color: 'success' as const,
    }
  }
  if (score === 3) {
    return {
      label: `${score}/5 • Neutro`,
      color: 'warning' as const,
    }
  }
  return {
    label: `${score}/5 • Insatisfeito`,
    color: 'error' as const,
  }
}

export function ContactFeedbacksDrawer({
  open,
  contact,
  onClose,
}: ContactFeedbacksDrawerProps) {
  const [feedbacks, setFeedbacks] = useState<readonly FeedbackResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // Dialog states
  const [dialogOpen, setDialogOpen] = useState(false)
  const [dialogMode, setDialogMode] = useState<'create' | 'edit'>('create')
  const [feedbackToEdit, setFeedbackToEdit] = useState<FeedbackResponse | null>(null)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [feedbackToDelete, setFeedbackToDelete] = useState<FeedbackResponse | null>(null)

  const contactId = contact?.id

  useEffect(() => {
    let active = true
    const controller = new AbortController()

    async function fetchFeedbacks() {
      if (contactId === undefined) {
        return
      }

      setLoading(true)
      try {
        const data = await getContactFeedbacks(contactId, controller.signal)
        if (!active) {
          return
        }
        setFeedbacks(data)
        setError(null)
        setLoading(false)
      } catch (err: unknown) {
        if (!active || (err instanceof Error && err.name === 'AbortError')) {
          return
        }
        const message = err instanceof Error ? err.message : 'Falha ao carregar o histórico de avaliações.'
        setError(message)
        setLoading(false)
      }
    }

    if (open && contactId !== undefined) {
      void fetchFeedbacks()
    }

    return () => {
      active = false
      controller.abort()
    }
  }, [open, contactId])

  const handleRetry = async () => {
    if (contactId === undefined) {
      return
    }
    setLoading(true)
    setError(null)
    try {
      const data = await getContactFeedbacks(contactId)
      setFeedbacks(data)
      setLoading(false)
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Falha ao carregar o histórico de avaliações.'
      setError(message)
      setLoading(false)
    }
  }

  const handleOpenCreate = () => {
    setDialogMode('create')
    setFeedbackToEdit(null)
    setDialogOpen(true)
  }

  const handleOpenEdit = (item: FeedbackResponse) => {
    setDialogMode('edit')
    setFeedbackToEdit(item)
    setDialogOpen(true)
  }

  const handleOpenDelete = (item: FeedbackResponse) => {
    setFeedbackToDelete(item)
    setDeleteDialogOpen(true)
  }

  const handleDialogSuccess = (saved: FeedbackResponse) => {
    setFeedbacks((prev) => {
      if (dialogMode === 'create') {
        return [saved, ...prev]
      }
      return prev.map((r) => (r.id === saved.id ? saved : r))
    })
  }

  const handleDeleteSuccess = (deletedId: number) => {
    setFeedbacks((prev) => prev.filter((r) => r.id !== deletedId))
  }

  return (
    <Drawer
      anchor="right"
      open={open}
      onClose={onClose}
      slotProps={{
        paper: {
          sx: {
            width: { xs: '100%', sm: 500 },
            boxSizing: 'border-box',
          },
        },
      }}
    >
      {/* Header da Gaveta */}
      <Box sx={{ p: 2.5, bgcolor: 'background.default', borderBottom: 1, borderColor: 'divider' }}>
        <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', mb: 1 }}>
          <Box>
            <Typography variant="h6" sx={{ fontWeight: 700 }}>
              Histórico de Avaliações
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {contact?.name ?? 'Contato'}
            </Typography>
          </Box>
          <IconButton
            size="small"
            onClick={onClose}
            aria-label="Fechar gaveta de histórico"
            edge="end"
          >
            <CloseIcon fontSize="small" />
          </IconButton>
        </Box>

        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap', mt: 1 }}>
          <Typography variant="caption" color="text.secondary">
            {contact?.email}
          </Typography>
          {contact?.segment !== undefined && (
            <Chip
              label={contact.segment}
              size="small"
              variant="outlined"
              color={contact.segment.toLowerCase().includes('black') ? 'primary' : 'default'}
              sx={{ height: 20, fontSize: '0.75rem' }}
            />
          )}
        </Box>

        <Box sx={{ mt: 2 }}>
          <Button
            variant="contained"
            size="small"
            startIcon={<AddIcon />}
            onClick={handleOpenCreate}
            fullWidth
          >
            Nova Avaliação
          </Button>
        </Box>
      </Box>

      {/* Conteúdo: Linha do Tempo */}
      <Box sx={{ p: 2.5, overflowY: 'auto', flex: 1 }}>
        {error !== null && (
          <Alert
            severity="error"
            action={
              <Button
                color="inherit"
                size="small"
                startIcon={<RefreshIcon />}
                onClick={() => { void handleRetry() }}
              >
                Tentar novamente
              </Button>
            }
            sx={{ mb: 2 }}
          >
            {error}
          </Alert>
        )}

        {loading ? (
          <Stack spacing={2}>
            {[1, 2, 3].map((key) => (
              <Card key={key} sx={{ p: 2, border: 1, borderColor: 'divider', boxShadow: 'none' }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1.5 }}>
                  <Skeleton variant="rounded" width={110} height={24} />
                  <Skeleton variant="text" width={80} />
                </Box>
                <Skeleton variant="text" width="60%" height={20} sx={{ mb: 1 }} />
                <Skeleton variant="rounded" width="100%" height={48} />
              </Card>
            ))}
          </Stack>
        ) : feedbacks.length === 0 ? (
          <EmptyState
            icon={<RateReviewOutlinedIcon sx={{ fontSize: 48 }} />}
            title="Nenhuma avaliação registrada"
            description="Este contato ainda não possui avaliações respondidas nesta organização."
            action={{
              label: 'Registrar Primeira Avaliação',
              icon: <AddIcon />,
              onClick: handleOpenCreate,
            }}
          />
        ) : (
          <Stack spacing={2}>
            {feedbacks.map((item) => {
              const chipProps = getScoreChipProps(item.score, item.surveyType)
              const formattedDate = new Date(item.respondedAt).toLocaleDateString('pt-BR', {
                day: '2-digit',
                month: 'short',
                year: 'numeric',
                hour: '2-digit',
                minute: '2-digit',
              })

              return (
                <Card
                  key={item.id}
                  sx={{
                    p: 2,
                    border: 1,
                    borderColor: 'divider',
                    boxShadow: 'none',
                    transition: 'all 0.2s',
                    '&:hover': {
                      borderColor: 'primary.light',
                      boxShadow: 1,
                    },
                  }}
                >
                  {/* Linha Superior: Nota e Ações */}
                  <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1.5 }}>
                    <Chip
                      label={chipProps.label}
                      color={chipProps.color}
                      size="small"
                      sx={{ fontWeight: 700 }}
                    />
                    <Stack direction="row" spacing={0.5}>
                      <Tooltip title="Editar avaliação">
                        <IconButton
                          size="small"
                          color="primary"
                          onClick={() => { handleOpenEdit(item) }}
                          aria-label={`Editar avaliação de ${item.surveyName}`}
                        >
                          <EditOutlinedIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Excluir avaliação">
                        <IconButton
                          size="small"
                          color="error"
                          onClick={() => { handleOpenDelete(item) }}
                          aria-label={`Excluir avaliação de ${item.surveyName}`}
                        >
                          <DeleteOutlinedIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </Stack>
                  </Box>

                  {/* Informações da Pesquisa e Canal */}
                  <Box sx={{ mb: 1 }}>
                    <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>
                      {item.surveyName}
                    </Typography>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mt: 0.5, flexWrap: 'wrap' }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, color: 'text.secondary' }}>
                        {getChannelIcon(item.channel)}
                        <Typography variant="caption" sx={{ textTransform: 'capitalize' }}>
                          {item.channel}
                        </Typography>
                      </Box>
                      <Divider orientation="vertical" flexItem sx={{ height: 12, my: 'auto' }} />
                      <Typography variant="caption" color="text.secondary">
                        {formattedDate}
                      </Typography>
                    </Box>
                  </Box>

                  {/* Comentário */}
                  {item.comment !== null && item.comment.trim().length > 0 ? (
                    <Box
                      sx={{
                        mt: 1.5,
                        p: 1.5,
                        bgcolor: 'background.default',
                        borderRadius: 1,
                        borderLeft: 3,
                        borderColor: 'primary.main',
                      }}
                    >
                      <Typography variant="body2" sx={{ fontStyle: 'italic', color: 'text.primary' }}>
                        &ldquo;{item.comment}&rdquo;
                      </Typography>
                    </Box>
                  ) : (
                    <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 1 }}>
                      Sem comentário adicional.
                    </Typography>
                  )}
                </Card>
              )
            })}
          </Stack>
        )}
      </Box>

      {/* Diálogos */}
      {contact !== null && (
        <FeedbackDialog
          open={dialogOpen}
          mode={dialogMode}
          contactId={contact.id}
          feedbackToEdit={feedbackToEdit}
          onClose={() => { setDialogOpen(false) }}
          onSuccess={handleDialogSuccess}
        />
      )}

      <DeleteFeedbackDialog
        open={deleteDialogOpen}
        feedbackItem={feedbackToDelete}
        onClose={() => { setDeleteDialogOpen(false) }}
        onSuccess={handleDeleteSuccess}
      />
    </Drawer>
  )
}
