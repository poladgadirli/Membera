import { useRef } from 'react'
import type { ComponentType, SVGProps } from 'react'
import { Link } from 'react-router-dom'
import { ChartIcon, QrIcon, StorefrontIcon, UsersIcon } from './icons'
import { TimelineAnimation } from '@/components/ui/hero-financial-utils/timeline-animation'

type Point = {
  Icon: ComponentType<SVGProps<SVGSVGElement>>
  title: string
  text: string
}

type Audience = {
  label: string
  heading: string
  tone: 'blue' | 'dark'
  cta: { label: string; to: string }
  points: Point[]
}

const audiences: Audience[] = [
  {
    label: 'For businesses',
    heading: 'Turn regulars into recurring revenue',
    tone: 'blue',
    cta: { label: 'Start as a business', to: '/signup' },
    points: [
      {
        Icon: StorefrontIcon,
        title: 'Manage subscription plans',
        text: 'Create, price, and pause plans from one dashboard, and see every redemption as it happens.',
      },
      {
        Icon: ChartIcon,
        title: 'Build a customer base',
        text: 'Keep subscribers coming back with predictable value, and understand who your best members are.',
      },
    ],
  },
  {
    label: 'For customers',
    heading: 'Get more from the places you already love',
    tone: 'dark',
    cta: { label: 'Find a plan', to: '/signup' },
    points: [
      {
        Icon: UsersIcon,
        title: 'Subscribe to your favorite places',
        text: 'Pick up a plan from the cafe, gym, or salon you visit most and pay once instead of every time.',
      },
      {
        Icon: QrIcon,
        title: 'Redeem easily via QR',
        text: 'Your membership lives on your phone — show the code, get it scanned, and you are done.',
      },
    ],
  },
]

const chipClass: Record<Audience['tone'], string> = {
  blue: 'bg-linear-to-br from-blue-500 to-blue-200 text-white',
  dark: 'bg-neutral-900 text-white',
}

const iconClass: Record<Audience['tone'], string> = {
  blue: 'bg-linear-to-br from-blue-500 via-blue-400 to-blue-200 text-white shadow-sm shadow-blue-500/30',
  dark: 'bg-neutral-900 text-white',
}

const ctaClass: Record<Audience['tone'], string> = {
  blue: 'bg-linear-to-br from-blue-500 via-blue-400 to-blue-200 text-white border border-blue-300',
  dark: 'bg-neutral-900 text-white border border-neutral-800 shadow-[inset_2px_2px_5px_0px_rgba(0,0,0,0.5),inset_-2px_-2px_6px_1px_rgba(80,78,78,0.5)]',
}

export default function WhoItsFor() {
  const sectionRef = useRef<HTMLElement>(null)

  return (
    <section
      ref={sectionRef}
      id="who-its-for"
      className="relative overflow-hidden bg-[#f7f9fc] py-20 sm:py-28"
    >
      <div className="relative z-10 mx-auto max-w-6xl px-4 sm:px-6 lg:px-8">
        <div className="mx-auto flex max-w-2xl flex-col items-center gap-5 text-center">
          <TimelineAnimation
            animationNum={0}
            timelineRef={sectionRef}
            className="inline-flex w-fit items-center gap-2 rounded-full border-2 border-white bg-white px-1.5 py-1 text-black shadow-lg shadow-blue-500/20"
          >
            <span className="rounded-full bg-linear-to-br from-blue-500 to-blue-200 px-2 py-0.5 text-xs font-medium uppercase tracking-widest text-white">
              Audience
            </span>
            <span className="text-sm font-medium">Who it&apos;s for</span>
          </TimelineAnimation>

          <TimelineAnimation
            as="h2"
            animationNum={1}
            timelineRef={sectionRef}
            className="text-4xl font-medium tracking-tight text-neutral-900 sm:text-5xl"
          >
            One platform, both sides of the counter
          </TimelineAnimation>
        </div>

        <div className="mt-16 grid gap-6 lg:grid-cols-2">
          {audiences.map((audience, index) => (
            <TimelineAnimation
              key={audience.label}
              animationNum={2 + index}
              timelineRef={sectionRef}
              className="flex flex-col rounded-2xl border border-white bg-white/70 p-8 shadow-sm backdrop-blur-xl"
            >
              <span
                className={`inline-flex w-fit rounded-full px-3 py-1 text-xs font-medium uppercase tracking-widest ${chipClass[audience.tone]}`}
              >
                {audience.label}
              </span>
              <h3 className="mt-4 text-2xl font-medium tracking-tight text-neutral-900">
                {audience.heading}
              </h3>

              <ul className="mt-6 flex-1 space-y-5">
                {audience.points.map((point) => (
                  <li key={point.title} className="flex gap-4">
                    <span
                      className={`mt-0.5 grid h-10 w-10 shrink-0 place-items-center rounded-lg ${iconClass[audience.tone]}`}
                    >
                      <point.Icon className="h-5 w-5" />
                    </span>
                    <div>
                      <p className="font-semibold text-neutral-900">
                        {point.title}
                      </p>
                      <p className="mt-1 text-sm text-neutral-500">
                        {point.text}
                      </p>
                    </div>
                  </li>
                ))}
              </ul>

              <Link
                to={audience.cta.to}
                className={`mt-8 inline-flex w-fit items-center gap-2 rounded-lg px-4 py-2.5 text-sm font-semibold transition hover:brightness-105 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-500 ${ctaClass[audience.tone]}`}
              >
                {audience.cta.label}
              </Link>
            </TimelineAnimation>
          ))}
        </div>
      </div>
    </section>
  )
}
