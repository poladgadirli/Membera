import {
  useCallback,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { useNavigate } from 'react-router-dom'
import { AuthContext, type AuthContextValue, type AuthUser } from '@/context/auth'
import { setUnauthorizedHandler } from '@/lib/apiClient'
import { clearTokens, loadTokens, saveTokens } from '@/lib/authStorage'
import { decodeToken, isTokenExpired } from '@/lib/jwt'

interface SessionState {
  accessToken: string
  refreshToken: string
  user: AuthUser
}

function sessionFromTokens(
  accessToken: string,
  refreshToken: string,
): SessionState | null {
  const user = decodeToken(accessToken)
  if (!user || isTokenExpired(user)) return null
  return { accessToken, refreshToken, user }
}

// Restore a persisted session on first render so a refresh doesn't log out.
function loadInitialSession(): SessionState | null {
  const stored = loadTokens()
  if (!stored) return null
  const session = sessionFromTokens(stored.accessToken, stored.refreshToken)
  if (!session) {
    clearTokens()
    return null
  }
  return session
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const navigate = useNavigate()
  const [session, setSession] = useState<SessionState | null>(loadInitialSession)

  const login = useCallback((accessToken: string, refreshToken: string) => {
    const next = sessionFromTokens(accessToken, refreshToken)
    if (!next) {
      // Token unreadable/expired on arrival — don't half-persist a bad session.
      clearTokens()
      setSession(null)
      return
    }
    saveTokens({ accessToken, refreshToken })
    setSession(next)
  }, [])

  const logout = useCallback(() => {
    clearTokens()
    setSession(null)
    navigate('/', { replace: true })
  }, [navigate])

  // When any API call 401s, drop the session and send the user to /login.
  useEffect(() => {
    setUnauthorizedHandler(() => {
      clearTokens()
      setSession(null)
      navigate('/login', { replace: true })
    })
    return () => setUnauthorizedHandler(null)
  }, [navigate])

  const value = useMemo<AuthContextValue>(
    () => ({
      user: session?.user ?? null,
      accessToken: session?.accessToken ?? null,
      refreshToken: session?.refreshToken ?? null,
      isAuthenticated: session !== null,
      login,
      logout,
    }),
    [session, login, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
