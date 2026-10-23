import type { CreateFeedbackRequest, FeedbackResponse, UpdateFeedbackRequest } from '../types/feedback.ts'
import { apiFetch, apiFetchBlob } from './apiClient.ts'

export function isFeedbackResponse(value: unknown): value is FeedbackResponse {
  if (typeof value !== 'object' || value === null) {
    return false
  }

  const candidate = value as { readonly [K in keyof FeedbackResponse]?: unknown }

  return (
    typeof candidate.id === 'number' &&
    typeof candidate.surveyId === 'number' &&
    typeof candidate.contactId === 'number' &&
    typeof candidate.surveyName === 'string' &&
    typeof candidate.surveyType === 'string' &&
    typeof candidate.score === 'number' &&
    (candidate.comment === null || typeof candidate.comment === 'string') &&
    typeof candidate.channel === 'string' &&
    typeof candidate.respondedAt === 'string'
  )
}

export function isFeedbackResponseList(value: unknown): value is readonly FeedbackResponse[] {
  return Array.isArray(value) && value.every(isFeedbackResponse)
}

function isVoid(value: unknown): value is void {
  return value === null || value === undefined
}

export async function getContactFeedbacks(
  contactId: number,
  signal?: AbortSignal,
): Promise<readonly FeedbackResponse[]> {
  const options = signal !== undefined ? { signal } : {}
  return await apiFetch<readonly FeedbackResponse[]>(
    `/api/contacts/${contactId}/responses`,
    isFeedbackResponseList,
    options,
  )
}

export async function getFeedbackById(id: number, signal?: AbortSignal): Promise<FeedbackResponse> {
  const options = signal !== undefined ? { signal } : {}
  return await apiFetch<FeedbackResponse>(`/api/responses/${id}`, isFeedbackResponse, options)
}

export async function createFeedback(
  request: CreateFeedbackRequest,
  signal?: AbortSignal,
): Promise<FeedbackResponse> {
  const options = {
    method: 'POST',
    body: JSON.stringify(request),
    ...(signal !== undefined ? { signal } : {}),
  }
  return await apiFetch<FeedbackResponse>('/api/responses', isFeedbackResponse, options)
}

export async function updateFeedback(
  id: number,
  request: UpdateFeedbackRequest,
  signal?: AbortSignal,
): Promise<FeedbackResponse> {
  const options = {
    method: 'PUT',
    body: JSON.stringify(request),
    ...(signal !== undefined ? { signal } : {}),
  }
  return await apiFetch<FeedbackResponse>(`/api/responses/${id}`, isFeedbackResponse, options)
}

export async function deleteFeedback(id: number, signal?: AbortSignal): Promise<void> {
  const options = {
    method: 'DELETE',
    ...(signal !== undefined ? { signal } : {}),
  }
  return await apiFetch<void>(`/api/responses/${id}`, isVoid, options)
}

export interface ExportResponsesFilter {
  readonly from?: string | undefined
  readonly to?: string | undefined
  readonly surveyId?: number | undefined
}

export async function exportResponsesCsv(
  filter: ExportResponsesFilter = {},
  signal?: AbortSignal,
): Promise<Blob> {
  const query = new URLSearchParams()
  if (filter.from !== undefined && filter.from.length > 0) {
    query.set('from', filter.from)
  }
  if (filter.to !== undefined && filter.to.length > 0) {
    query.set('to', filter.to)
  }
  if (filter.surveyId !== undefined && filter.surveyId > 0) {
    query.set('surveyId', String(filter.surveyId))
  }

  const queryString = query.toString()
  const endpoint = queryString.length > 0 ? `/api/responses/export?${queryString}` : '/api/responses/export'

  const options = signal !== undefined ? { signal } : {}
  return await apiFetchBlob(endpoint, options)
}

export function downloadBlob(blob: Blob, filename = 'respostas.csv'): void {
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = filename
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  URL.revokeObjectURL(url)
}
