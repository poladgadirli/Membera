import { Link } from 'react-router-dom'
import { ArrowRightIcon, CheckIcon, QrIcon } from './icons'

const highlights = ['No hardware to install', 'Works on any phone', 'Live in a day']

export default function Hero() {
  return (
    <section className="relative overflow-hidden">
      <div
        aria-hidden="true"
        className="pointer-events-none absolute inset-x-0 -top-40 -z-10 flex justify-center blur-3xl"
      >
        <div className="h-72 w-[48rem] max-w-full bg-gradient-to-tr from-primary/20 via-accent/15 to-primary/10 opacity-70 [clip-path:ellipse(60%_50%_at_50%_50%)]" />
      </div>

      <div className="mx-auto grid max-w-6xl gap-12 px-4 pb-16 pt-14 sm:px-6 sm:pb-20 sm:pt-20 lg:grid-cols-[1.1fr_0.9fr] lg:items-center lg:gap-8 lg:px-8">
        <div>
          <span className="inline-flex items-center gap-2 rounded-full border border-line bg-surface px-3 py-1 text-xs font-medium text-body">
            <span className="h-1.5 w-1.5 rounded-full bg-accent" />
            Subscriptions for restaurants, salons &amp; local services
          </span>

          <h1 className="mt-5 font-display text-4xl leading-[1.1] text-ink sm:text-5xl lg:text-6xl">
            Sell subscriptions your customers redeem with a QR&nbsp;code.
          </h1>

          <p className="mt-5 max-w-xl text-lg leading-relaxed text-body">
            Membera gives local businesses everything they need to launch
            subscription plans — a daily coffee, a monthly cut, a weekly class —
            and lets customers redeem each visit in seconds with a single scan.
          </p>

          <div className="mt-8 flex flex-col gap-3 sm:flex-row sm:items-center">
            <Link
              to="/signup"
              className="inline-flex items-center justify-center gap-2 rounded-xl bg-accent px-6 py-3.5 text-base font-semibold text-on-accent shadow-sm transition-colors hover:bg-accent-hover focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-accent"
            >
              Get Started
              <ArrowRightIcon className="h-5 w-5" />
            </Link>
            <Link
              to="/login"
              className="inline-flex items-center justify-center rounded-xl border border-line bg-surface px-6 py-3.5 text-base font-semibold text-ink transition-colors hover:bg-muted focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
            >
              Log In
            </Link>
          </div>

          <ul className="mt-8 flex flex-wrap gap-x-6 gap-y-2">
            {highlights.map((item) => (
              <li key={item} className="flex items-center gap-2 text-sm text-body">
                <CheckIcon className="h-4 w-4 text-primary" />
                {item}
              </li>
            ))}
          </ul>
        </div>

        <div className="relative">
          <div className="mx-auto max-w-sm rounded-3xl border border-line bg-surface p-6 shadow-xl shadow-primary/5">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-semibold text-ink">Bloom Coffee</p>
                <p className="text-xs text-body">Daily Espresso Plan</p>
              </div>
              <span className="rounded-full bg-primary/10 px-2.5 py-1 text-xs font-medium text-primary">
                Active
              </span>
            </div>

            <div className="mt-6 grid place-items-center rounded-2xl bg-canvas py-8">
              <QrIcon className="h-28 w-28 text-ink" />
            </div>

            <div className="mt-6 flex items-center justify-between text-sm">
              <span className="text-body">Redemptions this month</span>
              <span className="font-semibold text-ink">18 / 30</span>
            </div>
            <div className="mt-2 h-2 overflow-hidden rounded-full bg-muted">
              <div className="h-full w-3/5 rounded-full bg-accent" />
            </div>
          </div>
        </div>
      </div>
    </section>
  )
}
