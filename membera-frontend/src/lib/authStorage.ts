// Token persistence. Kept framework-free so both the AuthContext (React) and the
// apiClient (plain module) can share it without a circular dependency.

export interface StoredTokens {
  accessToken: string
  refreshToken: string
}

const ACCESS_TOKEN_KEY = 'membera.accessToken'
const REFRESH_TOKEN_KEY = 'membera.refreshToken'

export function loadTokens(): StoredTokens | null {
  try {
    const accessToken = localStorage.getItem(ACCESS_TOKEN_KEY)
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY)
    if (!accessToken || !refreshToken) return null
    return { accessToken, refreshToken }
  } catch {
    // Storage unavailable (private mode, blocked cookies) — behave as logged out.
    return null
  }
}

export function saveTokens(tokens: StoredTokens): void {
  try {
    localStorage.setItem(ACCESS_TOKEN_KEY, tokens.accessToken)
    localStorage.setItem(REFRESH_TOKEN_KEY, tokens.refreshToken)
  } catch {
    // Ignore — the session just won't survive a refresh.
  }
}

export function clearTokens(): void {
  try {
    localStorage.removeItem(ACCESS_TOKEN_KEY)
    localStorage.removeItem(REFRESH_TOKEN_KEY)
  } catch {
    // Ignore.
  }
}

// Read-only accessor used by the apiClient on every request.
export function getAccessToken(): string | null {
  try {
    return localStorage.getItem(ACCESS_TOKEN_KEY)
  } catch {
    return null
  }
}

// Read-only accessor used by the token-refresh coordinator (lib/tokenRefresh.ts).
export function getRefreshToken(): string | null {
  try {
    return localStorage.getItem(REFRESH_TOKEN_KEY)
  } catch {
    return null
  }
}
