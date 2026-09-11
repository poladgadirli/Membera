import { Link } from 'react-router-dom'
import { motion } from 'motion/react'
import { useAuth } from '@/hooks/useAuth'
import { SPRING_UI, motionSafe, usePrefersReducedMotion } from '@/lib/motion'
import { BTN_PRIMARY, CARD, PAGE_BG, PAGE_WASH } from '@/lib/ui'

export default function NotAuthorizedPage() {
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
          className="mx-auto grid h-10 w-10 place-items-center rounded-lg border border-red-200 bg-red-50 text-lg font-semibold text-red-600"
        >
          !
        </span>
        <h1 className="mt-4 text-2xl font-medium tracking-tight text-neutral-900">
          You don&rsquo;t have access to this page
        </h1>
        <p className="mt-2 text-sm text-neutral-500">
          Your account role doesn&rsquo;t permit viewing this area. If you think
          this is a mistake, contact your administrator.
        </p>
        <Link
          to={isAuthenticated ? '/dashboard' : '/login'}
          className={`${BTN_PRIMARY} mt-6`}
        >
          {isAuthenticated ? 'Back to dashboard' : 'Go to sign in'}
        </Link>
      </motion.div>
    </div>
  )
}
