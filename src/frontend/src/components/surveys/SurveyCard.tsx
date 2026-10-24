import DeleteOutlinedIcon from '@mui/icons-material/DeleteOutlined'
import EditOutlinedIcon from '@mui/icons-material/EditOutlined'
import StarBorderOutlinedIcon from '@mui/icons-material/StarBorderOutlined'
import TrendingUpIcon from '@mui/icons-material/TrendingUp'
import {
  Box,
  Button,
  Card,
  CardActions,
  CardContent,
  Chip,
  Tooltip,
  Typography,
} from '@mui/material'
import type { Survey } from '../../types/survey.ts'

export interface SurveyCardProps {
  readonly survey: Survey
  readonly onEdit: (survey: Survey) => void
  readonly onDelete: (survey: Survey) => void
}

export function SurveyCard({ survey, onEdit, onDelete }: SurveyCardProps) {
  const isNps = survey.type === 'NPS'
  const formattedDate = new Date(survey.createdAt).toLocaleDateString('pt-BR')

  return (
    <Card sx={{ height: '100%', display: 'flex', flexDirection: 'column', border: 1, borderColor: 'divider', boxShadow: 'none' }}>
      <CardContent sx={{ flexGrow: 1, p: 2.5 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 1, mb: 1.5 }}>
          <Chip
            icon={isNps ? <TrendingUpIcon fontSize="small" /> : <StarBorderOutlinedIcon fontSize="small" />}
            label={isNps ? 'Recomendação • NPS (0–10)' : 'Satisfação • CSAT (1–5)'}
            color={isNps ? 'primary' : 'secondary'}
            size="small"
            sx={{ fontWeight: 600 }}
          />
          <Chip
            label="Ativa"
            color="success"
            variant="outlined"
            size="small"
            sx={{ height: 22, fontSize: '0.72rem', fontWeight: 600 }}
          />
        </Box>

        <Typography variant="h6" component="h2" sx={{ fontWeight: 600, mb: 1, fontSize: '1.05rem', lineHeight: 1.3 }}>
          {survey.name}
        </Typography>

        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 1, mt: 1.5 }}>
          <Chip
            label={`${survey.responsesCount} ${survey.responsesCount === 1 ? 'avaliação' : 'avaliações'}`}
            size="small"
            variant="outlined"
            sx={{ fontWeight: 500, fontSize: '0.75rem' }}
          />
          <Typography variant="caption" color="text.secondary">
            Criada em: {formattedDate}
          </Typography>
        </Box>
      </CardContent>

      <CardActions sx={{ px: 2, py: 1.5, borderTop: 1, borderColor: 'divider', justifyContent: 'flex-end', gap: 1 }}>
        <Tooltip title="Editar nome da pesquisa">
          <Button
            size="small"
            startIcon={<EditOutlinedIcon />}
            onClick={() => onEdit(survey)}
          >
            Editar
          </Button>
        </Tooltip>
        <Tooltip title="Desativar pesquisa">
          <Button
            size="small"
            color="error"
            startIcon={<DeleteOutlinedIcon />}
            onClick={() => onDelete(survey)}
          >
            Desativar
          </Button>
        </Tooltip>
      </CardActions>
    </Card>
  )
}
