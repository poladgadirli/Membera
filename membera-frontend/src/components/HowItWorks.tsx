import { useRef } from 'react'
import type { ComponentType, SVGProps } from 'react'
import { CardIcon, QrIcon, StorefrontIcon } from './icons'
import { TimelineAnimation } from '@/components/ui/hero-financial-utils/timeline-animation'

type Step = {
  title: string
  description: string
  Icon: ComponentType<SVGProps<SVGSVGElement>>
}

const steps: Step[] = [
  {
    title: 'Businesses create plans',
    description:
      'A merchant sets up a subscription — price, what it includes, and how often it can be redeemed.',
    Icon: StorefrontIcon,
  },
  {
    title: 'Customers subscribe',
    description:
      'Customers browse local plans, subscribe to their favorites, and get a personal membership.',
    Icon: CardIcon,
  },
  {
    title: 'Customers redeem via QR code',
    description:
      'On each visit the customer shows their QR code, the business scans it, and the redemption is logged instantly.',
    Icon: QrIcon,
  },
]

export default function HowItWorks() {
  const sectionRef = useRef<HTMLElement>(null)

  return (
    <section
      ref={sectionRef}
      id="how-it-works"
      className="relative overflow-hidden bg-[#f7f9fc] py-20 sm:py-28"
    >
      <div
        aria-hidden="true"
        className="pointer-events-none absolute inset-x-0 top-0 h-72 bg-linear-to-b from-blue-100/70 via-blue-50/40 to-transparent"
      />

      <div className="relative z-10 mx-auto max-w-6xl px-4 sm:px-6 lg:px-8">
        <div className="mx-auto flex max-w-2xl flex-col items-center gap-5 text-center">
          <TimelineAnimation
            animationNum={0}
            timelineRef={sectionRef}
            className="inline-flex w-fit items-center gap-2 rounded-full border-2 border-white bg-white px-1.5 py-1 text-black shadow-lg shadow-blue-500/20"
          >
            <span className="rounded-full bg-linear-to-br from-blue-500 to-blue-200 px-2 py-0.5 text-xs font-medium uppercase tracking-widest text-white">
              Flow
            </span>
            <span className="text-sm font-medium">How it works</span>
          </TimelineAnimation>

          <TimelineAnimation
            as="h2"
            animationNum={1}
            timelineRef={sectionRef}
            className="text-4xl font-medium tracking-tight text-neutral-900 sm:text-5xl"
          >
            Three steps from sign-up to scan
          </TimelineAnimation>

          <TimelineAnimation
            as="p"
            animationNum={2}
            timelineRef={sectionRef}
            className="max-w-xl text-lg font-medium text-neutral-500"
          >
            No point-of-sale integration and no extra hardware — just a plan, a
            subscriber, and a QR code.
          </TimelineAnimation>
        </div>

        <ol className="mt-16 grid gap-6 sm:grid-cols-3">
          {steps.map((step, index) => (
            <TimelineAnimation
              as="li"
              key={step.title}
              animationNum={3 + index}
              timelineRef={sectionRef}
              className="relative flex flex-col rounded-2xl border border-white bg-white/70 p-6 shadow-sm backdrop-blur-xl"
            >
              <div className="flex items-center justify-between">
                <span className="grid h-12 w-12 shrink-0 place-items-center rounded-xl bg-linear-to-br from-blue-500 via-blue-400 to-blue-200 text-white shadow-sm shadow-blue-500/30">
                  <step.Icon className="h-6 w-6" />
                </span>
                <span className="text-2xl font-medium text-neutral-300">
                  {String(index + 1).padStart(2, '0')}
                </span>
              </div>
              <h3 className="mt-5 text-lg font-semibold text-neutral-900">
                {step.title}
              </h3>
              <p className="mt-2 text-neutral-500">{step.description}</p>
            </TimelineAnimation>
          ))}
        </ol>
      </div>
    </section>
  )
}
