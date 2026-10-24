import { Avatar } from '@mui/material'

export interface ContactAvatarProps {
  readonly name: string
  readonly size?: number | undefined
}

function getInitials(name: string): string {
  const parts = name.trim().split(/\s+/).filter((p) => p.length > 0)
  if (parts.length === 0) {
    return '?'
  }
  const first = parts[0]?.[0] ?? ''
  if (parts.length === 1) {
    return first.toUpperCase()
  }
  const last = parts[parts.length - 1]?.[0] ?? ''
  return `${first}${last}`.toUpperCase()
}

export function ContactAvatar({ name, size = 34 }: ContactAvatarProps) {
  return (
    <Avatar
      sx={{
        width: size,
        height: size,
        fontSize: size <= 32 ? '0.75rem' : '0.825rem',
        fontWeight: 700,
        bgcolor: 'primary.main',
        color: 'primary.contrastText',
        flexShrink: 0,
      }}
    >
      {getInitials(name)}
    </Avatar>
  )
}
