import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import dayjs, { type Dayjs } from 'dayjs'

export interface DatePickerFieldProps {
  readonly label: string
  readonly value: string
  readonly onChange: (newIsoDate: string) => void
  readonly minDate?: string | undefined
  readonly maxDate?: string | undefined
  readonly disabled?: boolean | undefined
}

const WEEKDAY_LABELS = ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb'] as const

function toDayjs(iso: string | undefined): Dayjs | undefined {
  if (iso === undefined || iso.trim().length === 0) {
    return undefined
  }
  const parsed = dayjs(iso, 'YYYY-MM-DD', true)
  return parsed.isValid() ? parsed : dayjs(iso)
}

export function DatePickerField({
  label,
  value,
  onChange,
  minDate,
  maxDate,
  disabled = false,
}: DatePickerFieldProps) {
  const parsedValue = toDayjs(value) ?? null
  const parsedMinDate = toDayjs(minDate)
  const parsedMaxDate = toDayjs(maxDate)

  const handleDateChange = (newValue: Dayjs | null) => {
    if (newValue === null) {
      onChange('')
      return
    }
    if (newValue.isValid() && newValue.year() >= 1900 && newValue.year() <= 2100) {
      onChange(newValue.format('YYYY-MM-DD'))
    }
  }

  return (
    <DatePicker
      label={label}
      value={parsedValue}
      onChange={handleDateChange}
      format="DD/MM/YYYY"
      disabled={disabled}
      showDaysOutsideCurrentMonth
      dayOfWeekFormatter={(date) => WEEKDAY_LABELS[date.day()]}
      {...(parsedMinDate !== undefined ? { minDate: parsedMinDate } : {})}
      {...(parsedMaxDate !== undefined ? { maxDate: parsedMaxDate } : {})}
      views={['year', 'month', 'day']}
      slotProps={{
        textField: {
          size: 'small',
          fullWidth: true,
          sx: {
            '& .MuiPickersOutlinedInput-notchedOutline legend': {
              transition: 'none',
            },
          },
        },
        field: {
          clearable: true,
        },
        layout: {
          sx: {
            '& .MuiDateCalendar-root': {
              height: 'auto',
              maxHeight: 310,
              mb: 0,
            },
            '& .MuiDayCalendar-slideTransition': {
              minHeight: 204,
            },
            '& .MuiPickersLayout-actionBar': {
              pt: 0,
              pb: 1,
              px: 2,
              borderTop: '1px solid var(--mui-palette-divider)',
            },
          },
        },
        actionBar: {
          actions: ['clear', 'today'],
        },
      }}
    />
  )
}
