import AddIcon from '@mui/icons-material/Add'
import CloseIcon from '@mui/icons-material/Close'
import RateReviewOutlinedIcon from '@mui/icons-material/RateReviewOutlined'
import RefreshIcon from '@mui/icons-material/Refresh'
import {
  Alert,
  Box,
  Button,
  Card,
  Chip,
  Drawer,
  IconButton,
  Skeleton,
  Stack,
  Typography,
} from '@mui/material'
import { useEffect, useState } from 'react'
import { getContactFeedbacks } from '../services/feedbackService.ts'
import type { Contact } from '../types/contact.ts'
import type { FeedbackResponse } from '../types/feedback.ts'
import { ContactAvatar } from './contacts/ContactAvatar.tsx'
import { DeleteFeedbackDialog } from './DeleteFeedbackDialog.tsx'
import { EmptyState } from './EmptyState.tsx'
import { FeedbackDialog } from './FeedbackDialog.tsx'
import { FeedbackTimelineItem } from './feedbacks/FeedbackTimelineItem.tsx'

export interface ContactFeedbacksDrawerProps {
  readonly open: boolean
  readonly contact: Contact | null
  readonly onClose: () => void
}

export function ContactFeedbacksDrawer({
  open,
  contact,
  onClose,
}: ContactFeedbacksDrawerProps) {
  const [feedbacks, setFeedbacks] = useState<readonly FeedbackResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

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
      {/* Cabeçalho da Gaveta com Avatar */}
      <Box sx={{ p: 2.5, bgcolor: 'action.hover', borderBottom: 1, borderColor: 'divider' }}>
        <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', mb: 1 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
            <ContactAvatar name={contact?.name ?? 'Contato'} size={42} />
            <Box>
              <Typography variant="h6" sx={{ fontWeight: 700, lineHeight: 1.2 }}>
                Histórico de Avaliações
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {contact?.name ?? 'Contato'}
              </Typography>
            </Box>
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
          {contact?.segment !== null && contact?.segment !== undefined && (
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

      {/* Conteúdo: Linha do Tempo Cronológica (@mui/lab/Timeline) */}
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
          <Stack spacing={0}>
            {feedbacks.map((item, idx) => (
              <FeedbackTimelineItem
                key={item.id}
                item={item}
                isLast={idx === feedbacks.length - 1}
                onEdit={handleOpenEdit}
                onDelete={handleOpenDelete}
              />
            ))}
          </Stack>
        )}
      </Box>

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
