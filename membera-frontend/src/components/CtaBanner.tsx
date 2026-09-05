import { useRef } from 'react'
import { Link } from 'react-router-dom'
import { ArrowRightIcon } from './icons'
import { TimelineAnimation } from '@/components/ui/hero-financial-utils/timeline-animation'

export default function CtaBanner() {
  const sectionRef = useRef<HTMLElement>(null)

  return (
    <section
      ref={sectionRef}
      className="relative overflow-hidden bg-[#f7f9fc] py-20 sm:py-28"
    >
      <div className="mx-auto max-w-6xl px-4 sm:px-6 lg:px-8">
        <TimelineAnimation
          animationNum={0}
          timelineRef={sectionRef}
          className="relative overflow-hidden rounded-3xl border border-blue-300 bg-linear-to-br from-blue-600 via-blue-500 to-blue-300 px-6 py-14 text-center shadow-xl shadow-blue-500/25 sm:px-12 sm:py-20"
        >
          <div
            aria-hidden="true"
            className="pointer-events-none absolute -left-24 -top-24 h-72 w-72 rounded-full bg-white/25 blur-3xl"
          />
          <div
            aria-hidden="true"
            className="pointer-events-none absolute -bottom-32 -right-16 h-80 w-80 rounded-full bg-blue-200/40 blur-3xl"
          />

          <div className="relative z-10 flex flex-col items-center gap-5">
            <span className="rounded-full border-2 border-white/60 bg-white/20 px-2.5 py-0.5 text-xs font-medium uppercase tracking-widest text-white backdrop-blur">
              Get started
            </span>
            <h2 className="max-w-2xl text-3xl font-medium tracking-tight text-white sm:text-5xl">
              Launch your first plan this week
            </h2>
            <p className="mx-auto max-w-xl text-lg font-medium text-blue-50">
              Set up a subscription, share it with your regulars, and start
              redeeming visits by QR code. It is free to explore.
            </p>
            <Link
              to="/signup"
              className="mt-2 inline-flex items-center justify-center gap-2 rounded-xl bg-linear-to-br from-neutral-50 via-neutral-100 to-neutral-300 px-6 py-3.5 text-base font-semibold text-neutral-900 shadow-sm transition hover:brightness-105 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white"
            >
              Get Started
              <ArrowRightIcon className="h-5 w-5" />
            </Link>
          </div>
        </TimelineAnimation>
      </div>
    </section>
  )
}
