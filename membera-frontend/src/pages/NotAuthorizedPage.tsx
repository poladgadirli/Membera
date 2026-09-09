import { Link } from 'react-router-dom'
import { useAuth } from '@/hooks/useAuth'

export default function NotAuthorizedPage() {
  const { isAuthenticated } = useAuth()

  return (
    <div className="grid min-h-screen place-items-center bg-background px-4">
      <div className="w-full max-w-md rounded-2xl border border-border bg-card p-8 text-center shadow-sm">
        <span
          aria-hidden="true"
          className="mx-auto grid h-10 w-10 place-items-center rounded-lg bg-destructive/10 text-lg font-semibold text-destructive"
        >
          !
        </span>
        <h1 className="mt-4 text-2xl font-medium tracking-tight text-foreground">
          You don&rsquo;t have access to this page
        </h1>
        <p className="mt-2 text-sm text-muted-foreground">
          Your account role doesn&rsquo;t permit viewing this area. If you think
          this is a mistake, contact your administrator.
        </p>
        <Link
          to={isAuthenticated ? '/dashboard' : '/login'}
          className="mt-6 inline-flex items-center justify-center rounded-lg bg-primary px-4 py-2.5 text-sm font-semibold text-primary-foreground transition-colors hover:bg-primary/90 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring"
        >
          {isAuthenticated ? 'Back to dashboard' : 'Go to sign in'}
        </Link>
      </div>
    </div>
  )
}
