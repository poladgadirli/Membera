// fetch wrapper for the two backend APIs. Attaches the bearer token, unwraps the
// BaseResponse<T> envelope, and normalises errors. On 401 it notifies a handler
// (registered by the AuthContext) which clears the session and sends the user to
// /login. Automatic refresh-token retry is intentionally not implemented yet.

import { getAccessToken } from '@/lib/authStorage'

const AUTH_API_BASE_URL = (
  (import.meta.env.VITE_AUTH_API_URL as string | undefined) ??
  'https://localhost:7214/api'
).replace(/\/$/, '')

const MERCHANT_API_BASE_URL = (
  (import.meta.env.VITE_MERCHANT_API_URL as string | undefined) ??
  'https://localhost:7241/api'
).replace(/\/$/, '')

export class ApiError extends Error {
  readonly status: number
  readonly payload: unknown

  constructor(message: string, status: number, payload?: unknown) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.payload = payload
  }
}

let unauthorizedHandler: (() => void) | null = null

/** Registered once by the AuthContext. Invoked whenever a request returns 401. */
export function setUnauthorizedHandler(handler: (() => void) | null): void {
  unauthorizedHandler = handler
}

function extractErrorMessage(payload: unknown): string | null {
  if (!payload) return null
  if (typeof payload === 'string') return payload

  const body = payload as Record<string, unknown>

  if (Array.isArray(body.errors) && body.errors.length > 0) {
    return body.errors.filter((e): e is string => typeof e === 'string').join(' ')
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

interface RequestOptions {
  method?: string
  body?: unknown
  headers?: Record<string, string>
  signal?: AbortSignal
}

async function request<T>(
  baseUrl: string,
  path: string,
  options: RequestOptions = {},
): Promise<T> {
  const headers = new Headers(options.headers)
  const hasBody = options.body !== undefined && options.body !== null
  if (hasBody && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json')
  }

  const token = getAccessToken()
  if (token) headers.set('Authorization', `Bearer ${token}`)

  let response: Response
  try {
    response = await fetch(`${baseUrl}${path}`, {
      method: options.method ?? 'GET',
      headers,
      body: hasBody ? JSON.stringify(options.body) : undefined,
      signal: options.signal,
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

  if (response.status === 401) {
    unauthorizedHandler?.()
    throw new ApiError(
      extractErrorMessage(payload) ??
        'Your session has expired. Please sign in again.',
      401,
      payload,
    )
  }

  if (!response.ok) {
    throw new ApiError(
      extractErrorMessage(payload) ?? 'Something went wrong. Please try again.',
      response.status,
      payload,
    )
  }

  // Unwrap BaseResponse<T> ({ success, data, message }) when present.
  if (payload && typeof payload === 'object' && 'data' in payload) {
    return (payload as Record<string, unknown>).data as T
  }
  return payload as T
}

function makeClient(baseUrl: string) {
  return {
    baseUrl,
    get: <T>(path: string, options?: Omit<RequestOptions, 'method' | 'body'>) =>
      request<T>(baseUrl, path, { ...options, method: 'GET' }),
    post: <T>(
      path: string,
      body?: unknown,
      options?: Omit<RequestOptions, 'method' | 'body'>,
    ) => request<T>(baseUrl, path, { ...options, method: 'POST', body }),
    put: <T>(
      path: string,
      body?: unknown,
      options?: Omit<RequestOptions, 'method' | 'body'>,
    ) => request<T>(baseUrl, path, { ...options, method: 'PUT', body }),
    patch: <T>(
      path: string,
      body?: unknown,
      options?: Omit<RequestOptions, 'method' | 'body'>,
    ) => request<T>(baseUrl, path, { ...options, method: 'PATCH', body }),
    delete: <T>(path: string, options?: Omit<RequestOptions, 'method'>) =>
      request<T>(baseUrl, path, { ...options, method: 'DELETE' }),
  }
}

/** Calls the Auth service (login, register, token refresh, account settings). */
export const authApi = makeClient(AUTH_API_BASE_URL)

/** Calls the Merchant service (plans, subscriptions, redemptions). */
export const merchantApi = makeClient(MERCHANT_API_BASE_URL)
