// Shared motion presets for the whole app, following the "Designing Fluid
// Interfaces" (WWDC 2018) model: springs are described by damping (overshoot)
// and response (seconds to reach target), not by duration/easing curves.
// Motion's `{ type: 'spring', bounce, duration }` API maps to that directly —
// bounce 0 = damping 1.0 (critically damped, no overshoot), duration = response.
// Every spring in the app should come from here so there's one deliberate,
// defensible set of numbers instead of ad hoc values per component.

import { useEffect, useState } from 'react'
import type { Transition, Variants } from 'motion/react'

/** Default UI spring — dialogs, cards, overlays, route fades. Critically
 * damped: settles smoothly with no overshoot, appropriate for anything that
 * wasn't set in motion by a user's flick or drag. */
export const SPRING_UI: Transition = { type: 'spring', bounce: 0, duration: 0.3 }

/** Heavier surfaces (drawers, sheets) — same critically-damped feel, slightly
 * slower response since bigger surfaces should read as "thicker". */
export const SPRING_SHEET: Transition = { type: 'spring', bounce: 0, duration: 0.4 }

/** Momentum / decorative entrances only — a little overshoot. Reserve for
 * one-shot reveals (e.g. a cascading entrance), never for input-driven UI
 * a user can grab and redirect. */
export const SPRING_MOMENTUM: Transition = { type: 'spring', bounce: 0.2, duration: 0.35 }

/** Snappy micro-interactions — a label swap, a small confirmation pop. */
export const SPRING_SNAPPY: Transition = { type: 'spring', bounce: 0, duration: 0.2 }

/** Fast, no-overshoot fallback for `prefers-reduced-motion: reduce`. */
const REDUCED_MOTION_TRANSITION: Transition = { type: 'tween', duration: 0.15, ease: 'linear' }

/** Swaps in the reduced-motion-safe transition when the user has asked for
 * less motion — CSS's own `@media (prefers-reduced-motion)` block doesn't
 * reach Motion's JS-driven springs, so call sites need this explicitly. */
export function motionSafe(transition: Transition, reduced: boolean): Transition {
  return reduced ? REDUCED_MOTION_TRANSITION : transition
}

/** Tracks `prefers-reduced-motion: reduce`, live. */
export function usePrefersReducedMotion(): boolean {
  const [reduced, setReduced] = useState(
    () =>
      typeof window !== 'undefined' &&
      window.matchMedia('(prefers-reduced-motion: reduce)').matches,
  )

  useEffect(() => {
    const query = window.matchMedia('(prefers-reduced-motion: reduce)')
    const onChange = () => setReduced(query.matches)
    query.addEventListener('change', onChange)
    return () => query.removeEventListener('change', onChange)
  }, [])

  return reduced
}

/** Simple opacity cross-fade — route transitions, plain show/hide. */
export const fadeVariants: Variants = {
  initial: { opacity: 0 },
  animate: { opacity: 1 },
  exit: { opacity: 0 },
}

/** "Materialize" a glass/translucent surface: opacity, scale, and blur move
 * together so it reads as a real material arriving, not a flat fade. Used for
 * modal panels, loading overlays, and state-swap reveals. */
export const materializeVariants: Variants = {
  initial: { opacity: 0, scale: 0.96, filter: 'blur(6px)' },
  animate: { opacity: 1, scale: 1, filter: 'blur(0px)' },
  exit: { opacity: 0, scale: 0.96, filter: 'blur(6px)' },
}

/** Fade + blur + rise — the sign-in/sign-up cascading field-by-field reveal
 * (replaces the old `animate-element` CSS keyframe). */
export const elementVariants: Variants = {
  initial: { opacity: 0, y: 12, filter: 'blur(4px)' },
  animate: { opacity: 1, y: 0, filter: 'blur(0px)' },
}

/** Fade + blur + slide-in from the right — the auth hero image panel
 * (replaces `animate-slide-right`). */
export const slideRightVariants: Variants = {
  initial: { opacity: 0, x: 28, filter: 'blur(6px)' },
  animate: { opacity: 1, x: 0, filter: 'blur(0px)' },
}

/** Fade + blur + rise + scale — testimonial cards (replaces `animate-testimonial`). */
export const testimonialVariants: Variants = {
  initial: { opacity: 0, y: 16, scale: 0.98, filter: 'blur(6px)' },
  animate: { opacity: 1, scale: 1, y: 0, filter: 'blur(0px)' },
}
