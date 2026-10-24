import type { CreateSurveyRequest, Survey, UpdateSurveyRequest } from '../types/survey.ts'
import { apiFetch } from './apiClient.ts'

export function isSurvey(value: unknown): value is Survey {
  if (typeof value !== 'object' || value === null) {
    return false
  }

  const candidate = value as { readonly [K in keyof Survey]?: unknown }

  return (
    typeof candidate.id === 'number' &&
    typeof candidate.name === 'string' &&
    (candidate.type === 'NPS' || candidate.type === 'CSAT') &&
    typeof candidate.createdAt === 'string' &&
    typeof candidate.responsesCount === 'number'
  )
}

export function isSurveyList(value: unknown): value is readonly Survey[] {
  return Array.isArray(value) && value.every(isSurvey)
}

function isVoid(value: unknown): value is void {
  return value === null || value === undefined
}

export async function getSurveys(signal?: AbortSignal): Promise<readonly Survey[]> {
  const options = signal !== undefined ? { signal } : {}
  return await apiFetch<readonly Survey[]>('/api/surveys', isSurveyList, options)
}

export async function getSurveyById(id: number, signal?: AbortSignal): Promise<Survey> {
  const options = signal !== undefined ? { signal } : {}
  return await apiFetch<Survey>(`/api/surveys/${id}`, isSurvey, options)
}

export async function createSurvey(
  request: CreateSurveyRequest,
  signal?: AbortSignal,
): Promise<Survey> {
  const options = {
    method: 'POST',
    body: JSON.stringify(request),
    ...(signal !== undefined ? { signal } : {}),
  }
  return await apiFetch<Survey>('/api/surveys', isSurvey, options)
}

export async function updateSurvey(
  id: number,
  request: UpdateSurveyRequest,
  signal?: AbortSignal,
): Promise<Survey> {
  const options = {
    method: 'PUT',
    body: JSON.stringify(request),
    ...(signal !== undefined ? { signal } : {}),
  }
  return await apiFetch<Survey>(`/api/surveys/${id}`, isSurvey, options)
}

export async function deleteSurvey(
  id: number,
  signal?: AbortSignal,
): Promise<void> {
  const options = {
    method: 'DELETE',
    ...(signal !== undefined ? { signal } : {}),
  }
  return await apiFetch<void>(`/api/surveys/${id}`, isVoid, options)
}
