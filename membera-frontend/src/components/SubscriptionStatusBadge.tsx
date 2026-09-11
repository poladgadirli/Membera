import type { SubscriptionStatus } from '@/lib/subscriptions'

// Pill styles per subscription status, in the app's palette (blue = the live/
// good state, amber = waiting, neutral = done, red = cancelled). Shape matches
// BADGE_ACTIVE / BADGE_INACTIVE in src/lib/ui.ts.
const STYLES: Record<string, { className: string; dot: string; label: string }> =
  {
    Active: {
      className:
        'border-blue-200 bg-blue-50 text-blue-700',
      dot: 'bg-blue-500',
      label: 'Active',
    },
    Pending: {
      className: 'border-amber-200 bg-amber-50 text-amber-700',
      dot: 'bg-amber-500',
      label: 'Pending',
    },
    Expired: {
      className: 'border-neutral-200 bg-neutral-100 text-neutral-500',
      dot: 'bg-neutral-400',
      label: 'Expired',
    },
    Cancelled: {
      className: 'border-red-200 bg-red-50 text-red-700',
      dot: 'bg-red-500',
      label: 'Cancelled',
    },
  }

export function SubscriptionStatusBadge({
  status,
}: {
  status: SubscriptionStatus | string
}) {
  const style = STYLES[status] ?? STYLES.Expired
  const label = STYLES[status]?.label ?? status

  return (
    <span
      className={`inline-flex items-center gap-1.5 rounded-full border px-2.5 py-0.5 text-xs font-medium ${style.className}`}
    >
      <span className={`h-1.5 w-1.5 rounded-full ${style.dot}`} />
      {label}
    </span>
  )
}
