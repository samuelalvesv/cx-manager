const MONTH_NAMES = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'] as const

export function getNpsScoreColor(score: number): 'success' | 'warning' | 'error' {
  if (score >= 50) return 'success'
  if (score >= 0) return 'warning'
  return 'error'
}

export function getNpsZoneLabel(score: number): string {
  if (score >= 75) return 'Zona de Excelência'
  if (score >= 50) return 'Zona de Qualidade'
  if (score >= 0) return 'Zona de Aperfeiçoamento'
  return 'Zona Crítica'
}

export function formatMonthYear(monthIso: string): string {
  const parts = monthIso.split('-')
  if (parts.length === 2 && parts[0] !== undefined && parts[1] !== undefined) {
    const monthIndex = Number.parseInt(parts[1], 10) - 1
    const label = MONTH_NAMES[monthIndex]
    if (label !== undefined) {
      return `${label}/${parts[0]}`
    }
  }
  return monthIso
}
