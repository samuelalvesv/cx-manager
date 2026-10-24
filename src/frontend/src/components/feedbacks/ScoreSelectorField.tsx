import {
  Box,
  Chip,
  Rating,
  ToggleButton,
  ToggleButtonGroup,
  Typography,
} from '@mui/material'
import { useState, type MouseEvent } from 'react'
import { getScoreOptionSuffix } from './feedbackFormatters.tsx'

export interface ScoreSelectorFieldProps {
  readonly surveyType: 'NPS' | 'CSAT'
  readonly score: number
  readonly disabled: boolean
  readonly onChange: (newScore: number) => void
}

function getNpsButtonColor(val: number): 'error' | 'warning' | 'success' {
  if (val >= 9) return 'success'
  if (val >= 7) return 'warning'
  return 'error'
}

export function ScoreSelectorField({
  surveyType,
  score,
  disabled,
  onChange,
}: ScoreSelectorFieldProps) {
  const [hoverScore, setHoverScore] = useState<number>(-1)

  if (surveyType === 'CSAT') {
    const displayedScore = hoverScore !== -1 ? hoverScore : score
    return (
      <Box
        sx={{
          p: 2,
          borderRadius: 1.5,
          border: 1,
          borderColor: 'divider',
          bgcolor: 'action.hover',
        }}
      >
        <Typography variant="caption" color="text.secondary" sx={{ display: 'block', fontWeight: 600, mb: 1 }}>
          Nota de Satisfação (CSAT: 1 a 5 estrelas)
        </Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', flexWrap: 'wrap', gap: 2 }}>
          <Rating
            name="feedback-csat-rating"
            value={score}
            max={5}
            size="large"
            disabled={disabled}
            onChange={(_, newValue) => {
              if (newValue !== null) {
                onChange(newValue)
              }
            }}
            onChangeActive={(_, newHover) => {
              setHoverScore(newHover)
            }}
            getLabelText={(val) => `${String(val)} — ${getScoreOptionSuffix(val, 'CSAT')}`}
          />
          <Chip
            label={`${String(displayedScore)}/5 • ${getScoreOptionSuffix(displayedScore, 'CSAT')}`}
            color={displayedScore >= 4 ? 'success' : displayedScore === 3 ? 'warning' : 'error'}
            size="small"
            sx={{ fontWeight: 700 }}
          />
        </Box>
      </Box>
    )
  }

  const npsScores = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10] as const

  const handleNpsToggle = (_: MouseEvent<HTMLElement>, newScore: number | null) => {
    if (newScore !== null) {
      onChange(newScore)
    }
  }

  return (
    <Box
      sx={{
        p: 2,
        borderRadius: 1.5,
        border: 1,
        borderColor: 'divider',
        bgcolor: 'action.hover',
      }}
    >
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 1, mb: 1.5 }}>
        <Typography variant="caption" color="text.secondary" sx={{ fontWeight: 600 }}>
          Nota de Recomendação (NPS: 0 a 10)
        </Typography>
        <Chip
          label={`${String(score)} • ${getScoreOptionSuffix(score, 'NPS')}`}
          color={getNpsButtonColor(score)}
          size="small"
          sx={{ fontWeight: 700 }}
        />
      </Box>

      <ToggleButtonGroup
        value={score}
        exclusive
        fullWidth
        size="small"
        disabled={disabled}
        onChange={handleNpsToggle}
        aria-label="Nota de recomendação de 0 a 10"
        sx={{
          display: 'grid',
          gridTemplateColumns: { xs: 'repeat(6, 1fr)', sm: 'repeat(11, 1fr)' },
          gap: 0.5,
          '& .MuiToggleButtonGroup-grouped': {
            border: 1,
            borderColor: 'divider',
            borderRadius: '6px !important',
            m: 0,
            py: 0.75,
            fontWeight: 700,
          },
        }}
      >
        {npsScores.map((val) => (
          <ToggleButton
            key={val}
            value={val}
            color={getNpsButtonColor(val)}
            aria-label={`Nota ${String(val)}`}
          >
            {val}
          </ToggleButton>
        ))}
      </ToggleButtonGroup>

      <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 1 }}>
        <Typography variant="caption" color="text.secondary">
          0 = Nada provável
        </Typography>
        <Typography variant="caption" color="text.secondary">
          10 = Extremamente provável
        </Typography>
      </Box>
    </Box>
  )
}
