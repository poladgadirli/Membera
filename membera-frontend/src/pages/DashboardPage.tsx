import { useAuth } from '@/hooks/useAuth'

const ROLE_LABELS: Record<string, string> = {
  User: 'Member',
  MerchantOwner: 'Merchant owner',
  Admin: 'Administrator',
  SuperAdmin: 'Super administrator',
}

export default function DashboardPage() {
  const { user, logout } = useAuth()

  const firstName = user?.firstName?.trim() || 'there'
  const role = user?.role ?? 'User'
  const roleLabel = ROLE_LABELS[role] ?? role

  return (
    <div className="flex min-h-screen flex-col bg-background">
      <header className="border-b border-border bg-card">
        <div className="mx-auto flex h-16 max-w-5xl items-center justify-between px-4 sm:px-6">
          <div className="flex items-center gap-2">
            <span
              aria-hidden="true"
              className="grid h-8 w-8 place-items-center rounded-lg bg-primary text-sm font-semibold text-primary-foreground"
            >
              M
            </span>
            <span className="text-sm font-semibold tracking-tight text-foreground">
              Membera
            </span>
          </div>
          <button
            type="button"
            onClick={logout}
            className="rounded-lg border border-border px-3 py-2 text-sm font-medium text-foreground transition-colors hover:bg-secondary focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring"
          >
            Log out
          </button>
        </div>
      </header>

      <main className="mx-auto w-full max-w-5xl flex-1 px-4 py-10 sm:px-6 sm:py-14">
        <p className="text-sm font-medium text-muted-foreground">Dashboard</p>
        <h1 className="mt-1 text-3xl font-semibold tracking-tight text-foreground">
          Welcome, {firstName}!
        </h1>
        <p className="mt-2 max-w-prose text-sm text-muted-foreground">
          You&rsquo;re signed in as{' '}
          <span className="font-medium text-foreground">{roleLabel}</span>. Your
          role-specific dashboard is coming next &mdash; this placeholder confirms
          the auth flow is working end to end.
        </p>

        <dl className="mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          <div className="rounded-2xl border border-border bg-card p-5">
            <dt className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
              Name
            </dt>
            <dd className="mt-1 text-sm font-medium text-foreground">
              {[user?.firstName, user?.lastName].filter(Boolean).join(' ') || '—'}
            </dd>
          </div>
          <div className="rounded-2xl border border-border bg-card p-5">
            <dt className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
              Email
            </dt>
            <dd className="mt-1 truncate text-sm font-medium text-foreground">
              {user?.email || '—'}
            </dd>
          </div>
          <div className="rounded-2xl border border-border bg-card p-5">
            <dt className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
              Role
            </dt>
            <dd className="mt-1 text-sm font-medium text-foreground">{role}</dd>
          </div>
        </dl>
      </main>
    </div>
  )
}
