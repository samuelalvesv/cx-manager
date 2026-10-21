import type { Contact, CreateContactRequest, PagedContactsResponse, UpdateContactRequest } from '../types/contact.ts'
import { apiFetch } from './apiClient.ts'

export function isContact(value: unknown): value is Contact {
  if (typeof value !== 'object' || value === null) {
    return false
  }

  const candidate = value as { readonly [K in keyof Contact]?: unknown }

  return (
    typeof candidate.id === 'number' &&
    typeof candidate.name === 'string' &&
    typeof candidate.email === 'string' &&
    typeof candidate.segment === 'string' &&
    typeof candidate.createdAt === 'string'
  )
}

export function isPagedContactsResponse(value: unknown): value is PagedContactsResponse {
  if (typeof value !== 'object' || value === null) {
    return false
  }

  const candidate = value as { readonly [K in keyof PagedContactsResponse]?: unknown }

  return (
    Array.isArray(candidate.items) &&
    candidate.items.every(isContact) &&
    typeof candidate.total === 'number' &&
    typeof candidate.page === 'number' &&
    typeof candidate.pageSize === 'number'
  )
}

function isVoid(value: unknown): value is void {
  return value === null || value === undefined
}

export async function getContacts(
  page: number = 1,
  pageSize: number = 20,
  search?: string,
  signal?: AbortSignal,
): Promise<PagedContactsResponse> {
  const params = new URLSearchParams({
    page: String(page),
    pageSize: String(pageSize),
  })

  if (search !== undefined && search.trim().length > 0) {
    params.set('search', search.trim())
  }

  const options = signal !== undefined ? { signal } : {}
  return await apiFetch<PagedContactsResponse>(`/api/contacts?${params.toString()}`, isPagedContactsResponse, options)
}

export async function getContactById(id: number, signal?: AbortSignal): Promise<Contact> {
  const options = signal !== undefined ? { signal } : {}
  return await apiFetch<Contact>(`/api/contacts/${id}`, isContact, options)
}

export async function createContact(
  request: CreateContactRequest,
  signal?: AbortSignal,
): Promise<Contact> {
  const options = {
    method: 'POST',
    body: JSON.stringify(request),
    ...(signal !== undefined ? { signal } : {}),
  }
  return await apiFetch<Contact>('/api/contacts', isContact, options)
}

export async function updateContact(
  id: number,
  request: UpdateContactRequest,
  signal?: AbortSignal,
): Promise<Contact> {
  const options = {
    method: 'PUT',
    body: JSON.stringify(request),
    ...(signal !== undefined ? { signal } : {}),
  }
  return await apiFetch<Contact>(`/api/contacts/${id}`, isContact, options)
}

export async function deleteContact(
  id: number,
  signal?: AbortSignal,
): Promise<void> {
  const options = {
    method: 'DELETE',
    ...(signal !== undefined ? { signal } : {}),
  }
  return await apiFetch<void>(`/api/contacts/${id}`, isVoid, options)
}
