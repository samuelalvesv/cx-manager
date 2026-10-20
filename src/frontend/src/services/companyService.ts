import type { Company, CreateCompanyRequest, UpdateCompanyRequest } from '../types/company.ts'
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

function isVoid(value: unknown): value is void {
  return value === null || value === undefined
}

export async function getCompanies(signal?: AbortSignal): Promise<readonly Company[]> {
  const options = {
    skipCompanyId: true,
    ...(signal !== undefined ? { signal } : {}),
  }
  return await apiFetch<readonly Company[]>('/api/companies', isCompanyList, options)
}

export async function getCompanyById(id: number, signal?: AbortSignal): Promise<Company> {
  const options = {
    skipCompanyId: true,
    ...(signal !== undefined ? { signal } : {}),
  }
  return await apiFetch<Company>(`/api/companies/${id}`, isCompany, options)
}

export async function createCompany(
  request: CreateCompanyRequest,
  signal?: AbortSignal,
): Promise<Company> {
  const options = {
    method: 'POST',
    body: JSON.stringify(request),
    skipCompanyId: true,
    ...(signal !== undefined ? { signal } : {}),
  }
  return await apiFetch<Company>('/api/companies', isCompany, options)
}

export async function updateCompany(
  id: number,
  request: UpdateCompanyRequest,
  signal?: AbortSignal,
): Promise<Company> {
  const options = {
    method: 'PUT',
    body: JSON.stringify(request),
    skipCompanyId: true,
    ...(signal !== undefined ? { signal } : {}),
  }
  return await apiFetch<Company>(`/api/companies/${id}`, isCompany, options)
}

export async function deleteCompany(
  id: number,
  signal?: AbortSignal,
): Promise<void> {
  const options = {
    method: 'DELETE',
    skipCompanyId: true,
    ...(signal !== undefined ? { signal } : {}),
  }
  return await apiFetch<void>(`/api/companies/${id}`, isVoid, options)
}
