import AddIcon from '@mui/icons-material/Add'
import ClearIcon from '@mui/icons-material/Clear'
import DeleteOutlinedIcon from '@mui/icons-material/DeleteOutlined'
import EditOutlinedIcon from '@mui/icons-material/EditOutlined'
import PeopleAltOutlinedIcon from '@mui/icons-material/PeopleAltOutlined'
import RefreshIcon from '@mui/icons-material/Refresh'
import SearchIcon from '@mui/icons-material/Search'
import {
  Alert,
  Button,
  Card,
  Chip,
  Container,
  IconButton,
  InputAdornment,
  Skeleton,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
  TextField,
  Tooltip,
} from '@mui/material'
import { useEffect, useState, type ChangeEvent } from 'react'
import { ContactDialog } from '../components/ContactDialog.tsx'
import { DeleteContactDialog } from '../components/DeleteContactDialog.tsx'
import { EmptyState } from '../components/EmptyState.tsx'
import { PageHeader } from '../components/PageHeader.tsx'
import { useCompany } from '../context/useCompany.ts'
import { getContacts } from '../services/contactService.ts'
import type { Contact } from '../types/contact.ts'

export function ContactsPage() {
  const { selectedCompanyId } = useCompany()
  const [contacts, setContacts] = useState<readonly Contact[]>([])
  const [total, setTotal] = useState(0)
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [searchInput, setSearchInput] = useState('')
  const [debouncedSearch, setDebouncedSearch] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // Modals state
  const [dialogOpen, setDialogOpen] = useState(false)
  const [dialogMode, setDialogMode] = useState<'create' | 'edit'>('create')
  const [selectedContact, setSelectedContact] = useState<Contact | null>(null)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [contactToDelete, setContactToDelete] = useState<Contact | null>(null)
  const [prevCompanyId, setPrevCompanyId] = useState(selectedCompanyId)

  // Reset pagination and search during render when company changes
  if (selectedCompanyId !== prevCompanyId) {
    setPrevCompanyId(selectedCompanyId)
    setPage(1)
    setSearchInput('')
    setDebouncedSearch('')
  }

  // Debounce search input
  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedSearch(searchInput)
      setPage(1)
    }, 300)

    return () => {
      clearTimeout(handler)
    }
  }, [searchInput])

  useEffect(() => {
    let active = true
    const controller = new AbortController()

    async function fetchContacts() {
      if (selectedCompanyId === null) {
        setLoading(false)
        return
      }

      setLoading(true)
      try {
        const response = await getContacts(page, pageSize, debouncedSearch, controller.signal)
        if (!active) {
          return
        }
        setContacts(response.items)
        setTotal(response.total)
        setError(null)
        setLoading(false)
      } catch (err: unknown) {
        if (!active || (err instanceof Error && err.name === 'AbortError')) {
          return
        }
        const message = err instanceof Error ? err.message : 'Falha ao carregar os contatos.'
        setError(message)
        setLoading(false)
      }
    }

    void fetchContacts()

    return () => {
      active = false
      controller.abort()
    }
  }, [selectedCompanyId, page, pageSize, debouncedSearch])

  const handleRetry = async () => {
    setLoading(true)
    setError(null)
    try {
      const response = await getContacts(page, pageSize, debouncedSearch)
      setContacts(response.items)
      setTotal(response.total)
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Falha ao carregar os contatos.'
      setError(message)
    } finally {
      setLoading(false)
    }
  }

  const handleSearchChange = (e: ChangeEvent<HTMLInputElement>) => {
    setSearchInput(e.target.value)
  }

  const handleClearSearch = () => {
    setSearchInput('')
  }

  const handlePageChange = (_: unknown, newPageIndex: number) => {
    setPage(newPageIndex + 1)
  }

  const handleRowsPerPageChange = (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const newPageSize = Number.parseInt(event.target.value, 10)
    setPageSize(newPageSize)
    setPage(1)
  }

  const handleOpenCreate = () => {
    setSelectedContact(null)
    setDialogMode('create')
    setDialogOpen(true)
  }

  const handleOpenEdit = (contact: Contact) => {
    setSelectedContact(contact)
    setDialogMode('edit')
    setDialogOpen(true)
  }

  const handleOpenDelete = (contact: Contact) => {
    setContactToDelete(contact)
    setDeleteDialogOpen(true)
  }

  const handleDialogSuccess = () => {
    void handleRetry()
  }

  const handleDeleteSuccess = () => {
    if (contacts.length === 1 && page > 1) {
      setPage((prev) => prev - 1)
    } else {
      void handleRetry()
    }
  }

  if (selectedCompanyId === null) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Alert severity="info">
          Selecione uma empresa no cabeçalho superior para gerenciar seus contatos.
        </Alert>
      </Container>
    )
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      {/* Cabeçalho da Página */}
      <PageHeader
        title="Gestão de Contatos"
        subtitle="Cadastro e busca ágil de clientes e alunos da organização."
        icon={<PeopleAltOutlinedIcon sx={{ color: 'primary.main', fontSize: 28 }} />}
        action={
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={handleOpenCreate}
            sx={{ fontWeight: 600 }}
          >
            Novo Contato
          </Button>
        }
      />

      {/* Barra de Filtros e Busca Textual com Debounce */}
      <Card sx={{ mb: 3, p: 2, border: 1, borderColor: 'divider', boxShadow: 'none' }}>
        <TextField
          placeholder="Buscar contato por nome ou e-mail..."
          value={searchInput}
          onChange={handleSearchChange}
          size="small"
          fullWidth
          slotProps={{
            htmlInput: {
              'aria-label': 'Buscar contatos por nome ou e-mail',
            },
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon color="action" fontSize="small" />
                </InputAdornment>
              ),
              endAdornment: searchInput.length > 0 ? (
                <InputAdornment position="end">
                  <IconButton
                    size="small"
                    onClick={handleClearSearch}
                    aria-label="Limpar termo de busca"
                  >
                    <ClearIcon fontSize="small" />
                  </IconButton>
                </InputAdornment>
              ) : null,
            },
          }}
        />
      </Card>

      {/* Estado de Erro */}
      {error !== null && (
        <Alert
          severity="error"
          action={
            <Button
              color="inherit"
              size="small"
              startIcon={<RefreshIcon />}
              onClick={() => void handleRetry()}
            >
              Tentar novamente
            </Button>
          }
          sx={{ mb: 3 }}
        >
          {error}
        </Alert>
      )}

      {/* Tabela de Contatos */}
      <Card sx={{ border: 1, borderColor: 'divider', boxShadow: 'none' }}>
        <TableContainer>
          <Table aria-label="Tabela de contatos cadastrados">
            <TableHead sx={{ bgcolor: 'background.default' }}>
              <TableRow>
                <TableCell sx={{ fontWeight: 700 }}>Nome</TableCell>
                <TableCell sx={{ fontWeight: 700 }}>E-mail</TableCell>
                <TableCell sx={{ fontWeight: 700 }}>Segmento / Plano</TableCell>
                <TableCell sx={{ fontWeight: 700 }}>Data de Cadastro</TableCell>
                <TableCell align="right" sx={{ fontWeight: 700 }}>Ações</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {loading ? (
                [1, 2, 3, 4, 5].map((key) => (
                  <TableRow key={key}>
                    <TableCell><Skeleton variant="text" width={160} /></TableCell>
                    <TableCell><Skeleton variant="text" width={200} /></TableCell>
                    <TableCell><Skeleton variant="rounded" width={90} height={24} /></TableCell>
                    <TableCell><Skeleton variant="text" width={100} /></TableCell>
                    <TableCell align="right"><Skeleton variant="rounded" width={80} height={32} sx={{ ml: 'auto' }} /></TableCell>
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
                              onClick: handleOpenCreate,
                            }
                          : undefined
                      }
                    />
                  </TableCell>
                </TableRow>
              ) : (
                contacts.map((contact) => (
                  <TableRow key={contact.id} hover>
                    <TableCell sx={{ fontWeight: 600 }}>{contact.name}</TableCell>
                    <TableCell sx={{ color: 'text.secondary' }}>{contact.email}</TableCell>
                    <TableCell>
                      <Chip
                        label={contact.segment}
                        size="small"
                        color={contact.segment.toLowerCase().includes('black') ? 'primary' : 'default'}
                        variant="outlined"
                      />
                    </TableCell>
                    <TableCell sx={{ color: 'text.secondary', fontSize: '0.875rem' }}>
                      {new Date(contact.createdAt).toLocaleDateString('pt-BR')}
                    </TableCell>
                    <TableCell align="right">
                      <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
                        <Tooltip title="Editar contato">
                          <IconButton
                            size="small"
                            color="primary"
                            onClick={() => handleOpenEdit(contact)}
                            aria-label={`Editar ${contact.name}`}
                          >
                            <EditOutlinedIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Excluir contato">
                          <IconButton
                            size="small"
                            color="error"
                            onClick={() => handleOpenDelete(contact)}
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

        <TablePagination
          rowsPerPageOptions={[5, 10, 20, 50]}
          component="div"
          count={total}
          rowsPerPage={pageSize}
          page={page - 1}
          onPageChange={handlePageChange}
          onRowsPerPageChange={handleRowsPerPageChange}
          labelRowsPerPage="Itens por página:"
          labelDisplayedRows={({ from, to, count }) => `${from}–${to} de ${count !== -1 ? count : `mais de ${to}`}`}
        />
      </Card>

      {/* Modais */}
      <ContactDialog
        open={dialogOpen}
        mode={dialogMode}
        contact={selectedContact}
        onClose={() => setDialogOpen(false)}
        onSuccess={handleDialogSuccess}
      />

      <DeleteContactDialog
        open={deleteDialogOpen}
        contact={contactToDelete}
        onClose={() => setDeleteDialogOpen(false)}
        onSuccess={handleDeleteSuccess}
      />
    </Container>
  )
}
