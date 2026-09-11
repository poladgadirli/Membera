import type { ReactNode } from 'react'
import { AnimatePresence, motion } from 'motion/react'
import { Spinner } from '@/components/Spinner'
import { SPRING_UI, motionSafe, usePrefersReducedMotion } from '@/lib/motion'

interface PageLoadingOverlayProps {
  /** True while a subsequent page of the same content is being fetched. */
  loading: boolean
  children: ReactNode
  className?: string
}

/**
 * Wraps paginated content (a table, a card grid) with a dimmed, materializing
 * state — opacity and blur move together, rather than a flat opacity fade —
 * plus a centered spinner, while the next page is in flight. Shared between
 * AdminDashboardPage and BrowsePlansPage, which previously duplicated this
 * pattern with plain `opacity-40 transition-opacity`.
 */
export function PageLoadingOverlay({
  loading,
  children,
  className,
}: PageLoadingOverlayProps) {
  const reduced = usePrefersReducedMotion()
  const transition = motionSafe(SPRING_UI, reduced)

  return (
    <div className={`relative ${className ?? ''}`}>
      <motion.div
        animate={
          loading
            ? { opacity: 0.4, filter: 'blur(1.5px)' }
            : { opacity: 1, filter: 'blur(0px)' }
        }
        transition={transition}
      >
        {children}
      </motion.div>
      <AnimatePresence>
        {loading && (
          <motion.div
            className="absolute inset-0 flex items-center justify-center"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={transition}
          >
            <Spinner />
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  )
}
