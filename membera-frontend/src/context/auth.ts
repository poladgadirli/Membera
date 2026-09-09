import { createContext } from 'react'
import type { DecodedUser } from '@/lib/jwt'

export type AuthUser = DecodedUser

export interface AuthContextValue {
  user: AuthUser | null
  accessToken: string | null
  refreshToken: string | null
  isAuthenticated: boolean
  /** Call after a successful login / register / google-login API response. */
  login: (accessToken: string, refreshToken: string) => void
  /** Clears the session and returns the user to the landing page. */
  logout: () => void
}

// The provider lives in AuthContext.tsx; the hook in src/hooks/useAuth.ts.
export const AuthContext = createContext<AuthContextValue | undefined>(undefined)
