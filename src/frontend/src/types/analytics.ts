export interface AnalyticsSummary {
  readonly responsesCount: number
  readonly npsResponses: number
  readonly promotersCount: number
  readonly promotersPercent: number
  readonly neutralsCount: number
  readonly neutralsPercent: number
  readonly detractorsCount: number
  readonly detractorsPercent: number
  readonly npsScore: number
  readonly csatAvg: number | null
}

export interface MonthlyNps {
  readonly month: string
  readonly responsesCount: number
  readonly promotersCount: number
  readonly neutralsCount: number
  readonly detractorsCount: number
  readonly npsScore: number
}

export interface SegmentNps {
  readonly segment: string
  readonly responsesCount: number
  readonly promotersCount: number
  readonly neutralsCount: number
  readonly detractorsCount: number
  readonly npsScore: number
}

export interface AnalyticsFilter {
  readonly from?: string
  readonly to?: string
  readonly surveyId?: number
}
