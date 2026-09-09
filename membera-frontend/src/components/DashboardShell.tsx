import type { ReactNode } from 'react'
import { useAuth } from '@/hooks/useAuth'

/**
 * Shared chrome for every authenticated page: the Membera header with a log-out
 * action, and a centered content column. Matches the header used on the auth
 * placeholder pages (border-b border-border bg-card, max-w-5xl, h-16).
 */
export function DashboardShell({ children }: { children: ReactNode }) {
  const { logout } = useAuth()

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
        {children}
      </main>
    </div>
  )
}
