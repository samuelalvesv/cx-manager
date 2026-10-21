export interface Contact {
  readonly id: number
  readonly name: string
  readonly email: string
  readonly segment: string
  readonly createdAt: string
}

export interface CreateContactRequest {
  readonly name: string
  readonly email: string
  readonly segment: string
}

export interface UpdateContactRequest {
  readonly name: string
  readonly email: string
  readonly segment: string
}

export interface PagedContactsResponse {
  readonly items: readonly Contact[]
  readonly total: number
  readonly page: number
  readonly pageSize: number
}
