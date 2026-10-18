import { useContext } from 'react'
import { CompanyContext, type CompanyContextValue } from './CompanyContextTypes.ts'

export function useCompany(): CompanyContextValue {
  const context = useContext(CompanyContext)
  if (context === null) {
    throw new Error('useCompany deve ser utilizado dentro de um CompanyProvider.')
  }
  return context
}
