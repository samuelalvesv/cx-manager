import DeleteOutlinedIcon from '@mui/icons-material/DeleteOutlined'
import EditOutlinedIcon from '@mui/icons-material/EditOutlined'
import {
  Box,
  Card,
  Chip,
  Divider,
  IconButton,
  Rating,
  Stack,
  Tooltip,
  Typography,
} from '@mui/material'
import type { FeedbackResponse } from '../../types/feedback.ts'
import { formatChannelLabel, getChannelIcon, getScoreChipProps } from './feedbackFormatters.tsx'

export interface FeedbackTimelineItemProps {
  readonly item: FeedbackResponse
  readonly isLast: boolean
  readonly onEdit: (item: FeedbackResponse) => void
  readonly onDelete: (item: FeedbackResponse) => void
}

export function FeedbackTimelineItem({
  item,
  isLast,
  onEdit,
  onDelete,
}: FeedbackTimelineItemProps) {
  const chipProps = getScoreChipProps(item.score, item.surveyType)
  const isCsat = item.surveyType.toUpperCase() === 'CSAT'
  const formattedDate = new Date(item.respondedAt).toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })

  return (
    <Box sx={{ display: 'flex', alignItems: 'stretch', gap: 2 }}>
      {/* Coluna do marcador e conector da linha do tempo */}
      <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', pt: 2 }}>
        <Box
          sx={{
            width: 12,
            height: 12,
            borderRadius: '50%',
            bgcolor: `${chipProps.color}.main`,
            flexShrink: 0,
          }}
        />
        {!isLast && (
          <Box
            sx={{
              width: 2,
              flexGrow: 1,
              bgcolor: 'divider',
              my: 0.75,
            }}
          />
        )}
      </Box>

      {/* Conteúdo do item na linha do tempo */}
      <Box sx={{ flexGrow: 1, pb: isLast ? 0 : 2, minWidth: 0 }}>
        <Card
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
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 1, mb: 1.5 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
              <Chip
                label={chipProps.label}
                color={chipProps.color}
                size="small"
                sx={{ fontWeight: 700 }}
              />
              {isCsat && (
                <Rating
                  name={`csat-timeline-${String(item.id)}`}
                  value={item.score}
                  max={5}
                  readOnly
                  size="small"
                />
              )}
            </Box>

            <Stack direction="row" spacing={0.5}>
              <Tooltip title="Editar avaliação">
                <IconButton
                  size="small"
                  color="primary"
                  onClick={() => onEdit(item)}
                  aria-label={`Editar avaliação de ${item.surveyName}`}
                >
                  <EditOutlinedIcon fontSize="small" />
                </IconButton>
              </Tooltip>
              <Tooltip title="Excluir avaliação">
                <IconButton
                  size="small"
                  color="error"
                  onClick={() => onDelete(item)}
                  aria-label={`Excluir avaliação de ${item.surveyName}`}
                >
                  <DeleteOutlinedIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            </Stack>
          </Box>

          <Box sx={{ mb: 1 }}>
            <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>
              {item.surveyName}
            </Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mt: 0.5, flexWrap: 'wrap' }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, color: 'text.secondary' }}>
                {getChannelIcon(item.channel)}
                <Typography variant="caption">
                  {formatChannelLabel(item.channel)}
                </Typography>
              </Box>
              <Divider orientation="vertical" flexItem sx={{ height: 12, my: 'auto' }} />
              <Typography variant="caption" color="text.secondary">
                {formattedDate}
              </Typography>
            </Box>
          </Box>

          {item.comment !== null && item.comment.trim().length > 0 ? (
            <Box
              sx={{
                mt: 1.5,
                p: 1.5,
                bgcolor: 'action.hover',
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
      </Box>
    </Box>
  )
}
