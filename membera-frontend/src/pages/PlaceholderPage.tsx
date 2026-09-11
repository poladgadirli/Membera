import { Link } from 'react-router-dom'
import { motion } from 'motion/react'
import { SPRING_UI, motionSafe, usePrefersReducedMotion } from '@/lib/motion'
import { BTN_PRIMARY, CARD, LOGO_MARK, PAGE_BG, PAGE_WASH } from '@/lib/ui'

type PlaceholderPageProps = {
  title: string
}

/**
 * Temporary stand-in for routes that are linked from the landing page but not
 * built yet (the footer links). Matches the landing page's frosted-glass look.
 */
export default function PlaceholderPage({ title }: PlaceholderPageProps) {
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
        <span aria-hidden="true" className={`mx-auto h-10 w-10 text-base ${LOGO_MARK}`}>
          M
        </span>
        <h1 className="mt-4 text-2xl font-medium tracking-tight text-neutral-900">
          {title}
        </h1>
        <p className="mt-2 text-sm text-neutral-500">
          This page is coming soon. The landing page links here so the flow is in
          place.
        </p>
        <Link to="/" className={`${BTN_PRIMARY} mt-6`}>
          Back to home
        </Link>
      </motion.div>
    </div>
  )
}
