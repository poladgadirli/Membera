import { Link } from 'react-router-dom'
import { ArrowRightIcon } from './icons'

export default function CtaBanner() {
  return (
    <section className="border-t border-line bg-surface">
      <div className="mx-auto max-w-6xl px-4 py-16 sm:px-6 sm:py-20 lg:px-8">
        <div className="overflow-hidden rounded-3xl bg-primary px-6 py-12 text-center sm:px-12 sm:py-16">
          <h2 className="font-display text-3xl text-on-primary sm:text-4xl">
            Launch your first plan this week
          </h2>
          <p className="mx-auto mt-4 max-w-xl text-on-primary/90">
            Set up a subscription, share it with your regulars, and start
            redeeming visits by QR code. It is free to explore.
          </p>
          <Link
            to="/signup"
            className="mt-8 inline-flex items-center justify-center gap-2 rounded-xl bg-accent px-6 py-3.5 text-base font-semibold text-on-accent shadow-sm transition-colors hover:bg-accent-hover focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white"
          >
            Get Started
            <ArrowRightIcon className="h-5 w-5" />
          </Link>
        </div>
      </div>
    </section>
  )
}
