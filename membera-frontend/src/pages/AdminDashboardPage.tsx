import { useCallback, useEffect, useMemo, useState } from 'react'
import { ConfirmDialog } from '@/components/ConfirmDialog'
import { DashboardShell } from '@/components/DashboardShell'
import { PageHeading } from '@/components/PageHeading'
import { Spinner } from '@/components/Spinner'
import { useAuth } from '@/hooks/useAuth'
import {
  ApiError,
  deleteAdminAccount,
  deleteUser,
  demoteAdmin,
  getAllUsers,
  promoteToAdmin,
  type AdminUser,
} from '@/lib/admin'
import { CARD, ERROR_BANNER, SECTION_LABEL } from '@/lib/ui'

type Status = 'loading' | 'ready' | 'error'

type PendingAction = {
  user: AdminUser
  kind: 'delete-user' | 'promote' | 'demote' | 'delete-admin'
}

const ACTION_COPY: Record<
  PendingAction['kind'],
  { title: string; verb: string; destructive: boolean; run: (id: string) => Promise<void> }
> = {
  'delete-user': {
    title: 'Delete this user?',
    verb: 'Delete user',
    destructive: true,
    run: deleteUser,
  },
  promote: {
    title: 'Promote to Admin?',
    verb: 'Promote',
    destructive: false,
    run: promoteToAdmin,
  },
  demote: {
    title: 'Demote this admin to User?',
    verb: 'Demote',
    destructive: true,
    run: demoteAdmin,
  },
  'delete-admin': {
    title: 'Delete this admin account?',
    verb: 'Delete admin',
    destructive: true,
    run: deleteAdminAccount,
  },
}

export default function AdminDashboardPage() {
  const { user: currentUser } = useAuth()
  const isSuperAdmin = currentUser?.role === 'SuperAdmin'
  const currentUserId = currentUser?.userId ?? ''

  const [status, setStatus] = useState<Status>('loading')
  const [users, setUsers] = useState<AdminUser[]>([])
  const [loadError, setLoadError] = useState<string | null>(null)
  const [pending, setPending] = useState<PendingAction | null>(null)

  const load = useCallback(async () => {
    try {
      const data = await getAllUsers()
      setUsers(data)
      setStatus('ready')
    } catch (err) {
      setLoadError(
        err instanceof ApiError ? err.message : 'Could not load users.',
      )
      setStatus('error')
    }
  }, [])

  useEffect(() => {
    let active = true
    getAllUsers().then(
      (data) => {
        if (!active) return
        setUsers(data)
        setStatus('ready')
      },
      (err) => {
        if (!active) return
        setLoadError(
          err instanceof ApiError ? err.message : 'Could not load users.',
        )
        setStatus('error')
      },
    )
    return () => {
      active = false
    }
  }, [])

  const sorted = useMemo(
    () =>
      [...users].sort((a, b) =>
        `${a.firstName} ${a.lastName}`.localeCompare(
          `${b.firstName} ${b.lastName}`,
        ),
      ),
    [users],
  )

  const actionsFor = (target: AdminUser): PendingAction['kind'][] => {
    if (target.id === currentUserId) return []
    if (target.isDeleted) return []

    const isAdmin = target.role === 'Admin'
    const isSuper = target.role === 'SuperAdmin'
    if (isSuper) return []

    if (isAdmin) {
      // Admin targets: SuperAdmin only.
      return isSuperAdmin ? ['demote', 'delete-admin'] : []
    }

    // User / MerchantOwner targets.
    const list: PendingAction['kind'][] = ['delete-user']
    if (isSuperAdmin) list.push('promote')
    return list
  }

  return (
    <DashboardShell>
      <PageHeading
        eyebrow="Admin"
        title="User management"
        description={
          isSuperAdmin
            ? 'Every account on Membera. As a super administrator you can promote, demote, and remove admins as well as users.'
            : 'Every account on Membera. You can remove standard user accounts; admin accounts are managed by a super administrator.'
        }
      />

      <section aria-labelledby="users-heading" className="mt-10">
        <h2 id="users-heading" className={SECTION_LABEL}>
          All users {status === 'ready' && `(${users.length})`}
        </h2>

        <div className="mt-3">
          {status === 'loading' && (
            <div className={`${CARD} p-6`}>
              <div className="flex items-center gap-3 text-sm text-neutral-500">
                <Spinner /> Loading users…
              </div>
            </div>
          )}

          {status === 'error' && (
            <div className={`${CARD} p-6`}>
              <div className={ERROR_BANNER}>{loadError}</div>
            </div>
          )}

          {status === 'ready' && (
            <div className={`${CARD} overflow-hidden`}>
              <div className="overflow-x-auto">
                <table className="w-full min-w-[40rem] text-left text-sm">
                  <thead>
                    <tr className="border-b border-neutral-200/70 text-xs uppercase tracking-wide text-neutral-500">
                      <th className="px-4 py-3 font-medium">User</th>
                      <th className="px-4 py-3 font-medium">Role</th>
                      <th className="px-4 py-3 font-medium">Status</th>
                      <th className="px-4 py-3 font-medium">Registered</th>
                      <th className="px-4 py-3 text-right font-medium">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {sorted.map((target) => {
                      const kinds = actionsFor(target)
                      const isSelf = target.id === currentUserId
                      return (
                        <tr
                          key={target.id}
                          className="border-b border-neutral-100 last:border-0"
                        >
                          <td className="px-4 py-3">
                            <div className="font-medium text-neutral-900">
                              {[target.firstName, target.lastName]
                                .filter(Boolean)
                                .join(' ') || '—'}
                              {isSelf && (
                                <span className="ml-2 rounded bg-neutral-100 px-1.5 py-0.5 text-[11px] font-medium text-neutral-500">
                                  You
                                </span>
                              )}
                            </div>
                            <div className="text-xs text-neutral-500">
                              {target.email}
                            </div>
                          </td>
                          <td className="px-4 py-3 text-neutral-700">
                            {target.role}
                          </td>
                          <td className="px-4 py-3">
                            {target.isDeleted ? (
                              <span className="inline-flex items-center gap-1.5 rounded-full border border-red-200 bg-red-50 px-2.5 py-0.5 text-xs font-medium text-red-700">
                                <span className="h-1.5 w-1.5 rounded-full bg-red-500" />
                                Deleted
                              </span>
                            ) : (
                              <span className="inline-flex items-center gap-1.5 rounded-full border border-blue-200 bg-blue-50 px-2.5 py-0.5 text-xs font-medium text-blue-700">
                                <span className="h-1.5 w-1.5 rounded-full bg-blue-500" />
                                Active
                              </span>
                            )}
                          </td>
                          <td className="px-4 py-3 text-neutral-500">
                            {formatDate(target.createdAt)}
                          </td>
                          <td className="px-4 py-3">
                            <div className="flex flex-wrap justify-end gap-2">
                              {kinds.length === 0 ? (
                                <span className="text-xs text-neutral-400">
                                  —
                                </span>
                              ) : (
                                kinds.map((kind) => (
                                  <button
                                    key={kind}
                                    type="button"
                                    onClick={() =>
                                      setPending({ user: target, kind })
                                    }
                                    className={
                                      ACTION_COPY[kind].destructive
                                        ? 'rounded-lg border border-red-200 bg-white px-2.5 py-1.5 text-xs font-semibold text-red-600 shadow-sm transition hover:bg-red-50 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-red-500'
                                        : 'rounded-lg border border-neutral-200 bg-white px-2.5 py-1.5 text-xs font-semibold text-neutral-900 shadow-sm transition hover:bg-neutral-50 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-500'
                                    }
                                  >
                                    {ACTION_COPY[kind].verb}
                                  </button>
                                ))
                              )}
                            </div>
                          </td>
                        </tr>
                      )
                    })}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </div>
      </section>

      {pending && (
        <ConfirmDialog
          open
          title={ACTION_COPY[pending.kind].title}
          description={describe(pending)}
          confirmLabel={ACTION_COPY[pending.kind].verb}
          destructive={ACTION_COPY[pending.kind].destructive}
          onConfirm={async () => {
            await ACTION_COPY[pending.kind].run(pending.user.id)
            await load()
          }}
          onClose={() => setPending(null)}
        />
      )}
    </DashboardShell>
  )
}

function describe(pending: PendingAction): string {
  const name =
    [pending.user.firstName, pending.user.lastName].filter(Boolean).join(' ') ||
    pending.user.email

  switch (pending.kind) {
    case 'delete-user':
      return `${name}'s account will be deactivated and their sessions revoked. This cannot be undone from here.`
    case 'promote':
      return `${name} will gain Admin access, including user management.`
    case 'demote':
      return `${name} will lose Admin access and become a standard User. Their sessions will be revoked.`
    case 'delete-admin':
      return `${name}'s admin account will be deactivated and their sessions revoked. This cannot be undone from here.`
  }
}

function formatDate(iso: string): string {
  const time = Date.parse(iso)
  if (!Number.isFinite(time)) return '—'
  return new Date(time).toLocaleDateString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}
