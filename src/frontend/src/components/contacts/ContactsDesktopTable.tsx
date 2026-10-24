import AddIcon from '@mui/icons-material/Add'
import DeleteOutlinedIcon from '@mui/icons-material/DeleteOutlined'
import EditOutlinedIcon from '@mui/icons-material/EditOutlined'
import PeopleAltOutlinedIcon from '@mui/icons-material/PeopleAltOutlined'
import RateReviewOutlinedIcon from '@mui/icons-material/RateReviewOutlined'
import {
  Box,
  Chip,
  IconButton,
  Skeleton,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TableSortLabel,
  Tooltip,
  Typography,
} from '@mui/material'
import type { ContactSortField, SortDirection } from '../../services/contactService.ts'
import type { Contact } from '../../types/contact.ts'
import { EmptyState } from '../EmptyState.tsx'
import { ContactAvatar } from './ContactAvatar.tsx'

export interface ContactsDesktopTableProps {
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

const CENTERED_SORT_LABEL_SX = { pl: '22px' } as const

export function ContactsDesktopTable({
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
}: ContactsDesktopTableProps) {
  return (
    <TableContainer sx={{ display: { xs: 'none', md: 'block' } }}>
      <Table aria-label="Tabela de contatos cadastrados" sx={{ tableLayout: 'fixed', width: '100%' }}>
        <TableHead sx={{ bgcolor: 'action.hover' }}>
          <TableRow>
            <TableCell
              align="center"
              sortDirection={sortBy === 'name' ? sortDir : false}
              sx={{ width: '27%', fontWeight: 700 }}
            >
              <TableSortLabel
                active={sortBy === 'name'}
                direction={sortBy === 'name' ? sortDir : 'asc'}
                onClick={() => onRequestSort('name')}
                sx={CENTERED_SORT_LABEL_SX}
              >
                Nome
              </TableSortLabel>
            </TableCell>
            <TableCell
              align="center"
              sortDirection={sortBy === 'email' ? sortDir : false}
              sx={{ width: '27%', fontWeight: 700 }}
            >
              <TableSortLabel
                active={sortBy === 'email'}
                direction={sortBy === 'email' ? sortDir : 'asc'}
                onClick={() => onRequestSort('email')}
                sx={CENTERED_SORT_LABEL_SX}
              >
                E-mail
              </TableSortLabel>
            </TableCell>
            <TableCell
              align="center"
              sortDirection={sortBy === 'segment' ? sortDir : false}
              sx={{ width: '18%', fontWeight: 700 }}
            >
              <TableSortLabel
                active={sortBy === 'segment'}
                direction={sortBy === 'segment' ? sortDir : 'asc'}
                onClick={() => onRequestSort('segment')}
                sx={CENTERED_SORT_LABEL_SX}
              >
                Segmento / Modalidade
              </TableSortLabel>
            </TableCell>
            <TableCell
              align="center"
              sortDirection={sortBy === 'createdAt' ? sortDir : false}
              sx={{ width: '14%', fontWeight: 700 }}
            >
              <TableSortLabel
                active={sortBy === 'createdAt'}
                direction={sortBy === 'createdAt' ? sortDir : 'asc'}
                onClick={() => onRequestSort('createdAt')}
                sx={CENTERED_SORT_LABEL_SX}
              >
                Data de Cadastro
              </TableSortLabel>
            </TableCell>
            <TableCell align="center" sx={{ width: '14%', fontWeight: 700 }}>
              Ações
            </TableCell>
          </TableRow>
        </TableHead>
        <TableBody
          sx={{
            opacity: loading && contacts.length > 0 ? 0.6 : 1,
            transition: 'opacity 150ms ease',
          }}
        >
          {loading && contacts.length === 0 ? (
            [1, 2, 3, 4, 5].map((key) => (
              <TableRow key={key}>
                <TableCell align="left">
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                    <Skeleton variant="circular" width={34} height={34} />
                    <Skeleton variant="text" width={150} />
                  </Box>
                </TableCell>
                <TableCell align="left">
                  <Skeleton variant="text" width={190} />
                </TableCell>
                <TableCell align="center">
                  <Skeleton variant="rounded" width={90} height={24} sx={{ mx: 'auto' }} />
                </TableCell>
                <TableCell align="center">
                  <Skeleton variant="text" width={90} sx={{ mx: 'auto' }} />
                </TableCell>
                <TableCell align="center">
                  <Skeleton variant="rounded" width={88} height={32} sx={{ mx: 'auto' }} />
                </TableCell>
              </TableRow>
            ))
          ) : contacts.length === 0 ? (
            <TableRow>
              <TableCell colSpan={5} sx={{ p: 0 }}>
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
              </TableCell>
            </TableRow>
          ) : (
            contacts.map((contact) => (
              <TableRow
                key={contact.id}
                hover
                sx={{ cursor: 'pointer' }}
                onClick={() => { onOpenHistory(contact) }}
              >
                <TableCell align="left" sx={{ fontWeight: 600, overflow: 'hidden' }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, minWidth: 0 }}>
                    <ContactAvatar name={contact.name} />
                    <Typography variant="body2" noWrap sx={{ fontWeight: 600 }}>
                      {contact.name}
                    </Typography>
                  </Box>
                </TableCell>
                <TableCell
                  align="left"
                  sx={{
                    color: 'text.secondary',
                    overflow: 'hidden',
                    textOverflow: 'ellipsis',
                    whiteSpace: 'nowrap',
                  }}
                >
                  {contact.email}
                </TableCell>
                <TableCell align="center">
                  {contact.segment !== null && contact.segment.length > 0 ? (
                    <Chip
                      label={contact.segment}
                      size="small"
                      color={contact.segment.toLowerCase().includes('black') ? 'primary' : 'default'}
                      variant="outlined"
                    />
                  ) : (
                    <Typography variant="body2" color="text.secondary">
                      —
                    </Typography>
                  )}
                </TableCell>
                <TableCell align="center" sx={{ color: 'text.secondary', fontSize: '0.875rem' }}>
                  {new Date(contact.createdAt).toLocaleDateString('pt-BR')}
                </TableCell>
                <TableCell align="center" onClick={(e) => { e.stopPropagation() }}>
                  <Stack direction="row" spacing={0.5} sx={{ justifyContent: 'center' }}>
                    <Tooltip title="Histórico de avaliações">
                      <IconButton
                        size="small"
                        color="info"
                        onClick={() => { onOpenHistory(contact) }}
                        aria-label={`Histórico de avaliações de ${contact.name}`}
                      >
                        <RateReviewOutlinedIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Editar contato">
                      <IconButton
                        size="small"
                        color="primary"
                        onClick={() => { onOpenEdit(contact) }}
                        aria-label={`Editar ${contact.name}`}
                      >
                        <EditOutlinedIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Excluir contato">
                      <IconButton
                        size="small"
                        color="error"
                        onClick={() => { onOpenDelete(contact) }}
                        aria-label={`Excluir ${contact.name}`}
                      >
                        <DeleteOutlinedIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  </Stack>
                </TableCell>
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>
    </TableContainer>
  )
}
