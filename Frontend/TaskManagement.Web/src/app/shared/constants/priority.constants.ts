export interface PriorityDefinition {
  readonly label: string;
  readonly className: string;
}

export const PRIORITY_DEFINITIONS: Readonly<Record<number, PriorityDefinition>> = {
  1: { label: 'Düşük', className: 'priority-low' },
  2: { label: 'Normal', className: 'priority-normal' },
  3: { label: 'Yüksek', className: 'priority-high' },
  4: { label: 'Acil', className: 'priority-urgent' },
  5: { label: 'Kritik', className: 'priority-critical' },
};

export const PRIORITY_OPTIONS = Object.entries(PRIORITY_DEFINITIONS).map(
  ([value, definition]) => ({
    value: Number(value),
    label: definition.label,
  })
);

const UNKNOWN_PRIORITY: PriorityDefinition = {
  label: 'Bilinmiyor',
  className: 'priority-unknown',
};

export function getPriorityDefinition(priority: number): PriorityDefinition {
  return PRIORITY_DEFINITIONS[priority] ?? UNKNOWN_PRIORITY;
}
