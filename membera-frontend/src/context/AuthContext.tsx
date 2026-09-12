import {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from 'react'
import { useNavigate } from 'react-router-dom'
import { AuthContext, type AuthContextValue, type AuthUser } from '@/context/auth'
import { clearTokens, loadTokens, saveTokens } from '@/lib/authStorage'
import { decodeToken, isTokenExpired } from '@/lib/jwt'
import {
  refreshSession,
  setRefreshedHandler,
  setUnauthorizedHandler,
} from '@/lib/tokenRefresh'

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
// If the stored access token is already expired (or unreadable), this
// returns null — the mount effect below then tries a silent token refresh
// before giving up, since the refresh token (7-day lifetime) very likely
// still covers it.
function loadInitialSession(): SessionState | null {
  const stored = loadTokens()
  if (!stored) return null
  return sessionFromTokens(stored.accessToken, stored.refreshToken)
}

// How long before the access token's exp to proactively refresh it: 1 minute,
// or 20% of whatever time is left, whichever is smaller. The 20% branch keeps
// this sane if the access token lifetime is ever configured much shorter than
// the normal 15 minutes (e.g. temporarily, for testing).
function refreshDelayMs(accessToken: string): number | null {
  const decoded = decodeToken(accessToken)
  if (!decoded?.exp) return null
  const remainingMs = decoded.exp * 1000 - Date.now()
  const marginMs = Math.min(60_000, Math.max(remainingMs, 0) * 0.2)
  return Math.max(0, remainingMs - marginMs)
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const navigate = useNavigate()
  const [session, setSession] = useState<SessionState | null>(loadInitialSession)
  const refreshTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const clearScheduledRefresh = useCallback(() => {
    if (refreshTimerRef.current !== null) {
      clearTimeout(refreshTimerRef.current)
      refreshTimerRef.current = null
    }
  }, [])

  // Arms a timer to proactively refresh shortly before `accessToken` expires.
  // Firing it just calls the shared refreshSession() (lib/tokenRefresh.ts) —
  // the same one apiClient's 401 handler uses — so a proactive refresh and a
  // reactive one never race each other, and this component's state is kept
  // in sync via the onRefreshed/onUnauthorized listeners below regardless of
  // which side triggered the refresh.
  const scheduleRefresh = useCallback(
    (accessToken: string) => {
      clearScheduledRefresh()
      const delay = refreshDelayMs(accessToken)
      if (delay === null) return
      refreshTimerRef.current = setTimeout(() => {
        void refreshSession()
      }, delay)
    },
    [clearScheduledRefresh],
  )

  const login = useCallback(
    (accessToken: string, refreshToken: string) => {
      const next = sessionFromTokens(accessToken, refreshToken)
      if (!next) {
        // Token unreadable/expired on arrival — don't half-persist a bad session.
        clearTokens()
        setSession(null)
        return
      }
      saveTokens({ accessToken, refreshToken })
      setSession(next)
      scheduleRefresh(accessToken)
    },
    [scheduleRefresh],
  )

  const logout = useCallback(() => {
    clearScheduledRefresh()
    clearTokens()
    setSession(null)
    navigate('/', { replace: true })
  }, [navigate, clearScheduledRefresh])

  // Register the token-refresh coordinator's listeners once. `onRefreshed`
  // fires after ANY successful refresh (proactive timer or apiClient's 401
  // retry) so React state and the next scheduled refresh stay current no
  // matter which side triggered it. `onUnauthorized` fires when a refresh
  // attempt fails — the only case that should still log the user out.
  useEffect(() => {
    setRefreshedHandler((result) => {
      const next = sessionFromTokens(result.accessToken, result.refreshToken)
      if (!next) return
      setSession(next)
      scheduleRefresh(result.accessToken)
    })
    setUnauthorizedHandler(() => {
      clearScheduledRefresh()
      setSession(null)
      navigate('/login', { replace: true })
    })
    return () => {
      setRefreshedHandler(null)
      setUnauthorizedHandler(null)
    }
  }, [navigate, scheduleRefresh, clearScheduledRefresh])

  // Mount-once bootstrap: arm the proactive timer for a session restored from
  // storage, or — if the stored access token was already expired/unreadable —
  // try one silent refresh before accepting that as a logout. Intentionally
  // run only on mount; all later (re)scheduling goes through login() and the
  // onRefreshed listener above.
  useEffect(() => {
    if (session) {
      scheduleRefresh(session.accessToken)
    } else if (loadTokens()) {
      void refreshSession()
    }
    return () => clearScheduledRefresh()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

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
