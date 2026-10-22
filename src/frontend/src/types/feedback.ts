export type FeedbackChannel = 'whatsapp' | 'email' | 'link'

export interface FeedbackResponse {
  readonly id: number
  readonly surveyId: number
  readonly contactId: number
  readonly surveyName: string
  readonly surveyType: string
  readonly score: number
  readonly comment: string | null
  readonly channel: string
  readonly respondedAt: string
}

export interface CreateFeedbackRequest {
  readonly surveyId: number
  readonly contactId: number
  readonly score: number
  readonly channel: string
  readonly comment: string | null
}

export interface UpdateFeedbackRequest {
  readonly score: number
  readonly comment: string | null
}
