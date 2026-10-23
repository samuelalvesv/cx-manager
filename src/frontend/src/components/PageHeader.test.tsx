import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { PageHeader } from './PageHeader.tsx'

describe('PageHeader', () => {
  it('deve renderizar o título principal corretamente', () => {
    render(<PageHeader title="Painel de Controle" />)

    expect(screen.getByRole('heading', { level: 1, name: 'Painel de Controle' })).toBeInTheDocument()
  })

  it('deve renderizar subtítulo e ação complementar quando fornecidos', () => {
    render(
      <PageHeader
        title="Contatos"
        subtitle="Gerenciamento de clientes e NPS"
        action={<button type="button">Novo Contato</button>}
      />,
    )

    expect(screen.getByText('Gerenciamento de clientes e NPS')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Novo Contato' })).toBeInTheDocument()
  })
})
