import {
  useCallback,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { getSelectedCompanyId, setSelectedCompanyId } from '../services/apiClient.ts'
import { getCompanies } from '../services/companyService.ts'
import type { Company } from '../types/company.ts'
import { CompanyContext, type CompanyContextValue } from './CompanyContextTypes.ts'

export interface CompanyProviderProps {
  readonly children: ReactNode
}

function resolveInitialCompanyId(data: readonly Company[]): number | null {
  const storedId = getSelectedCompanyId()
  const matched = data.find((c) => c.id === storedId)
  if (matched !== undefined) {
    return matched.id
  }
  const firstCompany = data[0]
  if (firstCompany !== undefined) {
    setSelectedCompanyId(firstCompany.id)
    return firstCompany.id
  }
  return null
}

export function CompanyProvider({ children }: CompanyProviderProps) {
  const [companies, setCompanies] = useState<readonly Company[]>([])
  const [selectedCompanyId, setSelectedCompanyIdState] = useState<number | null>(() => getSelectedCompanyId())
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const refreshCompanies = useCallback(async (targetCompanyId?: number) => {
    setLoading(true)
    setError(null)
    try {
      const data = await getCompanies()
      setCompanies(data)
      if (targetCompanyId !== undefined) {
        setSelectedCompanyId(targetCompanyId)
        setSelectedCompanyIdState(targetCompanyId)
      } else {
        const resolvedId = resolveInitialCompanyId(data)
        setSelectedCompanyIdState(resolvedId)
      }
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Falha ao carregar as empresas.'
      setError(message)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    let active = true
    const controller = new AbortController()

    async function initialize() {
      try {
        const data = await getCompanies(controller.signal)
        if (!active) {
          return
        }
        setCompanies(data)
        const resolvedId = resolveInitialCompanyId(data)
        setSelectedCompanyIdState(resolvedId)
        setLoading(false)
      } catch (err: unknown) {
        if (!active || (err instanceof Error && err.name === 'AbortError')) {
          return
        }
        const message = err instanceof Error ? err.message : 'Falha ao carregar as empresas.'
        setError(message)
        setLoading(false)
      }
    }

    void initialize()

    return () => {
      active = false
      controller.abort()
    }
  }, [])

  const selectCompany = useCallback((id: number) => {
    setSelectedCompanyId(id)
    setSelectedCompanyIdState(id)
  }, [])

  const selectedCompany = useMemo(
    () => companies.find((c) => c.id === selectedCompanyId) ?? null,
    [companies, selectedCompanyId],
  )

  const value = useMemo<CompanyContextValue>(
    () => ({
      companies,
      selectedCompany,
      selectedCompanyId,
      loading,
      error,
      selectCompany,
      refreshCompanies,
    }),
    [companies, selectedCompany, selectedCompanyId, loading, error, selectCompany, refreshCompanies],
  )

  return <CompanyContext.Provider value={value}>{children}</CompanyContext.Provider>
}
