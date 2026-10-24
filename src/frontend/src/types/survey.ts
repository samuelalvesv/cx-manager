export type SurveyType = 'NPS' | 'CSAT'

export interface Survey {
  readonly id: number
  readonly name: string
  readonly type: SurveyType
  readonly createdAt: string
  readonly responsesCount: number
}

export interface CreateSurveyRequest {
  readonly name: string
  readonly type: SurveyType
}

export interface UpdateSurveyRequest {
  readonly name: string
}
