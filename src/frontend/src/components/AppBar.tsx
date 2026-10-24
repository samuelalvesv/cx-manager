import DarkModeOutlinedIcon from '@mui/icons-material/DarkModeOutlined'
import InsightsIcon from '@mui/icons-material/Insights'
import LightModeOutlinedIcon from '@mui/icons-material/LightModeOutlined'
import PeopleAltOutlinedIcon from '@mui/icons-material/PeopleAltOutlined'
import PollOutlinedIcon from '@mui/icons-material/PollOutlined'
import {
  AppBar as MuiAppBar,
  Box,
  ButtonBase,
  Container,
  IconButton,
  Tab,
  Tabs,
  Tooltip,
  Typography,
} from '@mui/material'
import { useColorScheme } from '@mui/material/styles'
import { CompanySelectorBar } from './CompanySelectorBar.tsx'

export type ViewType = 'organization' | 'surveys' | 'contacts'

export interface AppBarProps {
  readonly currentView: ViewType
  readonly onViewChange: (view: ViewType) => void
}

export function AppBar({ currentView, onViewChange }: AppBarProps) {
  const { mode, systemMode, setMode } = useColorScheme()

  const effectiveMode = mode === 'system' ? systemMode : mode
  const isDark = effectiveMode === 'dark'

  const handleToggleColorMode = () => {
    setMode(isDark ? 'light' : 'dark')
  }

  const themeToggleLabel = isDark ? 'Ativar tema claro' : 'Ativar tema escuro'

  const brandButton = (
    <ButtonBase
      onClick={() => onViewChange('organization')}
      aria-label="Ir para a página inicial"
      sx={{
        display: 'flex',
        alignItems: 'center',
        gap: 1.25,
        minWidth: 0,
        textAlign: 'left',
        borderRadius: 1.5,
        px: 1,
        py: 0.5,
        mx: -1,
        transition: 'background-color 150ms ease',
        '&:hover': {
          bgcolor: 'action.hover',
        },
      }}
    >
      <InsightsIcon sx={{ color: 'primary.main', fontSize: 28, flexShrink: 0 }} />
      <Box sx={{ minWidth: 0 }}>
        <Typography variant="h6" component="div" sx={{ fontWeight: 700, lineHeight: 1.15 }}>
          CxManager
        </Typography>
        <Typography
          variant="caption"
          color="text.secondary"
          sx={{ display: 'block', whiteSpace: 'nowrap', lineHeight: 1.2 }}
        >
          Gestão de Experiência do Cliente
        </Typography>
      </Box>
    </ButtonBase>
  )

  const themeToggleButton = (
    <Tooltip title={themeToggleLabel}>
      <IconButton
        color="inherit"
        onClick={handleToggleColorMode}
        aria-label={themeToggleLabel}
        size="small"
        sx={{ border: 1, borderColor: 'divider' }}
      >
        {isDark ? <LightModeOutlinedIcon fontSize="small" /> : <DarkModeOutlinedIcon fontSize="small" />}
      </IconButton>
    </Tooltip>
  )

  const navigationTabs = (
    <Tabs
      value={currentView}
      onChange={(_, value: ViewType) => onViewChange(value)}
      textColor="primary"
      indicatorColor="primary"
      variant="fullWidth"
      aria-label="Navegação principal do sistema"
      sx={{
        minHeight: { xs: 48, md: 64 },
        '& .MuiTab-root': {
          minHeight: { xs: 48, md: 64 },
          minWidth: { xs: 0, md: 110 },
          px: { xs: 1, sm: 2 },
          textTransform: 'none',
          fontWeight: 600,
          fontSize: { xs: '0.825rem', sm: '0.875rem' },
        },
      }}
    >
      <Tab
        icon={<InsightsIcon fontSize="small" />}
        iconPosition="start"
        label="Visão Geral"
        value="organization"
      />
      <Tab
        icon={<PeopleAltOutlinedIcon fontSize="small" />}
        iconPosition="start"
        label="Contatos"
        value="contacts"
      />
      <Tab
        icon={<PollOutlinedIcon fontSize="small" />}
        iconPosition="start"
        label="Pesquisas"
        value="surveys"
      />
    </Tabs>
  )

  return (
    <>
      {/* Topo Mobile/Tablet (< md): Rola naturalmente com a página e só aparece no topo */}
      <Box
        sx={{
          display: { xs: 'block', md: 'none' },
          bgcolor: 'background.paper',
          color: 'text.primary',
          borderBottom: 1,
          borderColor: 'divider',
        }}
      >
        <Container maxWidth="lg">
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5, pt: 1.5, pb: 1.5 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 1.5 }}>
              {brandButton}
              {themeToggleButton}
            </Box>
            <Box sx={{ display: 'flex', alignItems: 'center', width: '100%', minWidth: 0 }}>
              <CompanySelectorBar />
            </Box>
          </Box>
        </Container>
      </Box>

      {/* Barra Fixa (Sticky): Em mobile fixa apenas as abas (48px); em desktop fixa a barra completa (64px) */}
      <MuiAppBar position="sticky" color="inherit">
        <Container maxWidth="lg" sx={{ display: { xs: 'none', md: 'block' } }}>
          <Box
            sx={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
              gap: 2,
              minHeight: 64,
            }}
          >
            {brandButton}
            <Box sx={{ width: 'auto' }}>{navigationTabs}</Box>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, minWidth: 280 }}>
              <CompanySelectorBar />
              {themeToggleButton}
            </Box>
          </Box>
        </Container>

        <Container
          maxWidth="lg"
          disableGutters
          sx={{ display: { xs: 'block', md: 'none' }, px: { xs: 0.5, sm: 2 } }}
        >
          {navigationTabs}
        </Container>
      </MuiAppBar>
    </>
  )
}
