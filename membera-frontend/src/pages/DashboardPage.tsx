import { DashboardShell } from '@/components/DashboardShell'
import { PageHeading } from '@/components/PageHeading'
import { useAuth } from '@/hooks/useAuth'
import { CARD } from '@/lib/ui'
import AdminDashboardPage from '@/pages/AdminDashboardPage'
import MerchantDashboardPage from '@/pages/MerchantDashboardPage'
import UserDashboardPage from '@/pages/UserDashboardPage'

const ROLE_LABELS: Record<string, string> = {
  User: 'Member',
  MerchantOwner: 'Merchant owner',
  Admin: 'Administrator',
  SuperAdmin: 'Super administrator',
}

/**
 * `/dashboard` is the single entry point for every signed-in user. It renders
 * the dashboard for their role. Unknown / missing roles fall through to a
 * minimal account summary.
 */
export default function DashboardPage() {
  const { user } = useAuth()

  switch (user?.role) {
    case 'MerchantOwner':
      return <MerchantDashboardPage />
    case 'Admin':
    case 'SuperAdmin':
      return <AdminDashboardPage />
    case 'User':
      return <UserDashboardPage />
    default:
      return <GenericDashboard />
  }
}

function GenericDashboard() {
  const { user } = useAuth()

  const firstName = user?.firstName?.trim() || 'there'
  const role = user?.role ?? 'User'
  const roleLabel = ROLE_LABELS[role] ?? role

  return (
    <DashboardShell>
      <PageHeading
        eyebrow="Dashboard"
        title={`Welcome, ${firstName}!`}
        description={
          <>
            You&rsquo;re signed in as{' '}
            <span className="font-semibold text-neutral-900">{roleLabel}</span>.
          </>
        }
      />

      <dl className="mt-10 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <div className={`${CARD} p-5`}>
          <dt className="text-xs font-medium uppercase tracking-wide text-neutral-500">
            Name
          </dt>
          <dd className="mt-1 text-sm font-medium text-neutral-900">
            {[user?.firstName, user?.lastName].filter(Boolean).join(' ') || '—'}
          </dd>
        </div>
        <div className={`${CARD} p-5`}>
          <dt className="text-xs font-medium uppercase tracking-wide text-neutral-500">
            Email
          </dt>
          <dd className="mt-1 truncate text-sm font-medium text-neutral-900">
            {user?.email || '—'}
          </dd>
        </div>
        <div className={`${CARD} p-5`}>
          <dt className="text-xs font-medium uppercase tracking-wide text-neutral-500">
            Role
          </dt>
          <dd className="mt-1 text-sm font-medium text-neutral-900">{role}</dd>
        </div>
      </dl>
    </DashboardShell>
  )
}
