// Loads the Google Identity Services script and exposes the OAuth client id.
// The client id is public (it ships in the browser); keep the real value in
// VITE_GOOGLE_CLIENT_ID and fall back to the one the Auth API validates against.

export const GOOGLE_CLIENT_ID =
  (import.meta.env.VITE_GOOGLE_CLIENT_ID as string | undefined) ??
  '300113136297-8bemuk3ei6hbo64qbd3u9lqem757inkd.apps.googleusercontent.com'

const GSI_SRC = 'https://accounts.google.com/gsi/client'

let scriptPromise: Promise<void> | null = null

export function loadGoogleIdentityScript(): Promise<void> {
  if (typeof window === 'undefined') {
    return Promise.reject(new Error('Google Identity Services need a browser.'))
  }
  if (window.google?.accounts?.id) return Promise.resolve()
  if (scriptPromise) return scriptPromise

  scriptPromise = new Promise<void>((resolve, reject) => {
    const fail = () => {
      scriptPromise = null
      reject(new Error('Failed to load Google Identity Services.'))
    }

    const existing = document.querySelector<HTMLScriptElement>(
      `script[src="${GSI_SRC}"]`,
    )
    if (existing) {
      if (window.google?.accounts?.id) {
        resolve()
      } else {
        existing.addEventListener('load', () => resolve())
        existing.addEventListener('error', fail)
      }
      return
    }

    const script = document.createElement('script')
    script.src = GSI_SRC
    script.async = true
    script.defer = true
    script.onload = () => resolve()
    script.onerror = fail
    document.head.appendChild(script)
  })

  return scriptPromise
}
