// Auth API calls. Thin wrappers over the shared api client (src/lib/apiClient.ts).

import { api } from '@/lib/apiClient'

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
