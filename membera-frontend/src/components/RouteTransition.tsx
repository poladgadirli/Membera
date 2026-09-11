import type { ReactNode } from 'react'
import { AnimatePresence, motion } from 'motion/react'
import { Routes, useLocation } from 'react-router-dom'
import { SPRING_UI, motionSafe, usePrefersReducedMotion } from '@/lib/motion'

/**
 * Cross-fades between routes instead of the hard cut a bare `<Routes>` gives
 * you. Deliberately opacity-only, no slide — pages here aren't spatially
 * adjacent panels, so a directional slide would imply a relationship that
 * doesn't exist, and a plain cross-fade is also the reduced-motion-safe
 * default, keeping the app to one motion path instead of two.
 */
export function RouteTransition({ children }: { children: ReactNode }) {
  const location = useLocation()
  const reduced = usePrefersReducedMotion()

  return (
    <AnimatePresence mode="wait" initial={false}>
      <motion.div
        key={location.pathname}
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        exit={{ opacity: 0 }}
        transition={motionSafe(SPRING_UI, reduced)}
      >
        <Routes location={location}>{children}</Routes>
      </motion.div>
    </AnimatePresence>
  )
}
