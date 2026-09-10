import type { ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '@/hooks/useAuth'
import { BTN_SECONDARY_SM, PAGE_BG } from '@/lib/ui'

/**
 * Shared chrome for every authenticated page. Matches the landing page: the
 * #f7f9fc canvas with a soft blue wash at the top, a frosted sticky header with
 * the gradient "M" mark, and a centered max-w-6xl column.
 */
export function DashboardShell({ children }: { children: ReactNode }) {
  const { user, logout } = useAuth()

  const name = [user?.firstName, user?.lastName].filter(Boolean).join(' ')

  return (
    <div className={`relative flex min-h-screen flex-col overflow-hidden ${PAGE_BG}`}>
      <div
        aria-hidden="true"
        className="pointer-events-none absolute inset-x-0 top-0 h-80 bg-linear-to-b from-blue-100/70 via-blue-50/40 to-transparent"
      />

      <header className="sticky top-0 z-40 border-b border-neutral-200/70 bg-white/70 backdrop-blur-xl">
        <div className="mx-auto flex h-16 max-w-6xl items-center justify-between px-4 sm:px-6 lg:px-8">
          <Link
            to="/dashboard"
            className="flex items-center gap-2 rounded-md focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-blue-500"
          >
            <span
              aria-hidden="true"
              className="grid h-8 w-8 place-items-center rounded-lg bg-linear-to-br from-blue-500 via-blue-400 to-blue-200 text-sm font-bold text-white shadow-sm shadow-blue-500/30"
            >
              M
            </span>
            <span className="text-sm font-semibold tracking-tight text-neutral-900">
              Membera
            </span>
          </Link>

          <div className="flex items-center gap-3">
            {name && (
              <span className="hidden text-sm text-neutral-500 sm:inline">
                {name}
              </span>
            )}
            <button type="button" onClick={logout} className={BTN_SECONDARY_SM}>
              Log out
            </button>
          </div>
        </div>
      </header>

      <main className="relative z-10 mx-auto w-full max-w-6xl flex-1 px-4 py-12 sm:px-6 sm:py-16 lg:px-8">
        {children}
      </main>
    </div>
  )
}
