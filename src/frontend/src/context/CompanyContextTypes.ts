import { createContext } from 'react'
import type { Company } from '../types/company.ts'

export interface CompanyContextValue {
  readonly companies: readonly Company[]
  readonly selectedCompany: Company | null
  readonly selectedCompanyId: number | null
  readonly loading: boolean
  readonly error: string | null
  readonly selectCompany: (id: number) => void
  readonly refreshCompanies: () => Promise<void>
}

export const CompanyContext = createContext<CompanyContextValue | null>(null)
