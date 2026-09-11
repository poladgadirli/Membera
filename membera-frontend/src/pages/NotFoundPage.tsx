import { Link } from 'react-router-dom'
import { motion } from 'motion/react'
import { useAuth } from '@/hooks/useAuth'
import { SPRING_UI, motionSafe, usePrefersReducedMotion } from '@/lib/motion'
import { BTN_PRIMARY, CARD, PAGE_BG, PAGE_WASH } from '@/lib/ui'

/**
 * Catch-all 404 for unmatched routes. Same frosted-card error-state layout as
 * NotAuthorizedPage. Sends signed-in users back to their dashboard, everyone
 * else to the landing page.
 */
export default function NotFoundPage() {
  const { isAuthenticated } = useAuth()
  const reduced = usePrefersReducedMotion()

  return (
    <div
      className={`relative grid min-h-screen place-items-center overflow-hidden px-4 ${PAGE_BG}`}
    >
      <div aria-hidden="true" className={PAGE_WASH} />
      <motion.div
        initial={{ opacity: 0, y: 8 }}
        animate={{ opacity: 1, y: 0 }}
        transition={motionSafe(SPRING_UI, reduced)}
        className={`relative z-10 w-full max-w-md ${CARD} p-8 text-center`}
      >
        <span
          aria-hidden="true"
          className="mx-auto grid h-10 w-16 place-items-center rounded-lg border border-neutral-200 bg-neutral-50 text-sm font-semibold text-neutral-500"
        >
          404
        </span>
        <h1 className="mt-4 text-2xl font-medium tracking-tight text-neutral-900">
          Page not found
        </h1>
        <p className="mt-2 text-sm text-neutral-500">
          The page you&rsquo;re looking for doesn&rsquo;t exist or may have moved.
        </p>
        <Link
          to={isAuthenticated ? '/dashboard' : '/'}
          className={`${BTN_PRIMARY} mt-6`}
        >
          {isAuthenticated ? 'Back to dashboard' : 'Back to home'}
        </Link>
      </motion.div>
    </div>
  )
}
