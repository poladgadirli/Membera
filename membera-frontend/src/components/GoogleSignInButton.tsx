import { useEffect, useRef } from 'react'
import { GOOGLE_CLIENT_ID, loadGoogleIdentityScript } from '@/lib/googleAuth'

interface GoogleSignInButtonProps {
  /** Called with the Google ID token once the user picks an account. */
  onCredential: (idToken: string) => void
  onError?: (message: string) => void
  text?: 'signin_with' | 'signup_with' | 'continue_with'
}

// Renders Google's official Identity Services button. A click opens Google's
// account chooser and returns an ID token (a signed JWT) that the Auth API
// verifies at POST /api/auth/google-login.
export function GoogleSignInButton({
  onCredential,
  onError,
  text = 'continue_with',
}: GoogleSignInButtonProps) {
  const containerRef = useRef<HTMLDivElement>(null)
  const onCredentialRef = useRef(onCredential)
  const onErrorRef = useRef(onError)
  onCredentialRef.current = onCredential
  onErrorRef.current = onError

  useEffect(() => {
    let cancelled = false

    loadGoogleIdentityScript()
      .then(() => {
        const container = containerRef.current
        const google = window.google
        if (cancelled || !container || !google) return

        google.accounts.id.initialize({
          client_id: GOOGLE_CLIENT_ID,
          callback: (response) => {
            if (response.credential) {
              onCredentialRef.current(response.credential)
            } else {
              onErrorRef.current?.('Google sign-in was cancelled.')
            }
          },
        })

        container.innerHTML = ''
        google.accounts.id.renderButton(container, {
          type: 'standard',
          theme: 'outline',
          size: 'large',
          shape: 'pill',
          text,
          logo_alignment: 'center',
          width: Math.min(container.offsetWidth || 360, 400),
        })
      })
      .catch(() => {
        if (!cancelled) {
          onErrorRef.current?.(
            'Could not load Google sign-in. Please try again.',
          )
        }
      })

    return () => {
      cancelled = true
    }
  }, [text])

  return (
    <div
      ref={containerRef}
      className="flex min-h-[40px] justify-center [color-scheme:light]"
    />
  )
}
