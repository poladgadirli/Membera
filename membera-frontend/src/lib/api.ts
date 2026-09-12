// Auth API calls. Thin wrappers over the shared api client (src/lib/apiClient.ts).

import { api, ApiError } from '@/lib/apiClient'
import { API_BASE_URL } from '@/lib/apiConfig'

export { ApiError } from '@/lib/apiClient'

export interface LoginResult {
  accessToken: string
  refreshToken: string
  accessTokenExpiresAt: string
}

export interface RegisterResult {
  userId: string
  email: string
  role: string
}

export interface RegisterPayload {
  firstName: string
  lastName: string
  email: string
  password: string
  confirmPassword: string
  isMerchantOwner: boolean
}

export function login(email: string, password: string): Promise<LoginResult> {
  return api.post<LoginResult>('/auth/login', { email, password })
}

export function register(payload: RegisterPayload): Promise<RegisterResult> {
  return api.post<RegisterResult>('/auth/register', payload)
}

// Exchanges a Google ID token for a Membera session. The endpoint signs the
// user in, creating the account on first use, so it covers both "log in with
// Google" and "sign up with Google".
export function googleLogin(idToken: string): Promise<LoginResult> {
  return api.post<LoginResult>('/auth/google-login', { idToken })
}

export interface RefreshResult {
  accessToken: string
  refreshToken: string
}

/**
 * Exchanges a refresh token for a new access/refresh token pair (the backend
 * rotates the refresh token on each use). Deliberately bypasses the shared
 * `api` client: that client attaches the current — possibly already expired —
 * access token and runs its own 401-retry logic, which is exactly what calls
 * this function. A plain fetch avoids both the stale header and any
 * possibility of recursing back into that retry logic.
 */
export async function refreshAccessToken(
  refreshToken: string,
): Promise<RefreshResult> {
  let response: Response
  try {
    response = await fetch(`${API_BASE_URL}/auth/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken }),
    })
  } catch (cause) {
    throw new ApiError('Unable to reach the server to refresh the session.', 0, cause)
  }

  const text = await response.text()
  let payload: unknown = null
  if (text) {
    try {
      payload = JSON.parse(text)
    } catch {
      payload = text
    }
  }

  if (!response.ok) {
    throw new ApiError('Unable to refresh session.', response.status, payload)
  }

  // Unwrap BaseResponse<T> ({ success, data, message }) when present, same as
  // the shared client does.
  const data =
    payload && typeof payload === 'object' && 'data' in payload
      ? (payload as Record<string, unknown>).data
      : payload

  return data as RefreshResult
}
