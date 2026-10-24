import { Box, Typography } from '@mui/material'
import type { ReactNode } from 'react'

export interface PageHeaderProps {
  readonly title: string
  readonly subtitle?: ReactNode | undefined
  readonly icon?: ReactNode | undefined
  readonly badges?: ReactNode | undefined
  readonly action?: ReactNode | undefined
}

export function PageHeader({
  title,
  subtitle,
  icon,
  badges,
  action,
}: PageHeaderProps) {
  return (
    <Box
      sx={{
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'flex-start',
        flexWrap: 'wrap',
        gap: 2,
        mb: 3,
      }}
    >
      <Box sx={{ minWidth: 0, flex: '1 1 260px' }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
          {icon}
          <Typography variant="h5" component="h1" sx={{ fontWeight: 700 }}>
            {title}
          </Typography>
        </Box>
        {subtitle && (
          <Typography variant="body2" color="text.secondary">
            {subtitle}
          </Typography>
        )}
        {badges && <Box sx={{ mt: 1.5 }}>{badges}</Box>}
      </Box>
      {action && (
        <Box
          sx={{
            width: { xs: '100%', sm: 'auto' },
            '& > .MuiButton-root': {
              width: { xs: '100%', sm: 'auto' },
            },
          }}
        >
          {action}
        </Box>
      )}
    </Box>
  )
}
