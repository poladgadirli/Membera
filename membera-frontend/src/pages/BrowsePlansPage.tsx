import { useEffect, useState, type ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { DashboardShell } from '@/components/DashboardShell'
import { CalendarIcon, ClockIcon, ImageIcon, RepeatIcon } from '@/components/icons'
import { PageHeading } from '@/components/PageHeading'
import { Spinner } from '@/components/Spinner'
import {
  formatDuration,
  formatPrice,
  formatTimeRange,
  formatUsageLimit,
} from '@/lib/merchant'
import { BTN_PRIMARY, CARD, ERROR_BANNER, SECTION_LABEL } from '@/lib/ui'
import {
  ApiError,
  BROWSE_PLANS_USES_PLACEHOLDER,
  browseActivePlans,
  checkoutSubscription,
  type BrowsePlan,
} from '@/lib/subscriptions'

type Status = 'loading' | 'ready' | 'error'

export default function BrowsePlansPage() {
  const [status, setStatus] = useState<Status>('loading')
  const [plans, setPlans] = useState<BrowsePlan[]>([])
  const [loadError, setLoadError] = useState<string | null>(null)

  useEffect(() => {
    let active = true
    browseActivePlans().then(
      (data) => {
        if (!active) return
        setPlans(data)
        setStatus('ready')
      },
      (err) => {
        if (!active) return
        setLoadError(
          err instanceof ApiError
            ? err.message
            : 'Could not load subscription plans.',
        )
        setStatus('error')
      },
    )
    return () => {
      active = false
    }
  }, [])

  return (
    <DashboardShell>
      <PageHeading
        eyebrow="Browse plans"
        title="Find a plan to subscribe to"
        description="Buy a subscription once, then redeem it at the counter with a single scan — no app, no card, just your code."
      />

      {BROWSE_PLANS_USES_PLACEHOLDER && (
        <div className="mt-6 rounded-lg border border-amber-200 bg-amber-50 px-3 py-2.5 text-sm text-amber-800">
          <strong className="font-semibold">Preview data.</strong> There is no
          backend endpoint yet to list active plans across all merchants, so
          these cards are sample data. The page is wired to swap in the real
          call (<code className="font-mono text-xs">browseActivePlans()</code> in{' '}
          <code className="font-mono text-xs">src/lib/subscriptions.ts</code>).
        </div>
      )}

      <section aria-labelledby="browse-plans-heading" className="mt-10">
        <h2 id="browse-plans-heading" className={SECTION_LABEL}>
          Available plans
        </h2>

        <div className="mt-3">
          {status === 'loading' && (
            <div className={`${CARD} p-6`}>
              <div className="flex items-center gap-3 text-sm text-neutral-500">
                <Spinner /> Loading plans…
              </div>
            </div>
          )}

          {status === 'error' && (
            <div className={`${CARD} p-6`}>
              <div className={ERROR_BANNER}>{loadError}</div>
            </div>
          )}

          {status === 'ready' && plans.length === 0 && (
            <div
              className={`${CARD} border-dashed border-neutral-300 p-8 text-center`}
            >
              <p className="text-sm font-medium text-neutral-900">
                No plans available right now
              </p>
              <p className="mx-auto mt-1 max-w-sm text-sm text-neutral-500">
                Check back soon — merchants are still setting up their
                subscription plans.
              </p>
            </div>
          )}

          {status === 'ready' && plans.length > 0 && (
            <ul className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
              {plans.map((plan) => (
                <BrowsePlanCard key={plan.id} plan={plan} />
              ))}
            </ul>
          )}
        </div>
      </section>

      <p className="mt-10 text-sm text-neutral-500">
        Already subscribed?{' '}
        <Link
          to="/dashboard"
          className="font-medium text-blue-600 transition-colors hover:text-blue-500"
        >
          View your subscriptions
        </Link>
        .
      </p>
    </DashboardShell>
  )
}

function BrowsePlanCard({ plan }: { plan: BrowsePlan }) {
  const [redirecting, setRedirecting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const merchantMonogram =
    plan.merchantBusinessName.trim().charAt(0).toUpperCase() || 'M'

  const handleSubscribe = async () => {
    setError(null)
    setRedirecting(true)
    try {
      const { checkoutUrl } = await checkoutSubscription(plan.id)
      // Leave the SPA for Stripe's hosted checkout page.
      window.location.href = checkoutUrl
    } catch (err) {
      setError(
        err instanceof ApiError
          ? err.message
          : 'Could not start checkout. Please try again.',
      )
      setRedirecting(false)
    }
  }

  return (
    <li className={`${CARD} flex flex-col`}>
      {/* Top: plan image (or a gradient placeholder). */}
      <div className="relative aspect-[3/2] overflow-hidden rounded-t-2xl bg-linear-to-br from-blue-100 via-blue-50 to-white">
        {plan.imageUrl ? (
          <img
            src={plan.imageUrl}
            alt=""
            className="h-full w-full object-cover"
          />
        ) : (
          <div className="flex h-full w-full flex-col items-center justify-center gap-2 text-blue-400">
            <ImageIcon className="h-8 w-8" />
          </div>
        )}
      </div>

      {/* Body: merchant, name / price, description, meta. */}
      <div className="flex flex-1 flex-col p-4">
        <div className="flex items-center gap-2 text-xs text-neutral-500">
          {plan.merchantLogoUrl ? (
            <img
              src={plan.merchantLogoUrl}
              alt=""
              className="h-5 w-5 rounded-full object-cover"
            />
          ) : (
            <span className="grid h-5 w-5 place-items-center rounded-full bg-linear-to-br from-blue-500 via-blue-400 to-blue-200 text-[10px] font-bold text-white">
              {merchantMonogram}
            </span>
          )}
          <span className="truncate font-medium text-neutral-700">
            {plan.merchantBusinessName}
          </span>
        </div>

        <div className="mt-2 flex items-baseline justify-between gap-2">
          <h3 className="truncate text-base font-semibold tracking-tight text-neutral-900">
            {plan.name}
          </h3>
          <span className="shrink-0 text-base font-semibold text-neutral-900">
            {formatPrice(plan.price)}
          </span>
        </div>

        {plan.description?.trim() && (
          <p className="mt-1 line-clamp-2 text-xs text-neutral-500">
            {plan.description}
          </p>
        )}

        <div className="mt-3 flex flex-wrap gap-x-3 gap-y-1 text-xs text-neutral-500">
          <Meta
            icon={<CalendarIcon className="h-3.5 w-3.5" />}
            value={formatDuration(plan.durationInDays)}
          />
          <Meta
            icon={<RepeatIcon className="h-3.5 w-3.5" />}
            value={formatUsageLimit(plan.usageLimit)}
          />
          <Meta
            icon={<ClockIcon className="h-3.5 w-3.5" />}
            value={formatTimeRange(plan.activeFrom, plan.activeUntil)}
          />
        </div>

        {error && (
          <p className="mt-3 text-xs text-red-600" role="alert">
            {error}
          </p>
        )}

        <button
          type="button"
          onClick={handleSubscribe}
          disabled={redirecting}
          className={`${BTN_PRIMARY} mt-4 w-full`}
        >
          {redirecting && <Spinner className="h-4 w-4" />}
          {redirecting ? 'Starting checkout…' : 'Subscribe'}
        </button>
      </div>
    </li>
  )
}

function Meta({ icon, value }: { icon: ReactNode; value: string }) {
  return (
    <div className="flex items-center gap-1.5">
      <span className="text-neutral-400">{icon}</span>
      <span className="font-medium text-neutral-700">{value}</span>
    </div>
  )
}
