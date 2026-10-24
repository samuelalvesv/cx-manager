import AddIcon from '@mui/icons-material/Add'
import ArrowDownwardIcon from '@mui/icons-material/ArrowDownward'
import ArrowUpwardIcon from '@mui/icons-material/ArrowUpward'
import DeleteOutlinedIcon from '@mui/icons-material/DeleteOutlined'
import EditOutlinedIcon from '@mui/icons-material/EditOutlined'
import PeopleAltOutlinedIcon from '@mui/icons-material/PeopleAltOutlined'
import RateReviewOutlinedIcon from '@mui/icons-material/RateReviewOutlined'
import {
  Box,
  Button,
  Chip,
  Divider,
  FormControl,
  IconButton,
  InputLabel,
  MenuItem,
  Select,
  Skeleton,
  Stack,
  Tooltip,
  Typography,
  type SelectChangeEvent,
} from '@mui/material'
import type { ContactSortField, SortDirection } from '../../services/contactService.ts'
import type { Contact } from '../../types/contact.ts'
import { EmptyState } from '../EmptyState.tsx'
import { ContactAvatar } from './ContactAvatar.tsx'

export interface ContactsMobileListProps {
  readonly contacts: readonly Contact[]
  readonly loading: boolean
  readonly debouncedSearch: string
  readonly sortBy: ContactSortField
  readonly sortDir: SortDirection
  readonly onRequestSort: (field: ContactSortField) => void
  readonly onOpenCreate: () => void
  readonly onOpenEdit: (contact: Contact) => void
  readonly onOpenDelete: (contact: Contact) => void
  readonly onOpenHistory: (contact: Contact) => void
}

export function ContactsMobileList({
  contacts,
  loading,
  debouncedSearch,
  sortBy,
  sortDir,
  onRequestSort,
  onOpenCreate,
  onOpenEdit,
  onOpenDelete,
  onOpenHistory,
}: ContactsMobileListProps) {
  const handleFieldChange = (event: SelectChangeEvent<ContactSortField>) => {
    const field = event.target.value
    if (field !== sortBy) {
      onRequestSort(field)
    }
  }

  return (
    <Box sx={{ display: { xs: 'block', md: 'none' } }}>
      {contacts.length > 0 && (
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            gap: 1.5,
            p: 1.5,
            bgcolor: 'action.hover',
            borderBottom: 1,
            borderColor: 'divider',
          }}
        >
          <FormControl size="small" sx={{ flexGrow: 1 }}>
            <InputLabel id="mobile-contact-sort-label">Ordenar por</InputLabel>
            <Select<ContactSortField>
              labelId="mobile-contact-sort-label"
              value={sortBy}
              label="Ordenar por"
              onChange={handleFieldChange}
            >
              <MenuItem value="name">Nome</MenuItem>
              <MenuItem value="email">E-mail</MenuItem>
              <MenuItem value="segment">Segmento / Modalidade</MenuItem>
              <MenuItem value="createdAt">Data de Cadastro</MenuItem>
            </Select>
          </FormControl>

          <Tooltip title={sortDir === 'asc' ? 'Ordem crescente' : 'Ordem decrescente'}>
            <IconButton
              size="small"
              color="primary"
              onClick={() => onRequestSort(sortBy)}
              aria-label="Alternar direção da ordenação"
              sx={{ border: 1, borderColor: 'divider' }}
            >
              {sortDir === 'asc' ? <ArrowUpwardIcon fontSize="small" /> : <ArrowDownwardIcon fontSize="small" />}
            </IconButton>
          </Tooltip>
        </Box>
      )}

      {loading && contacts.length === 0 ? (
        <Stack divider={<Divider />}>
          {[1, 2, 3, 4].map((key) => (
            <Box key={key} sx={{ p: 2 }}>
              <Skeleton variant="text" width="55%" height={24} />
              <Skeleton variant="text" width="75%" height={20} sx={{ mb: 1 }} />
              <Skeleton variant="rounded" width={120} height={28} />
            </Box>
          ))}
        </Stack>
      ) : contacts.length === 0 ? (
        <EmptyState
          icon={<PeopleAltOutlinedIcon sx={{ fontSize: 48 }} />}
          title="Nenhum contato encontrado"
          description={
            debouncedSearch.length > 0
              ? `Nenhum resultado corresponde à busca por "${debouncedSearch}". Ajuste os termos pesquisados.`
              : 'Esta organização ainda não possui contatos cadastrados.'
          }
          action={
            debouncedSearch.length === 0
              ? {
                  label: 'Cadastrar Primeiro Contato',
                  icon: <AddIcon />,
                  onClick: onOpenCreate,
                }
              : undefined
          }
        />
      ) : (
        <Stack
          divider={<Divider />}
          sx={{
            opacity: loading ? 0.6 : 1,
            transition: 'opacity 150ms ease',
          }}
        >
          {contacts.map((contact) => {
            const formattedDate = new Date(contact.createdAt).toLocaleDateString('pt-BR')
            return (
              <Box key={contact.id} sx={{ p: 2 }}>
                <Box
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                    flexWrap: 'wrap',
                    gap: 1,
                    mb: 0.75,
                  }}
                >
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.25 }}>
                    <ContactAvatar name={contact.name} size={32} />
                    <Typography variant="subtitle1" sx={{ fontWeight: 700, lineHeight: 1.3 }}>
                      {contact.name}
                    </Typography>
                  </Box>
                  {contact.segment !== null && contact.segment.length > 0 && (
                    <Chip
                      label={contact.segment}
                      size="small"
                      color={contact.segment.toLowerCase().includes('black') ? 'primary' : 'default'}
                      variant="outlined"
                    />
                  )}
                </Box>

                <Typography variant="body2" color="text.secondary" sx={{ wordBreak: 'break-word', mb: 0.5 }}>
                  {contact.email}
                </Typography>

                <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 1.5 }}>
                  Cadastrado em: {formattedDate}
                </Typography>

                <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap' }} useFlexGap>
                  <Button
                    size="small"
                    variant="outlined"
                    color="primary"
                    startIcon={<RateReviewOutlinedIcon fontSize="small" />}
                    onClick={() => { onOpenHistory(contact) }}
                  >
                    Avaliações
                  </Button>
                  <Button
                    size="small"
                    color="inherit"
                    startIcon={<EditOutlinedIcon fontSize="small" />}
                    onClick={() => { onOpenEdit(contact) }}
                  >
                    Editar
                  </Button>
                  <Button
                    size="small"
                    color="error"
                    startIcon={<DeleteOutlinedIcon fontSize="small" />}
                    onClick={() => { onOpenDelete(contact) }}
                  >
                    Excluir
                  </Button>
                </Stack>
              </Box>
            )
          })}
        </Stack>
      )}
    </Box>
  )
}
