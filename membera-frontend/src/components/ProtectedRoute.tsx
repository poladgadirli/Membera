import type { ReactNode } from 'react'
import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '@/hooks/useAuth'
import type { UserRole } from '@/lib/jwt'

interface ProtectedRouteProps {
  /** When set, the user's role must be one of these or they're sent away. */
  allowedRoles?: UserRole[]
  /** Render as a wrapper (`<ProtectedRoute><Page/></ProtectedRoute>`) or, when
   * omitted, as a layout route via `<Outlet/>`. */
  children?: ReactNode
}

export function ProtectedRoute({ allowedRoles, children }: ProtectedRouteProps) {
  const { isAuthenticated, user } = useAuth()
  const location = useLocation()

  if (!isAuthenticated) {
    // Remember where they were headed so login can bounce them back later.
    return <Navigate to="/login" state={{ from: location }} replace />
  }

  if (
    allowedRoles &&
    allowedRoles.length > 0 &&
    !allowedRoles.includes(user?.role as UserRole)
  ) {
    return <Navigate to="/not-authorized" replace />
  }

  return children ? <>{children}</> : <Outlet />
}
