export interface Company {
  readonly id: number
  readonly name: string
  readonly description: string
  readonly createdAt: string
}

export interface CreateCompanyRequest {
  readonly name: string
  readonly description: string
}

export interface UpdateCompanyRequest {
  readonly name: string
  readonly description: string
}

export interface CompanySurveyImpactItem {
  readonly id: number
  readonly name: string
  readonly type: string
  readonly responsesCount: number
}

export interface CompanyImpactSummary {
  readonly companyId: number
  readonly companyName: string
  readonly activeContactsCount: number
  readonly totalResponsesCount: number
  readonly surveys: readonly CompanySurveyImpactItem[]
}
