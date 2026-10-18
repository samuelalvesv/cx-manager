import type { Company } from '../types/company.ts'
import { apiFetch } from './apiClient.ts'

export function isCompany(value: unknown): value is Company {
  if (typeof value !== 'object' || value === null) {
    return false
  }

  const candidate = value as { readonly [K in keyof Company]?: unknown }

  return (
    typeof candidate.id === 'number' &&
    typeof candidate.name === 'string' &&
    typeof candidate.description === 'string' &&
    typeof candidate.createdAt === 'string'
  )
}

export function isCompanyList(value: unknown): value is readonly Company[] {
  return Array.isArray(value) && value.every(isCompany)
}

export async function getCompanies(signal?: AbortSignal): Promise<readonly Company[]> {
  const options = signal !== undefined ? { signal, skipCompanyId: true } : { skipCompanyId: true }
  return await apiFetch<readonly Company[]>('/api/companies', isCompanyList, options)
}
