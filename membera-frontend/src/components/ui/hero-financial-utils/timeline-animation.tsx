import type { ReactNode, RefObject } from 'react'
import { motion, useInView } from 'motion/react'

type TimelineTag =
  | 'div'
  | 'section'
  | 'header'
  | 'span'
  | 'p'
  | 'h1'
  | 'h2'
  | 'h3'
  | 'h4'
  | 'a'
  | 'ul'
  | 'ol'
  | 'li'
  | 'button'
  | 'img'

interface TimelineAnimationProps {
  /** Element to render. Defaults to `div`. */
  as?: TimelineTag
  /** Stagger index — higher numbers reveal later. */
  animationNum: number
  /** Ref to the scroll container that triggers the reveal. */
  timelineRef: RefObject<HTMLElement | null>
  className?: string
  children?: ReactNode
  /** Passthrough for tag-specific props such as `src` / `alt` on `img`. */
  [key: string]: unknown
}

/**
 * Reveals its content with a blur + slide-up transition once `timelineRef`
 * scrolls into view. `animationNum` controls the stagger delay.
 */
export const TimelineAnimation = ({
  as = 'div',
  animationNum,
  timelineRef,
  className,
  children,
  ...rest
}: TimelineAnimationProps) => {
  const isInView = useInView(timelineRef, {
    once: true,
    margin: '0px 0px -15% 0px',
  })

  const MotionTag = motion[as] as typeof motion.div
  const delay = Math.max(0, animationNum) * 0.12

  return (
    <MotionTag
      className={className}
      initial={{ opacity: 0, y: 28, filter: 'blur(8px)' }}
      animate={
        isInView
          ? { opacity: 1, y: 0, filter: 'blur(0px)' }
          : { opacity: 0, y: 28, filter: 'blur(8px)' }
      }
      transition={{ duration: 0.6, delay, ease: [0.22, 1, 0.36, 1] }}
      {...rest}
    >
      {children}
    </MotionTag>
  )
}
