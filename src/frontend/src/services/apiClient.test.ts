import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import {
  apiFetch,
  apiFetchBlob,
  getSelectedCompanyId,
  setSelectedCompanyId,
} from './apiClient.ts'

interface SamplePayload {
  readonly id: number
  readonly name: string
}

function isSamplePayload(value: unknown): value is SamplePayload {
  if (typeof value !== 'object' || value === null) {
    return false
  }
  return 'id' in value && typeof value.id === 'number' && 'name' in value && typeof value.name === 'string'
}

describe('apiClient', () => {
  beforeEach(() => {
    localStorage.clear()
    setSelectedCompanyId(null)
    vi.restoreAllMocks()
  })

  afterEach(() => {
    localStorage.clear()
    setSelectedCompanyId(null)
  })

  describe('Company Context State', () => {
    it('deve armazenar e recuperar a empresa selecionada via memória e localStorage', () => {
      expect(getSelectedCompanyId()).toBeNull()

      setSelectedCompanyId(42)
      expect(getSelectedCompanyId()).toBe(42)
      expect(localStorage.getItem('cx_company_id')).toBe('42')

      setSelectedCompanyId(null)
      expect(getSelectedCompanyId()).toBeNull()
      expect(localStorage.getItem('cx_company_id')).toBeNull()
    })
  })

  describe('apiFetch', () => {
    it('deve executar requisição com sucesso e validar o payload de retorno', async () => {
      const mockData: SamplePayload = { id: 1, name: 'Empresa Teste' }

      const response = new Response(JSON.stringify(mockData), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
      })
      const fetchMock = vi.fn<typeof fetch>().mockResolvedValue(response)

      vi.stubGlobal('fetch', fetchMock)

      setSelectedCompanyId(10)

      const result = await apiFetch<SamplePayload>('/api/test', isSamplePayload)

      expect(result).toEqual(mockData)
      expect(fetchMock).toHaveBeenCalledTimes(1)

      const firstCall = fetchMock.mock.calls[0]
      expect(firstCall).toBeDefined()
      if (firstCall) {
        const [calledUrl, calledOptions] = firstCall
        expect(calledUrl).toBe('/api/test')

        const headers = new Headers(calledOptions?.headers)
        expect(headers.get('Content-Type')).toBe('application/json')
        expect(headers.get('X-Company-Id')).toBe('10')
      }
    })

    it('não deve enviar X-Company-Id quando skipCompanyId for verdadeiro', async () => {
      const mockData: SamplePayload = { id: 1, name: 'Público' }

      const response = new Response(JSON.stringify(mockData), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
      })
      const fetchMock = vi.fn<typeof fetch>().mockResolvedValue(response)

      vi.stubGlobal('fetch', fetchMock)

      setSelectedCompanyId(10)

      const result = await apiFetch<SamplePayload>('/api/public', isSamplePayload, {
        skipCompanyId: true,
      })

      expect(result).toEqual(mockData)

      const firstCall = fetchMock.mock.calls[0]
      expect(firstCall).toBeDefined()
      if (firstCall) {
        const [, calledOptions] = firstCall
        const headers = new Headers(calledOptions?.headers)
        expect(headers.get('X-Company-Id')).toBeNull()
      }
    })

    it('deve lançar exceção quando o validador rejeita o formato retornado', async () => {
      const invalidData = { id: 'não-é-um-número', name: 'Dado Inválido' }

      const response = new Response(JSON.stringify(invalidData), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
      })
      const fetchMock = vi.fn<typeof fetch>().mockResolvedValue(response)

      vi.stubGlobal('fetch', fetchMock)

      await expect(apiFetch('/api/invalid', isSamplePayload)).rejects.toThrow(
        'Resposta da API em formato inesperado.',
      )
    })

    it('deve extrair e repassar a mensagem de erro da API quando status não for OK', async () => {
      const errorPayload = { error: 'Empresa não encontrada no banco de dados.' }

      const response = new Response(JSON.stringify(errorPayload), {
        status: 404,
        statusText: 'Not Found',
        headers: { 'Content-Type': 'application/json' },
      })
      const fetchMock = vi.fn<typeof fetch>().mockResolvedValue(response)

      vi.stubGlobal('fetch', fetchMock)

      await expect(apiFetch('/api/missing', isSamplePayload)).rejects.toThrow(
        'Empresa não encontrada no banco de dados.',
      )
    })
  })

  describe('apiFetchBlob', () => {
    it('deve retornar o Blob na requisição de exportação com sucesso', async () => {
      const mockBlob = new Blob(['coluna1,coluna2\nvalor1,valor2'], { type: 'text/csv' })

      const response = new Response(mockBlob, {
        status: 200,
        headers: { 'Content-Type': 'text/csv' },
      })
      const fetchMock = vi.fn<typeof fetch>().mockResolvedValue(response)

      vi.stubGlobal('fetch', fetchMock)

      const blob = await apiFetchBlob('/api/responses/export')

      expect(blob).toBeDefined()
      expect(fetchMock).toHaveBeenCalledTimes(1)
    })
  })
})
