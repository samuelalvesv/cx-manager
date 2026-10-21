import { Box, Button, Typography } from '@mui/material'
import type { ReactNode } from 'react'

export interface EmptyStateProps {
  readonly icon: ReactNode
  readonly title: string
  readonly description?: ReactNode | undefined
  readonly action?: {
    readonly label: string
    readonly icon?: ReactNode | undefined
    readonly onClick: () => void
  } | undefined
}

export function EmptyState({
  icon,
  title,
  description,
  action,
}: EmptyStateProps) {
  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        textAlign: 'center',
        gap: 1,
        py: 6,
        px: 2,
      }}
    >
      <Box sx={{ color: 'text.disabled', display: 'flex', mb: 0.5 }}>
        {icon}
      </Box>
      <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
        {title}
      </Typography>
      {description && (
        <Typography variant="body2" color="text.secondary" sx={{ maxWidth: 460 }}>
          {description}
        </Typography>
      )}
      {action && (
        <Button
          variant="outlined"
          size="small"
          startIcon={action.icon}
          onClick={action.onClick}
          sx={{ mt: 1.5, fontWeight: 600 }}
        >
          {action.label}
        </Button>
      )}
    </Box>
  )
}
