import { Link } from 'react-router-dom'

type PlaceholderPageProps = {
  title: string
}

/**
 * Temporary stand-in for routes that are linked from the landing page but not
 * built yet (sign-up and the footer links).
 */
export default function PlaceholderPage({ title }: PlaceholderPageProps) {
  return (
    <div className="grid min-h-screen place-items-center bg-background px-4">
      <div className="w-full max-w-md rounded-2xl border border-border bg-card p-8 text-center shadow-sm">
        <span
          aria-hidden="true"
          className="mx-auto grid h-10 w-10 place-items-center rounded-lg bg-primary text-base font-semibold text-primary-foreground"
        >
          M
        </span>
        <h1 className="mt-4 text-2xl font-medium tracking-tight text-foreground">
          {title}
        </h1>
        <p className="mt-2 text-sm text-muted-foreground">
          This page is coming soon. The landing page links here so the flow is in
          place.
        </p>
        <Link
          to="/"
          className="mt-6 inline-flex items-center justify-center rounded-lg bg-primary px-4 py-2.5 text-sm font-semibold text-primary-foreground transition-colors hover:bg-primary/90 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring"
        >
          Back to home
        </Link>
      </div>
    </div>
  )
}
