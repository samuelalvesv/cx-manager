import {
  Alert,
  Box,
  DialogContentText,
  Skeleton,
  Stack,
  Typography,
} from '@mui/material'
import { useEffect, useState } from 'react'
import { deleteCompany, getCompanyImpact } from '../services/companyService.ts'
import type { Company, CompanyImpactSummary } from '../types/company.ts'
import { ConfirmDeleteDialog } from './ConfirmDeleteDialog.tsx'

export interface DeleteCompanyDialogProps {
  readonly open: boolean
  readonly company: Company | null
  readonly onClose: () => void
  readonly onSuccess: (deletedId: number) => void
}

export function DeleteCompanyDialog({
  open,
  company,
  onClose,
  onSuccess,
}: DeleteCompanyDialogProps) {
  const [impact, setImpact] = useState<CompanyImpactSummary | null>(null)
  const [loading, setLoading] = useState(false)
  const [fetchError, setFetchError] = useState<string | null>(null)

  useEffect(() => {
    let active = true
    const controller = new AbortController()

    if (open && company !== null) {
      async function loadImpact() {
        if (company === null) {
          return
        }
        setLoading(true)
        setFetchError(null)

        try {
          const data = await getCompanyImpact(company.id, controller.signal)
          if (!active) {
            return
          }
          setImpact(data)
          setLoading(false)
        } catch (err: unknown) {
          if (!active || (err instanceof Error && err.name === 'AbortError')) {
            return
          }
          const message = err instanceof Error ? err.message : 'Falha ao carregar impacto da empresa.'
          setFetchError(message)
          setLoading(false)
        }
      }

      void loadImpact()
    }

    return () => {
      active = false
      controller.abort()
    }
  }, [open, company])

  const handleConfirm = async () => {
    if (company === null) {
      return
    }
    await deleteCompany(company.id)
    onSuccess(company.id)
  }

  const handleDialogClose = () => {
    setImpact(null)
    setFetchError(null)
    onClose()
  }

  const hasItems =
    impact !== null &&
    (impact.activeContactsCount > 0 || impact.surveys.length > 0 || impact.totalResponsesCount > 0)

  return (
    <ConfirmDeleteDialog
      open={open}
      title="Desativar Empresa"
      confirmLabel="Desativar Empresa"
      submittingLabel="Desativando..."
      defaultErrorMessage="Falha ao desativar a empresa."
      maxWidth="sm"
      onClose={handleDialogClose}
      onConfirm={handleConfirm}
    >
      {loading ? (
        <Stack spacing={1.5} sx={{ py: 1 }}>
          <Skeleton variant="text" width="80%" height={24} />
          <Skeleton variant="rounded" height={100} />
        </Stack>
      ) : fetchError !== null ? (
        <Alert severity="error" sx={{ my: 1 }}>
          {fetchError}
        </Alert>
      ) : (
        <>
          <DialogContentText sx={{ mb: hasItems ? 1.5 : 1 }}>
            Tem certeza de que deseja desativar a empresa{' '}
            <strong>{company?.name ?? ''}</strong>?
          </DialogContentText>

          {hasItems ? (
            <Alert severity="warning" sx={{ mt: 1 }}>
              <Typography variant="body2" sx={{ fontWeight: 600, mb: 1 }}>
                Atenção: Ao desativar esta empresa, você perderá o acesso a toda a organização e a todos os itens vinculados:
              </Typography>
              <Box component="ul" sx={{ pl: 2.5, my: 0.5, typography: 'body2' }}>
                <li>
                  <strong>{impact.activeContactsCount}</strong>{' '}
                  {impact.activeContactsCount === 1 ? 'contato cadastrado' : 'contatos cadastrados'}
                </li>
                <li>
                  <strong>{impact.surveys.length}</strong>{' '}
                  {impact.surveys.length === 1 ? 'pesquisa ativa' : 'pesquisas ativas'} e suas{' '}
                  <strong>{impact.totalResponsesCount}</strong>{' '}
                  {impact.totalResponsesCount === 1 ? 'avaliação' : 'avaliações'}:
                  {impact.surveys.length > 0 && (
                    <Box component="ul" sx={{ pl: 2.5, my: 0.5 }}>
                      {impact.surveys.map((survey) => (
                        <li key={survey.id}>
                          <em>{survey.name}</em>: <strong>{survey.responsesCount}</strong>{' '}
                          {survey.responsesCount === 1 ? 'avaliação' : 'avaliações'}
                        </li>
                      ))}
                    </Box>
                  )}
                </li>
              </Box>
              <Typography variant="body2" sx={{ mt: 1, fontWeight: 500 }}>
                Todos os relatórios, gráficos e históricos desta organização ficarão inacessíveis.
              </Typography>
            </Alert>
          ) : (
            <DialogContentText variant="caption" color="text.secondary" sx={{ display: 'block', mt: 1 }}>
              Você perderá o acesso a esta organização e ao seu painel de controle.
            </DialogContentText>
          )}
        </>
      )}
    </ConfirmDeleteDialog>
  )
}
