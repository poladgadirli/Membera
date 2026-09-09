import { DashboardShell } from '@/components/DashboardShell'
import { useAuth } from '@/hooks/useAuth'
import MerchantDashboardPage from '@/pages/MerchantDashboardPage'

const ROLE_LABELS: Record<string, string> = {
  User: 'Member',
  MerchantOwner: 'Merchant owner',
  Admin: 'Administrator',
  SuperAdmin: 'Super administrator',
}

export default function DashboardPage() {
  const { user } = useAuth()

  // Role-specific dashboards. Merchant owners get the full experience; other
  // roles keep the generic placeholder until their dashboards are built.
  if (user?.role === 'MerchantOwner') {
    return <MerchantDashboardPage />
  }

  return <GenericDashboard />
}

function GenericDashboard() {
  const { user } = useAuth()

  const firstName = user?.firstName?.trim() || 'there'
  const role = user?.role ?? 'User'
  const roleLabel = ROLE_LABELS[role] ?? role

  return (
    <DashboardShell>
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
    </DashboardShell>
  )
}
