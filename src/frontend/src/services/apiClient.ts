const STORAGE_KEY = 'cx_company_id'

let currentCompanyId: number | null = (() => {
  try {
    const saved = localStorage.getItem(STORAGE_KEY)
    if (saved !== null) {
      const parsed = Number(saved)
      if (!Number.isNaN(parsed) && parsed > 0) {
        return parsed
      }
    }
  } catch {
    // LocalStorage indisponível em determinados ambientes
  }
  return null
})()

export function getSelectedCompanyId(): number | null {
  return currentCompanyId
}

export function setSelectedCompanyId(id: number | null): void {
  currentCompanyId = id
  try {
    if (id !== null) {
      localStorage.setItem(STORAGE_KEY, String(id))
    } else {
      localStorage.removeItem(STORAGE_KEY)
    }
  } catch {
    // Ignora falhas de escrita no localStorage
  }
}

export interface RequestOptions extends RequestInit {
  readonly skipCompanyId?: boolean
}

function parseApiErrorMessage(value: unknown): string | null {
  if (typeof value === 'object' && value !== null && 'error' in value) {
    const errorProp = value.error
    if (typeof errorProp === 'string' && errorProp.length > 0) {
      return errorProp
    }
  }
  return null
}

export async function apiFetch<T>(
  endpoint: string,
  validator: (data: unknown) => data is T,
  options: RequestOptions = {},
): Promise<T> {
  const { skipCompanyId = false, headers, ...rest } = options

  const requestHeaders = new Headers(headers)
  if (!requestHeaders.has('Content-Type')) {
    requestHeaders.set('Content-Type', 'application/json')
  }

  if (!skipCompanyId && currentCompanyId !== null) {
    requestHeaders.set('X-Company-Id', String(currentCompanyId))
  }

  const response = await fetch(endpoint, {
    ...rest,
    headers: requestHeaders,
  })

  if (!response.ok) {
    let errorMessage = `Erro HTTP ${response.status}: ${response.statusText}`
    try {
      const errorJson: unknown = await response.json()
      const parsedMessage = parseApiErrorMessage(errorJson)
      if (parsedMessage !== null) {
        errorMessage = parsedMessage
      }
    } catch {
      // Falha ao parsear corpo de erro JSON
    }
    throw new Error(errorMessage)
  }

  const text = await response.text()
  const result: unknown = text.length > 0 ? JSON.parse(text) : null

  if (validator(result)) {
    return result
  }

  throw new Error('Resposta da API em formato inesperado.')
}

export async function apiFetchBlob(
  endpoint: string,
  options: RequestOptions = {},
): Promise<Blob> {
  const { skipCompanyId = false, headers, ...rest } = options

  const requestHeaders = new Headers(headers)

  if (!skipCompanyId && currentCompanyId !== null) {
    requestHeaders.set('X-Company-Id', String(currentCompanyId))
  }

  const response = await fetch(endpoint, {
    ...rest,
    headers: requestHeaders,
  })

  if (!response.ok) {
    let errorMessage = `Erro HTTP ${response.status}: ${response.statusText}`
    try {
      const errorJson: unknown = await response.json()
      const parsedMessage = parseApiErrorMessage(errorJson)
      if (parsedMessage !== null) {
        errorMessage = parsedMessage
      }
    } catch {
      // Falha ao parsear corpo de erro JSON
    }
    throw new Error(errorMessage)
  }

  return await response.blob()
}
