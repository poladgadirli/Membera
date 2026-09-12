// fetch wrapper for the backend API (reached through the Membera.Gateway reverse
// proxy). Attaches the bearer token, unwraps the BaseResponse<T> envelope, and
// normalises errors. On 401 it attempts one token refresh (see
// lib/tokenRefresh.ts) and retries the request once with the new token; only
// if that refresh fails (or a request still 401s right after it) does it
// notify the unauthorized handler (registered by the AuthContext), which
// clears the session and sends the user to /login.

import { API_BASE_URL } from '@/lib/apiConfig'
import { getAccessToken } from '@/lib/authStorage'
import { forceLogout, refreshSession } from '@/lib/tokenRefresh'

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
  isRetry = false,
): Promise<T> {
  const headers = new Headers(options.headers)
  const hasBody = options.body !== undefined && options.body !== null
  // FormData (file uploads) must NOT get a Content-Type header — the browser
  // sets `multipart/form-data` with the correct boundary itself. Everything
  // else is sent as JSON.
  const isFormData =
    typeof FormData !== 'undefined' && options.body instanceof FormData
  if (hasBody && !isFormData && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json')
  }

  const token = getAccessToken()
  if (token) headers.set('Authorization', `Bearer ${token}`)

  let response: Response
  try {
    response = await fetch(`${baseUrl}${path}`, {
      method: options.method ?? 'GET',
      headers,
      body: hasBody
        ? isFormData
          ? (options.body as FormData)
          : JSON.stringify(options.body)
        : undefined,
      signal: options.signal,
    })
  } catch (cause) {
    // Don't swallow the underlying failure. `fetch` rejects (or throws) for CORS
    // blocks, TLS/cert errors, offline, and any bug in the request setup above —
    // without this log they all collapse into one vague message and never reach
    // the console or DevTools.
    console.error(`[apiClient] ${options.method ?? 'GET'} ${path} failed before a response:`, cause)
    throw new ApiError(
      'Unable to reach the server. Check that the API is running and try again.',
      0,
      cause,
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
    if (!isRetry) {
      // One refresh-and-retry attempt. refreshSession() shares a single
      // in-flight request across concurrent 401s (and the AuthContext's own
      // proactive refresh), and already clears the session + notifies the
      // unauthorized handler on failure — nothing more to do in that case.
      const refreshed = await refreshSession().then(
        () => true,
        () => false,
      )
      if (refreshed) {
        return request<T>(baseUrl, path, options, true)
      }
    } else {
      // Retried once already with a freshly refreshed token and still got
      // 401 (e.g. the account was deactivated mid-session) — no further
      // refresh to attempt, but the session is unusable either way.
      forceLogout()
    }

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
    /** POST a FormData body (file uploads). Skips the JSON Content-Type header. */
    postForm: <T>(
      path: string,
      body: FormData,
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

/**
 * The single API client. Every request goes through the gateway, which routes
 * it to the Auth or Merchant service by path prefix (/auth, /admin → Auth;
 * /merchant, /subscription-plans, /subscriptions → Merchant).
 */
export const api = makeClient(API_BASE_URL)
