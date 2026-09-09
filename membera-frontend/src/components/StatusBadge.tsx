interface StatusBadgeProps {
  active: boolean
  activeLabel?: string
  inactiveLabel?: string
}

/** Small pill showing an active / inactive state, using theme tokens only. */
export function StatusBadge({
  active,
  activeLabel = 'Active',
  inactiveLabel = 'Inactive',
}: StatusBadgeProps) {
  return active ? (
    <span className="inline-flex items-center gap-1.5 rounded-full bg-primary/10 px-2.5 py-0.5 text-xs font-medium text-primary">
      <span className="h-1.5 w-1.5 rounded-full bg-primary" />
      {activeLabel}
    </span>
  ) : (
    <span className="inline-flex items-center gap-1.5 rounded-full bg-muted px-2.5 py-0.5 text-xs font-medium text-muted-foreground">
      <span className="h-1.5 w-1.5 rounded-full bg-muted-foreground" />
      {inactiveLabel}
    </span>
  )
}
