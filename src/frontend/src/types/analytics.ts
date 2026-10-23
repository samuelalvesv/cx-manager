export interface AnalyticsClassItem {
  readonly count: number
  readonly pct: number
}

export interface AnalyticsSummary {
  readonly npsScore: number
  readonly npsResponses: number
  readonly promoters: AnalyticsClassItem
  readonly neutrals: AnalyticsClassItem
  readonly detractors: AnalyticsClassItem
  readonly responsesCount: number
  readonly csatAvg: number | null
}

export interface MonthlyNps {
  readonly month: string
  readonly responses: number
  readonly npsScore: number
}

export interface SegmentNps {
  readonly segment: string
  readonly responses: number
  readonly npsScore: number
}

export interface AnalyticsFilter {
  readonly from?: string | undefined
  readonly to?: string | undefined
  readonly surveyId?: number | undefined
}
