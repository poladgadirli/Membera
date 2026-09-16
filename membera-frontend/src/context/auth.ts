import { createContext } from 'react'
import type { DecodedUser } from '@/lib/jwt'

export type AuthUser = DecodedUser

export interface AuthContextValue {
  user: AuthUser | null
  accessToken: string | null
  refreshToken: string | null
  isAuthenticated: boolean
  /**
   * Whether the signed-in user's email is verified, read straight from the
   * current access token's `emailVerified` claim — `null` only when there's
   * no session at all. Because the backend re-reads `User.IsEmailVerified`
   * from the database and re-mints this claim on every login *and* on every
   * silent token refresh, this value self-corrects shortly after
   * verification without any client-side bookkeeping.
   */
  emailVerified: boolean | null
  /** Call after a successful login / register / google-login API response. */
  login: (accessToken: string, refreshToken: string) => void
  /** Clears the session and returns the user to the landing page. */
  logout: () => void
}

// The provider lives in AuthContext.tsx; the hook in src/hooks/useAuth.ts.
export const AuthContext = createContext<AuthContextValue | undefined>(undefined)
