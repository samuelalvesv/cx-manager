import EmailOutlinedIcon from '@mui/icons-material/EmailOutlined'
import LinkOutlinedIcon from '@mui/icons-material/LinkOutlined'
import WhatsAppIcon from '@mui/icons-material/WhatsApp'

export const CHANNEL_OPTIONS = [
  { value: 'whatsapp', label: 'WhatsApp' },
  { value: 'email', label: 'E-mail' },
  { value: 'link', label: 'Página Web' },
] as const

export function getChannelIcon(channel: string) {
  const normalized = channel.toLowerCase()
  if (normalized.includes('whatsapp')) {
    return <WhatsAppIcon sx={{ fontSize: 16 }} />
  }
  if (normalized.includes('email') || normalized.includes('e-mail')) {
    return <EmailOutlinedIcon sx={{ fontSize: 16 }} />
  }
  return <LinkOutlinedIcon sx={{ fontSize: 16 }} />
}

export function formatChannelLabel(channel: string): string {
  const normalized = channel.trim().toLowerCase()
  if (normalized === 'whatsapp') return 'WhatsApp'
  if (normalized === 'email' || normalized === 'e-mail') return 'E-mail'
  if (normalized === 'link') return 'Página Web'
  return channel
}

export function getScoreOptionSuffix(val: number, surveyType: 'NPS' | 'CSAT'): string {
  if (surveyType === 'NPS') {
    if (val >= 9) return 'Promotor'
    if (val >= 7) return 'Neutro'
    return 'Detrator'
  }
  if (val === 5) return 'Muito Satisfeito'
  if (val === 4) return 'Satisfeito'
  if (val === 3) return 'Neutro'
  if (val === 2) return 'Insatisfeito'
  return 'Muito Insatisfeito'
}

export function getScoreChipProps(score: number, surveyType: string) {
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
