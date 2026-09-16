import { createContext } from 'react'
import type { DecodedUser } from '@/lib/jwt'

export type AuthUser = DecodedUser

export interface AuthContextValue {
  user: AuthUser | null
  accessToken: string | null
  refreshToken: string | null
  isAuthenticated: boolean
  /**
   * Whether the signed-in user has completed email verification this
   * session. `null` means unknown: the access token carries no
   * `isEmailVerified` claim (the backend doesn't issue one yet), so this
   * is never populated from the JWT — only `markEmailVerified()` sets it,
   * and it resets to `null` on every login/logout. Treat it as a
   * this-session-only hint, not a durable verification status.
   */
  emailVerified: boolean | null
  /** Call after a successful login / register / google-login API response. */
  login: (accessToken: string, refreshToken: string) => void
  /** Record that the current user just verified their email, for the rest of this session. */
  markEmailVerified: () => void
  /** Clears the session and returns the user to the landing page. */
  logout: () => void
}

// The provider lives in AuthContext.tsx; the hook in src/hooks/useAuth.ts.
export const AuthContext = createContext<AuthContextValue | undefined>(undefined)
