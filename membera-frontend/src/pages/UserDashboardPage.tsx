import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { DashboardShell } from '@/components/DashboardShell'
import { PageHeading } from '@/components/PageHeading'
import { Spinner } from '@/components/Spinner'
import { SubscriptionStatusBadge } from '@/components/SubscriptionStatusBadge'
import { useAuth } from '@/hooks/useAuth'
import { formatUsageLimit } from '@/lib/merchant'
import { BTN_PRIMARY, CARD, ERROR_BANNER, SECTION_LABEL } from '@/lib/ui'
import {
  ApiError,
  getMySubscriptions,
  type UserSubscription,
} from '@/lib/subscriptions'

type Status = 'loading' | 'ready' | 'error'

/** The API sends DateTime.MinValue ("0001-01-01…") for not-yet-activated subs. */
function hasRealDate(iso: string): boolean {
  const time = Date.parse(iso)
  return Number.isFinite(time) && new Date(time).getUTCFullYear() > 2000
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}

export default function UserDashboardPage() {
  const { user } = useAuth()
  const firstName = user?.firstName?.trim()

  const [status, setStatus] = useState<Status>('loading')
  const [subscriptions, setSubscriptions] = useState<UserSubscription[]>([])
  const [loadError, setLoadError] = useState<string | null>(null)

  useEffect(() => {
    let active = true
    getMySubscriptions().then(
      (data) => {
        if (!active) return
        setSubscriptions(data)
        setStatus('ready')
      },
      (err) => {
        if (!active) return
        setLoadError(
          err instanceof ApiError
            ? err.message
            : 'Could not load your subscriptions.',
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
        eyebrow="Your account"
        title={firstName ? `Welcome, ${firstName}!` : 'Welcome!'}
        description="Your subscriptions and the redemption codes you show at the counter."
      />

      <section aria-labelledby="my-subscriptions-heading" className="mt-10">
        <div className="flex items-center justify-between gap-4">
          <h2 id="my-subscriptions-heading" className={SECTION_LABEL}>
            My subscriptions
          </h2>
          <Link to="/plans" className={BTN_PRIMARY}>
            Browse plans
          </Link>
        </div>

        <div className="mt-3">
          {status === 'loading' && (
            <div className={`${CARD} p-6`}>
              <div className="flex items-center gap-3 text-sm text-neutral-500">
                <Spinner /> Loading your subscriptions…
              </div>
            </div>
          )}

          {status === 'error' && (
            <div className={`${CARD} p-6`}>
              <div className={ERROR_BANNER}>{loadError}</div>
            </div>
          )}

          {status === 'ready' && subscriptions.length === 0 && (
            <div
              className={`${CARD} border-dashed border-neutral-300 p-8 text-center`}
            >
              <p className="text-sm font-medium text-neutral-900">
                You don&rsquo;t have any subscriptions yet
              </p>
              <p className="mx-auto mt-1 max-w-sm text-sm text-neutral-500">
                Browse plans to get started — buy once, then redeem at the
                counter with your code.
              </p>
              <Link to="/plans" className={`${BTN_PRIMARY} mt-4`}>
                Browse plans
              </Link>
            </div>
          )}

          {status === 'ready' && subscriptions.length > 0 && (
            <ul className="grid gap-5 sm:grid-cols-2">
              {subscriptions.map((subscription) => (
                <SubscriptionCard
                  key={subscription.id}
                  subscription={subscription}
                />
              ))}
            </ul>
          )}
        </div>
      </section>
    </DashboardShell>
  )
}

function SubscriptionCard({
  subscription,
}: {
  subscription: UserSubscription
}) {
  const [copied, setCopied] = useState(false)

  const isActive = subscription.status === 'Active'
  const showCode = subscription.status === 'Active' || subscription.status === 'Pending'

  const copyCode = async () => {
    try {
      await navigator.clipboard.writeText(subscription.redemptionCode)
      setCopied(true)
      setTimeout(() => setCopied(false), 2000)
    } catch {
      // Clipboard blocked — the code is visible on screen anyway.
    }
  }

  return (
    <li className={`${CARD} flex flex-col p-5`}>
      <div className="flex items-start justify-between gap-3">
        <h3 className="text-base font-semibold tracking-tight text-neutral-900">
          {subscription.planName || 'Subscription'}
        </h3>
        <SubscriptionStatusBadge status={subscription.status} />
      </div>

      {/* Redemption code — the thing the user reads out at the counter. */}
      <div className="mt-4 rounded-xl border border-neutral-200 bg-neutral-50 p-3">
        <p className="text-xs font-medium uppercase tracking-wide text-neutral-500">
          Redemption code
        </p>
        {showCode ? (
          <div className="mt-1 flex items-center justify-between gap-3">
            <span className="font-mono text-lg font-semibold tracking-wider text-neutral-900">
              {subscription.redemptionCode}
            </span>
            <button
              type="button"
              onClick={copyCode}
              className="shrink-0 rounded-md px-2 py-1 text-xs font-medium text-blue-600 transition-colors hover:bg-blue-50 hover:text-blue-500 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-500"
            >
              {copied ? 'Copied' : 'Copy'}
            </button>
          </div>
        ) : (
          <p className="mt-1 text-sm text-neutral-500">
            Available once the subscription is active.
          </p>
        )}
      </div>

      <dl className="mt-4 grid grid-cols-2 gap-3 text-sm">
        <div>
          <dt className="text-xs font-medium uppercase tracking-wide text-neutral-500">
            {isActive ? 'Expires' : 'Valid until'}
          </dt>
          <dd className="mt-0.5 font-medium text-neutral-900">
            {hasRealDate(subscription.expiresAt)
              ? formatDate(subscription.expiresAt)
              : '—'}
          </dd>
        </div>
        <div>
          <dt className="text-xs font-medium uppercase tracking-wide text-neutral-500">
            Usages remaining
          </dt>
          <dd className="mt-0.5 font-medium text-neutral-900">
            {subscription.usagesRemaining == null
              ? formatUsageLimit(null)
              : subscription.usagesRemaining}
          </dd>
        </div>
      </dl>

      {subscription.status === 'Pending' && (
        <p className="mt-4 text-xs text-neutral-500">
          Payment is still being confirmed. This usually takes a moment — refresh
          to check.
        </p>
      )}
    </li>
  )
}
