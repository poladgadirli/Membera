import { BADGE_ACTIVE, BADGE_INACTIVE } from '@/lib/ui'

interface StatusBadgeProps {
  active: boolean
  activeLabel?: string
  inactiveLabel?: string
}

/** Small pill showing an active / inactive state, in the landing-page blue. */
export function StatusBadge({
  active,
  activeLabel = 'Active',
  inactiveLabel = 'Inactive',
}: StatusBadgeProps) {
  return active ? (
    <span className={BADGE_ACTIVE}>
      <span className="h-1.5 w-1.5 rounded-full bg-blue-500" />
      {activeLabel}
    </span>
  ) : (
    <span className={BADGE_INACTIVE}>
      <span className="h-1.5 w-1.5 rounded-full bg-neutral-400" />
      {inactiveLabel}
    </span>
  )
}
