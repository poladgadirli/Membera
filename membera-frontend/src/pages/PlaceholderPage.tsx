import { Link } from 'react-router-dom'

type PlaceholderPageProps = {
  title: string
}

/**
 * Temporary stand-in for routes that are linked from the landing page but not
 * built yet (login, sign-up, and the footer links).
 */
export default function PlaceholderPage({ title }: PlaceholderPageProps) {
  return (
    <div className="grid min-h-screen place-items-center bg-canvas px-4">
      <div className="w-full max-w-md rounded-2xl border border-line bg-surface p-8 text-center shadow-sm">
        <span
          aria-hidden="true"
          className="mx-auto grid h-10 w-10 place-items-center rounded-lg bg-primary font-display text-base text-on-primary"
        >
          M
        </span>
        <h1 className="mt-4 font-display text-2xl text-ink">{title}</h1>
        <p className="mt-2 text-sm text-body">
          This page is coming soon. The landing page links here so the flow is in
          place.
        </p>
        <Link
          to="/"
          className="mt-6 inline-flex items-center justify-center rounded-lg bg-primary px-4 py-2.5 text-sm font-semibold text-on-primary transition-colors hover:bg-primary-hover focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
        >
          Back to home
        </Link>
      </div>
    </div>
  )
}
