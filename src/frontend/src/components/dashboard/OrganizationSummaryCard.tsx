import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import { Box, Card, CardContent, Divider, Typography } from '@mui/material'
import type { Company } from '../../types/company.ts'

export interface OrganizationSummaryCardProps {
  readonly company: Company | null
}

export function OrganizationSummaryCard({ company }: OrganizationSummaryCardProps) {
  return (
    <Card sx={{ mb: 3, border: 1, borderColor: 'divider', boxShadow: 'none' }}>
      <CardContent sx={{ p: { xs: 2, sm: 3 } }}>
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            flexWrap: 'wrap',
            gap: 1,
            mb: 1,
          }}
        >
          <Typography variant="overline" color="primary.main" sx={{ fontWeight: 700, letterSpacing: 1.2 }}>
            Painel de Satisfação
          </Typography>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
            <CheckCircleIcon color="success" fontSize="small" />
            <Typography variant="caption" color="text.secondary" sx={{ fontWeight: 600 }}>
              {company?.name ?? 'Organização ativa'}
            </Typography>
          </Box>
        </Box>
        <Typography
          variant="h5"
          component="h1"
          sx={{ fontWeight: 700, mb: 1, fontSize: { xs: '1.35rem', sm: '1.6rem', md: '1.85rem' } }}
        >
          Indicadores de Experiência e Satisfação
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Acompanhe os índices consolidados de recomendação (NPS) e satisfação (CSAT) da sua organização por período e pesquisa.
        </Typography>
      </CardContent>
      {company !== null && company.description.trim().length > 0 && (
        <>
          <Divider />
          <CardContent sx={{ bgcolor: 'action.hover', py: 1.75, px: { xs: 2, sm: 3 } }}>
            <Typography variant="body2" color="text.secondary">
              <strong>Sobre a organização:</strong> {company.description}
            </Typography>
          </CardContent>
        </>
      )}
    </Card>
  )
}
