export interface Contact {
  readonly id: number
  readonly name: string
  readonly email: string
  readonly segment: string | null
  readonly createdAt: string
}

export interface CreateContactRequest {
  readonly name: string
  readonly email: string
  readonly segment: string | null
}

export interface UpdateContactRequest {
  readonly name: string
  readonly email: string
  readonly segment: string | null
}

export interface PagedContactsResponse {
  readonly items: readonly Contact[]
  readonly total: number
  readonly page: number
  readonly pageSize: number
}
