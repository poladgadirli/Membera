import type { ComponentType, SVGProps } from 'react'
import { CardIcon, QrIcon, StorefrontIcon } from './icons'

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
  return (
    <section id="how-it-works" className="border-t border-line bg-surface">
      <div className="mx-auto max-w-6xl px-4 py-16 sm:px-6 sm:py-24 lg:px-8">
        <div className="mx-auto max-w-2xl text-center">
          <p className="text-sm font-semibold uppercase tracking-wide text-primary">
            How it works
          </p>
          <h2 className="mt-3 font-display text-3xl text-ink sm:text-4xl">
            Three steps from sign-up to scan
          </h2>
          <p className="mt-4 text-lg text-body">
            No point-of-sale integration and no extra hardware — just a plan, a
            subscriber, and a QR code.
          </p>
        </div>

        <ol className="mt-14 grid gap-8 sm:grid-cols-3">
          {steps.map((step, index) => (
            <li key={step.title} className="relative flex flex-col">
              <div className="flex items-center gap-4">
                <span className="grid h-12 w-12 shrink-0 place-items-center rounded-xl bg-primary/10 text-primary">
                  <step.Icon className="h-6 w-6" />
                </span>
                <span className="font-display text-2xl text-line">
                  {String(index + 1).padStart(2, '0')}
                </span>
              </div>
              <h3 className="mt-5 text-lg font-semibold text-ink">
                {step.title}
              </h3>
              <p className="mt-2 text-body">{step.description}</p>
            </li>
          ))}
        </ol>
      </div>
    </section>
  )
}
