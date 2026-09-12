// Coordinates access-token refresh between the two places that need it: the
// proactive refresh AuthContext schedules ahead of expiry, and the reactive
// 401-retry in apiClient.ts. Both call refreshSession() below instead of
// hitting POST /auth/refresh directly, so they share a single in-flight
// request — the backend rotates the refresh token on every use, so two
// concurrent calls with the same (soon-to-be-superseded) token would leave
// one of them failing and forcing an unnecessary logout.

import { refreshAccessToken, type RefreshResult } from '@/lib/api'
import { clearTokens, getRefreshToken, saveTokens } from '@/lib/authStorage'

type RefreshedHandler = (result: RefreshResult) => void

let refreshedHandler: RefreshedHandler | null = null
let unauthorizedHandler: (() => void) | null = null

/** Registered once by the AuthContext: called with the new tokens whenever a
 * refresh (proactive or reactive) succeeds, so React state and the next
 * scheduled refresh stay in sync no matter which caller triggered it. */
export function setRefreshedHandler(handler: RefreshedHandler | null): void {
  refreshedHandler = handler
}

/** Registered once by the AuthContext: called whenever the session is
 * definitively unusable — a refresh attempt failed, there was no refresh
 * token to try, or a request still 401s right after a successful refresh. */
export function setUnauthorizedHandler(handler: (() => void) | null): void {
  unauthorizedHandler = handler
}

/** Shared "give up" path: clears the stored tokens and notifies whoever is
 * listening (AuthContext clears its React state and redirects to /login). */
export function forceLogout(): void {
  clearTokens()
  unauthorizedHandler?.()
}

let inFlight: Promise<RefreshResult> | null = null

/**
 * Refreshes the session's tokens. Concurrent callers share the same in-flight
 * request instead of each firing their own POST /auth/refresh. Resolves with
 * the new tokens (already persisted) on success; on failure, clears the
 * session, notifies the unauthorized handler, and rejects.
 */
export function refreshSession(): Promise<RefreshResult> {
  if (inFlight) return inFlight

  const refreshToken = getRefreshToken()
  if (!refreshToken) {
    forceLogout()
    return Promise.reject(new Error('No refresh token available.'))
  }

  inFlight = refreshAccessToken(refreshToken)
    .then((result) => {
      saveTokens({ accessToken: result.accessToken, refreshToken: result.refreshToken })
      refreshedHandler?.(result)
      return result
    })
    .catch((err: unknown) => {
      forceLogout()
      throw err
    })
    .finally(() => {
      inFlight = null
    })

  return inFlight
}
