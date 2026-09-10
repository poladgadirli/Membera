// fetch wrapper for the backend API (reached through the Membera.Gateway reverse
// proxy). Attaches the bearer token, unwraps the BaseResponse<T> envelope, and
// normalises errors. On 401 it notifies a handler (registered by the AuthContext)
// which clears the session and sends the user to /login. Automatic refresh-token
// retry is intentionally not implemented yet.

import { getAccessToken } from '@/lib/authStorage'

const API_BASE_URL = (
  (import.meta.env.VITE_API_URL as string | undefined) ??
  'https://localhost:7174/api'
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
