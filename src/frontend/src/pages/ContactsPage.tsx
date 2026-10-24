import AddIcon from '@mui/icons-material/Add'
import ClearIcon from '@mui/icons-material/Clear'
import PeopleAltOutlinedIcon from '@mui/icons-material/PeopleAltOutlined'
import RefreshIcon from '@mui/icons-material/Refresh'
import SearchIcon from '@mui/icons-material/Search'
import {
  Alert,
  Button,
  Card,
  Container,
  Divider,
  IconButton,
  InputAdornment,
  LinearProgress,
  TablePagination,
  TextField,
} from '@mui/material'
import { useEffect, useState, type ChangeEvent } from 'react'
import { ContactDialog } from '../components/ContactDialog.tsx'
import { ContactFeedbacksDrawer } from '../components/ContactFeedbacksDrawer.tsx'
import { ContactsDesktopTable } from '../components/contacts/ContactsDesktopTable.tsx'
import { ContactsMobileList } from '../components/contacts/ContactsMobileList.tsx'
import { DeleteContactDialog } from '../components/DeleteContactDialog.tsx'
import { PageHeader } from '../components/PageHeader.tsx'
import { useCompany } from '../context/useCompany.ts'
import { getContacts, type ContactSortField, type SortDirection } from '../services/contactService.ts'
import type { Contact } from '../types/contact.ts'

export function ContactsPage() {
  const { selectedCompanyId } = useCompany()
  const [contacts, setContacts] = useState<readonly Contact[]>([])
  const [total, setTotal] = useState(0)
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [sortBy, setSortBy] = useState<ContactSortField>('name')
  const [sortDir, setSortDir] = useState<SortDirection>('asc')
  const [searchInput, setSearchInput] = useState('')
  const [debouncedSearch, setDebouncedSearch] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [dialogOpen, setDialogOpen] = useState(false)
  const [dialogMode, setDialogMode] = useState<'create' | 'edit'>('create')
  const [selectedContact, setSelectedContact] = useState<Contact | null>(null)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [contactToDelete, setContactToDelete] = useState<Contact | null>(null)
  const [historyDrawerOpen, setHistoryDrawerOpen] = useState(false)
  const [contactForHistory, setContactForHistory] = useState<Contact | null>(null)
  const [prevCompanyId, setPrevCompanyId] = useState(selectedCompanyId)

  if (selectedCompanyId !== prevCompanyId) {
    setPrevCompanyId(selectedCompanyId)
    setPage(1)
    setSortBy('name')
    setSortDir('asc')
    setSearchInput('')
    setDebouncedSearch('')
  }

  useEffect(() => {
    if (searchInput === debouncedSearch) {
      return undefined
    }
    const handler = setTimeout(() => {
      setDebouncedSearch(searchInput)
      setPage(1)
    }, 300)

    return () => {
      clearTimeout(handler)
    }
  }, [searchInput, debouncedSearch])

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
        const response = await getContacts(page, pageSize, debouncedSearch, sortBy, sortDir, controller.signal)
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
  }, [selectedCompanyId, page, pageSize, debouncedSearch, sortBy, sortDir])

  const handleRetry = async () => {
    setLoading(true)
    setError(null)
    try {
      const response = await getContacts(page, pageSize, debouncedSearch, sortBy, sortDir)
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

  const handleRequestSort = (field: ContactSortField) => {
    if (sortBy === field) {
      setSortDir((prev) => (prev === 'asc' ? 'desc' : 'asc'))
    } else {
      setSortBy(field)
      setSortDir('asc')
    }
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

  const handleOpenHistory = (contact: Contact) => {
    setContactForHistory(contact)
    setHistoryDrawerOpen(true)
  }

  if (selectedCompanyId === null) {
    return (
      <Container maxWidth="lg" sx={{ py: { xs: 2.5, sm: 3, md: 4 } }}>
        <Alert severity="info">
          Selecione uma empresa no cabeçalho superior para gerenciar seus contatos.
        </Alert>
      </Container>
    )
  }

  return (
    <Container maxWidth="lg" sx={{ py: { xs: 2.5, sm: 3, md: 4 } }}>
      <PageHeader
        title="Gestão de Contatos"
        subtitle="Cadastro, segmentação e histórico de avaliações dos contatos da organização."
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

      <Card sx={{ position: 'relative', border: 1, borderColor: 'divider', boxShadow: 'none' }}>
        {loading && contacts.length > 0 && (
          <LinearProgress sx={{ position: 'absolute', top: 0, left: 0, right: 0, zIndex: 2 }} />
        )}

        <ContactsMobileList
          contacts={contacts}
          loading={loading}
          debouncedSearch={debouncedSearch}
          sortBy={sortBy}
          sortDir={sortDir}
          onRequestSort={handleRequestSort}
          onOpenCreate={handleOpenCreate}
          onOpenEdit={handleOpenEdit}
          onOpenDelete={handleOpenDelete}
          onOpenHistory={handleOpenHistory}
        />

        <ContactsDesktopTable
          contacts={contacts}
          loading={loading}
          debouncedSearch={debouncedSearch}
          sortBy={sortBy}
          sortDir={sortDir}
          onRequestSort={handleRequestSort}
          onOpenCreate={handleOpenCreate}
          onOpenEdit={handleOpenEdit}
          onOpenDelete={handleOpenDelete}
          onOpenHistory={handleOpenHistory}
        />

        <Divider />

        <TablePagination
          rowsPerPageOptions={[5, 10, 20, 50]}
          component="div"
          count={total}
          rowsPerPage={pageSize}
          page={page - 1}
          onPageChange={handlePageChange}
          onRowsPerPageChange={handleRowsPerPageChange}
          sx={{
            '.MuiTablePagination-toolbar': {
              flexWrap: 'wrap',
              justifyContent: { xs: 'center', sm: 'flex-end' },
              gap: 1,
              px: { xs: 1, sm: 2 },
              py: { xs: 1, sm: 0 },
            },
            '.MuiTablePagination-spacer': {
              display: { xs: 'none', sm: 'block' },
            },
          }}
        />
      </Card>

      <ContactDialog
        open={dialogOpen}
        mode={dialogMode}
        contact={selectedContact}
        onClose={() => { setDialogOpen(false) }}
        onSuccess={handleDialogSuccess}
      />

      <DeleteContactDialog
        open={deleteDialogOpen}
        contact={contactToDelete}
        onClose={() => { setDeleteDialogOpen(false) }}
        onSuccess={handleDeleteSuccess}
      />

      <ContactFeedbacksDrawer
        open={historyDrawerOpen}
        contact={contactForHistory}
        onClose={() => { setHistoryDrawerOpen(false) }}
      />
    </Container>
  )
}
