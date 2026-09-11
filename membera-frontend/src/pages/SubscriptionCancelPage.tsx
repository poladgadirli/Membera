import { Link } from 'react-router-dom'
import { motion } from 'motion/react'
import { DashboardShell } from '@/components/DashboardShell'
import { SPRING_UI, motionSafe, usePrefersReducedMotion } from '@/lib/motion'
import { BTN_PRIMARY, BTN_SECONDARY, CARD } from '@/lib/ui'

export default function SubscriptionCancelPage() {
  const reduced = usePrefersReducedMotion()

  return (
    <DashboardShell>
      <motion.div
        initial={{ opacity: 0, y: 8 }}
        animate={{ opacity: 1, y: 0 }}
        transition={motionSafe(SPRING_UI, reduced)}
        className={`${CARD} mx-auto max-w-xl p-8 text-center`}
      >
        <span
          aria-hidden="true"
          className="mx-auto grid h-14 w-14 place-items-center rounded-full border border-neutral-200 bg-neutral-50 text-2xl text-neutral-400"
        >
          ×
        </span>

        <h1 className="mt-5 text-2xl font-medium tracking-tight text-neutral-900">
          Checkout cancelled
        </h1>
        <p className="mx-auto mt-2 max-w-md text-sm text-neutral-500">
          No payment was taken. You can pick a plan and try again whenever
          you&rsquo;re ready.
        </p>

        <div className="mt-8 flex flex-wrap items-center justify-center gap-3">
          <Link to="/plans" className={BTN_PRIMARY}>
            Back to browse plans
          </Link>
          <Link to="/dashboard" className={BTN_SECONDARY}>
            Go to my dashboard
          </Link>
        </div>
      </motion.div>
    </DashboardShell>
  )
}
