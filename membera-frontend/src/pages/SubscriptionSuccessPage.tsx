import { useEffect, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { AnimatePresence, motion } from 'motion/react'
import { DashboardShell } from '@/components/DashboardShell'
import { CheckIcon } from '@/components/icons'
import { SubscriptionStatusBadge } from '@/components/SubscriptionStatusBadge'
import LoaderOne from '@/components/ui/loader-one'
import { SPRING_UI, materializeVariants, motionSafe, usePrefersReducedMotion } from '@/lib/motion'
import { BTN_PRIMARY, BTN_SECONDARY, CARD } from '@/lib/ui'
import { getMySubscriptions, type UserSubscription } from '@/lib/subscriptions'

const POLL_INTERVAL_MS = 4000
const MAX_POLLS = 6

/** Most recently started Active subscription — the one they most likely just bought. */
function newestActive(subs: UserSubscription[]): UserSubscription | null {
  const active = subs
    .filter((s) => s.status === 'Active')
    .sort((a, b) => Date.parse(b.startedAt) - Date.parse(a.startedAt))
  return active[0] ?? null
}

/**
 * The subscription to show, once it's activated. With a Stripe session id (from
 * the ?session_id= Stripe appends to the redirect) we match it exactly and wait
 * for that one row to flip Active — no guessing. Without one (direct navigation,
 * or an older backend) we fall back to the newest-Active heuristic.
 */
function pickActivated(
  subs: UserSubscription[],
  sessionId: string | null,
): UserSubscription | null {
  if (sessionId) {
    const match = subs.find((s) => s.stripeSessionId === sessionId)
    return match?.status === 'Active' ? match : null
  }
  return newestActive(subs)
}

export default function SubscriptionSuccessPage() {
  const [searchParams] = useSearchParams()
  const sessionId = searchParams.get('session_id')

  const [activated, setActivated] = useState<UserSubscription | null>(null)
  // `round` re-arms the polling effect; bumping it (via "Check again") restarts.
  const [round, setRound] = useState(0)
  const [polling, setPolling] = useState(true)
  const reduced = usePrefersReducedMotion()

  useEffect(() => {
    let cancelled = false
    let attempts = 0
    let timer: ReturnType<typeof setTimeout>

    setPolling(true)

    const tick = async () => {
      if (cancelled) return
      attempts += 1
      try {
        const found = pickActivated(await getMySubscriptions(), sessionId)
        if (found && !cancelled) {
          setActivated(found)
          setPolling(false)
          return
        }
      } catch {
        // Webhook may not have landed yet — keep polling.
      }
      if (cancelled) return
      if (attempts >= MAX_POLLS) {
        setPolling(false)
        return
      }
      timer = setTimeout(tick, POLL_INTERVAL_MS)
    }

    void tick()
    return () => {
      cancelled = true
      clearTimeout(timer)
    }
  }, [round, sessionId])

  return (
    <DashboardShell>
      <div className={`${CARD} mx-auto max-w-xl p-8 text-center`}>
        <span className="mx-auto grid h-14 w-14 place-items-center rounded-full bg-linear-to-br from-blue-500 via-blue-400 to-blue-200 text-white shadow-sm shadow-blue-500/30">
          <CheckIcon className="h-7 w-7" />
        </span>

        <h1 className="mt-5 text-2xl font-medium tracking-tight text-neutral-900">
          Payment received
        </h1>
        <p className="mx-auto mt-2 max-w-md text-sm text-neutral-500">
          Your subscription will be active shortly. Activation is confirmed by
          Stripe on our server, so it can take a moment to show up here.
        </p>

        <AnimatePresence mode="wait" initial={false}>
          {activated ? (
            <motion.div
              key="activated"
              variants={materializeVariants}
              initial="initial"
              animate="animate"
              exit="exit"
              transition={motionSafe(SPRING_UI, reduced)}
              className="mt-6 rounded-xl border border-neutral-200 bg-neutral-50 p-4 text-left"
            >
              <div className="flex items-center justify-between gap-3">
                <span className="text-sm font-semibold text-neutral-900">
                  {activated.planName || 'Your subscription'}
                </span>
                <SubscriptionStatusBadge status={activated.status} />
              </div>
              <p className="mt-3 text-xs font-medium uppercase tracking-wide text-neutral-500">
                Redemption code
              </p>
              <p className="mt-1 font-mono text-lg font-semibold tracking-wider text-neutral-900">
                {activated.redemptionCode}
              </p>
              <p className="mt-2 text-xs text-neutral-500">
                Show or read this code to the merchant at the counter.
              </p>
            </motion.div>
          ) : polling ? (
            <motion.div
              key="polling"
              variants={materializeVariants}
              initial="initial"
              animate="animate"
              exit="exit"
              transition={motionSafe(SPRING_UI, reduced)}
              className="mt-6 flex flex-col items-center gap-3 text-sm text-neutral-500"
            >
              <LoaderOne />
              Waiting for activation…
            </motion.div>
          ) : (
            <motion.div
              key="timeout"
              variants={materializeVariants}
              initial="initial"
              animate="animate"
              exit="exit"
              transition={motionSafe(SPRING_UI, reduced)}
              className="mt-6 space-y-3"
            >
              <p className="text-sm text-neutral-500">
                Still processing. You can safely leave this page — your code will
                appear on your dashboard once it&rsquo;s ready.
              </p>
              <button
                type="button"
                onClick={() => setRound((r) => r + 1)}
                className={BTN_SECONDARY}
              >
                Check again
              </button>
            </motion.div>
          )}
        </AnimatePresence>

        <div className="mt-8 flex flex-wrap items-center justify-center gap-3">
          <Link to="/dashboard" className={BTN_PRIMARY}>
            Go to my subscriptions
          </Link>
          <Link to="/plans" className={BTN_SECONDARY}>
            Browse more plans
          </Link>
        </div>
      </div>
    </DashboardShell>
  )
}
