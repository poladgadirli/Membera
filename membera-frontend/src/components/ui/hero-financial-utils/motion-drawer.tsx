import { useEffect, useState, type ReactNode } from 'react'
import { AnimatePresence, motion } from 'motion/react'
import { Menu, X } from 'lucide-react'
import { cn } from '@/lib/utils'
import { SPRING_SHEET } from '@/lib/motion'

interface MotionDrawerProps {
  children: ReactNode
  direction?: 'left' | 'right'
  /** Panel width in pixels. */
  width?: number
  backgroundColor?: string
  /** Class for the close ("X") button inside the panel. */
  clsBtnClassName?: string
  /** Class for the sliding panel. */
  contentClassName?: string
  /** Class for the trigger button. */
  btnClassName?: string
}

/**
 * A minimal slide-in navigation drawer built on `motion`.
 * Opens from `direction`, dims the page behind it, and closes on
 * overlay click or Escape.
 */
export default function MotionDrawer({
  children,
  direction = 'left',
  width = 300,
  backgroundColor = '#ffffff',
  clsBtnClassName,
  contentClassName,
  btnClassName,
}: MotionDrawerProps) {
  const [open, setOpen] = useState(false)
  const offscreen = direction === 'left' ? -width : width

  useEffect(() => {
    if (!open) return
    const onKey = (e: KeyboardEvent) => e.key === 'Escape' && setOpen(false)
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [open])

  return (
    <>
      <button
        type="button"
        aria-label="Open menu"
        className={btnClassName}
        onClick={() => setOpen(true)}
      >
        <Menu size={20} />
      </button>

      <AnimatePresence>
        {open && (
          <>
            <motion.div
              className="fixed inset-0 z-40 bg-black/40"
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              onClick={() => setOpen(false)}
            />
            <motion.aside
              className={cn(
                'fixed top-0 z-50 h-full p-6',
                direction === 'left' ? 'left-0' : 'right-0',
                contentClassName,
              )}
              style={{ width, backgroundColor }}
              initial={{ x: offscreen }}
              animate={{ x: 0 }}
              exit={{ x: offscreen }}
              transition={SPRING_SHEET}
            >
              <button
                type="button"
                aria-label="Close menu"
                className={cn(
                  'absolute top-4 rounded-full p-1',
                  direction === 'left' ? 'right-4' : 'left-4',
                  clsBtnClassName,
                )}
                onClick={() => setOpen(false)}
              >
                <X size={18} />
              </button>
              <div className="mt-10">{children}</div>
            </motion.aside>
          </>
        )}
      </AnimatePresence>
    </>
  )
}
