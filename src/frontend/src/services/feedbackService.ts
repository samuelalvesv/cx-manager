import type { CreateFeedbackRequest, FeedbackResponse, UpdateFeedbackRequest } from '../types/feedback.ts'
import { apiFetch } from './apiClient.ts'

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
