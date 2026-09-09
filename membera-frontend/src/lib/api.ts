// Thin client for the Membera Auth API. Only login + register are wired up.

const API_BASE_URL = (
  (import.meta.env.VITE_API_URL as string | undefined) ?? 'http://localhost:5035'
).replace(/\/$/, '')

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

export class ApiError extends Error {
  readonly status: number

  constructor(message: string, status: number) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

// The Auth API wraps successes in BaseResponse<T> and returns either the same
// shape or ASP.NET ValidationProblemDetails on failure. Pull a human message
// out of whichever we got.
function extractErrorMessage(payload: unknown): string | null {
  if (!payload) return null
  if (typeof payload === 'string') return payload

  const body = payload as Record<string, unknown>

  if (Array.isArray(body.errors) && body.errors.length > 0) {
    return body.errors.filter((e) => typeof e === 'string').join(' ')
  }

  if (body.errors && typeof body.errors === 'object') {
    const messages = Object.values(body.errors as Record<string, unknown>)
      .flat()
      .filter((m): m is string => typeof m === 'string')
    if (messages.length > 0) return messages.join(' ')
  }

  if (typeof body.message === 'string' && body.message) return body.message
  if (typeof body.title === 'string' && body.title) return body.title

  return null
}

async function post<T>(path: string, requestBody: unknown): Promise<T> {
  let response: Response
  try {
    response = await fetch(`${API_BASE_URL}${path}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(requestBody),
    })
  } catch {
    throw new ApiError(
      'Unable to reach the server. Check that the API is running and try again.',
      0,
    )
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
    throw new ApiError(
      extractErrorMessage(payload) ?? 'Something went wrong. Please try again.',
      response.status,
    )
  }

  // Unwrap BaseResponse<T> when present.
  if (payload && typeof payload === 'object' && 'data' in payload) {
    return (payload as Record<string, unknown>).data as T
  }
  return payload as T
}

export function login(email: string, password: string): Promise<LoginResult> {
  return post<LoginResult>('/api/auth/login', { email, password })
}

export function register(payload: RegisterPayload): Promise<RegisterResult> {
  return post<RegisterResult>('/api/auth/register', payload)
}

const ACCESS_TOKEN_KEY = 'membera.accessToken'
const REFRESH_TOKEN_KEY = 'membera.refreshToken'
const EXPIRES_AT_KEY = 'membera.accessTokenExpiresAt'

export const authStorage = {
  save(result: LoginResult) {
    try {
      localStorage.setItem(ACCESS_TOKEN_KEY, result.accessToken)
      localStorage.setItem(REFRESH_TOKEN_KEY, result.refreshToken)
      localStorage.setItem(EXPIRES_AT_KEY, result.accessTokenExpiresAt)
    } catch {
      // Storage can be unavailable (private mode); the session just won't persist.
    }
  },
  get accessToken(): string | null {
    try {
      return localStorage.getItem(ACCESS_TOKEN_KEY)
    } catch {
      return null
    }
  },
  clear() {
    try {
      localStorage.removeItem(ACCESS_TOKEN_KEY)
      localStorage.removeItem(REFRESH_TOKEN_KEY)
      localStorage.removeItem(EXPIRES_AT_KEY)
    } catch {
      // ignore
    }
  },
}
