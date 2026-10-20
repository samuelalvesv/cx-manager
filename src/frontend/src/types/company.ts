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
