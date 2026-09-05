import { Link } from 'react-router-dom'
import { ChartIcon, QrIcon, StorefrontIcon, UsersIcon } from './icons'

const audiences = [
  {
    label: 'For businesses',
    heading: 'Turn regulars into recurring revenue',
    accent: 'text-primary',
    chip: 'bg-primary/10 text-primary',
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
    accent: 'text-accent',
    chip: 'bg-accent/10 text-accent',
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

export default function WhoItsFor() {
  return (
    <section id="who-its-for" className="border-t border-line bg-canvas">
      <div className="mx-auto max-w-6xl px-4 py-16 sm:px-6 sm:py-24 lg:px-8">
        <div className="mx-auto max-w-2xl text-center">
          <p className="text-sm font-semibold uppercase tracking-wide text-primary">
            Who it&apos;s for
          </p>
          <h2 className="mt-3 font-display text-3xl text-ink sm:text-4xl">
            One platform, both sides of the counter
          </h2>
        </div>

        <div className="mt-14 grid gap-6 lg:grid-cols-2">
          {audiences.map((audience) => (
            <div
              key={audience.label}
              className="flex flex-col rounded-2xl border border-line bg-surface p-8 shadow-sm"
            >
              <span
                className={`inline-flex w-fit rounded-full px-3 py-1 text-xs font-semibold ${audience.chip}`}
              >
                {audience.label}
              </span>
              <h3 className="mt-4 font-display text-2xl text-ink">
                {audience.heading}
              </h3>

              <ul className="mt-6 flex-1 space-y-5">
                {audience.points.map((point) => (
                  <li key={point.title} className="flex gap-4">
                    <span
                      className={`mt-0.5 grid h-10 w-10 shrink-0 place-items-center rounded-lg bg-canvas ${audience.accent}`}
                    >
                      <point.Icon className="h-5 w-5" />
                    </span>
                    <div>
                      <p className="font-semibold text-ink">{point.title}</p>
                      <p className="mt-1 text-sm text-body">{point.text}</p>
                    </div>
                  </li>
                ))}
              </ul>

              <Link
                to={audience.cta.to}
                className={`mt-8 inline-flex w-fit items-center gap-2 rounded-lg border border-line bg-surface px-4 py-2.5 text-sm font-semibold ${audience.accent} transition-colors hover:bg-muted focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary`}
              >
                {audience.cta.label}
              </Link>
            </div>
          ))}
        </div>
      </div>
    </section>
  )
}
