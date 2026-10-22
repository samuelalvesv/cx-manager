import type { AnalyticsFilter, AnalyticsSummary, MonthlyNps, SegmentNps } from '../types/analytics.ts'
import { apiFetch } from './apiClient.ts'

export function isAnalyticsSummary(value: unknown): value is AnalyticsSummary {
  if (typeof value !== 'object' || value === null) {
    return false
  }

  const candidate = value as { readonly [K in keyof AnalyticsSummary]?: unknown }

  return (
    typeof candidate.responsesCount === 'number' &&
    typeof candidate.npsResponses === 'number' &&
    typeof candidate.promotersCount === 'number' &&
    typeof candidate.promotersPercent === 'number' &&
    typeof candidate.neutralsCount === 'number' &&
    typeof candidate.neutralsPercent === 'number' &&
    typeof candidate.detractorsCount === 'number' &&
    typeof candidate.detractorsPercent === 'number' &&
    typeof candidate.npsScore === 'number' &&
    (candidate.csatAvg === null || typeof candidate.csatAvg === 'number')
  )
}

export function isMonthlyNps(value: unknown): value is MonthlyNps {
  if (typeof value !== 'object' || value === null) {
    return false
  }

  const candidate = value as { readonly [K in keyof MonthlyNps]?: unknown }

  return (
    typeof candidate.month === 'string' &&
    typeof candidate.responsesCount === 'number' &&
    typeof candidate.promotersCount === 'number' &&
    typeof candidate.neutralsCount === 'number' &&
    typeof candidate.detractorsCount === 'number' &&
    typeof candidate.npsScore === 'number'
  )
}

export function isMonthlyNpsList(value: unknown): value is readonly MonthlyNps[] {
  return Array.isArray(value) && value.every(isMonthlyNps)
}

export function isSegmentNps(value: unknown): value is SegmentNps {
  if (typeof value !== 'object' || value === null) {
    return false
  }

  const candidate = value as { readonly [K in keyof SegmentNps]?: unknown }

  return (
    typeof candidate.segment === 'string' &&
    typeof candidate.responsesCount === 'number' &&
    typeof candidate.promotersCount === 'number' &&
    typeof candidate.neutralsCount === 'number' &&
    typeof candidate.detractorsCount === 'number' &&
    typeof candidate.npsScore === 'number'
  )
}

export function isSegmentNpsList(value: unknown): value is readonly SegmentNps[] {
  return Array.isArray(value) && value.every(isSegmentNps)
}

function buildQueryString(filter?: AnalyticsFilter): string {
  if (filter === undefined) {
    return ''
  }
  const params = new URLSearchParams()
  if (filter.from !== undefined && filter.from.length > 0) {
    params.set('from', filter.from)
  }
  if (filter.to !== undefined && filter.to.length > 0) {
    params.set('to', filter.to)
  }
  if (filter.surveyId !== undefined && filter.surveyId > 0) {
    params.set('surveyId', String(filter.surveyId))
  }
  const query = params.toString()
  return query.length > 0 ? `?${query}` : ''
}

export async function getAnalyticsSummary(
  filter?: AnalyticsFilter,
  signal?: AbortSignal,
): Promise<AnalyticsSummary> {
  const query = buildQueryString(filter)
  const options = signal !== undefined ? { signal } : {}
  return await apiFetch<AnalyticsSummary>(`/api/analytics/summary${query}`, isAnalyticsSummary, options)
}

export async function getNpsByMonth(
  filter?: AnalyticsFilter,
  signal?: AbortSignal,
): Promise<readonly MonthlyNps[]> {
  const query = buildQueryString(filter)
  const options = signal !== undefined ? { signal } : {}
  return await apiFetch<readonly MonthlyNps[]>(`/api/analytics/nps-by-month${query}`, isMonthlyNpsList, options)
}

export async function getNpsBySegment(
  filter?: AnalyticsFilter,
  signal?: AbortSignal,
): Promise<readonly SegmentNps[]> {
  const query = buildQueryString(filter)
  const options = signal !== undefined ? { signal } : {}
  return await apiFetch<readonly SegmentNps[]>(`/api/analytics/nps-by-segment${query}`, isSegmentNpsList, options)
}
