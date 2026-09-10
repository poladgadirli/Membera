import { useState, type FormEvent } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { SignInPage, type Testimonial } from '@/components/ui/sign-in'
import { useAuth } from '@/hooks/useAuth'
import { ApiError, googleLogin, login } from '@/lib/api'

const testimonials: Testimonial[] = [
  {
    avatarSrc:
      'https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=facearea&facepad=3&w=128&h=128&q=80',
    name: 'Sarah Chen',
    handle: '@sarahdigital',
    text: 'Membera keeps my regulars coming back — subscriptions were live the same afternoon.',
  },
  {
    avatarSrc:
      'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=facearea&facepad=3&w=128&h=128&q=80',
    name: 'Marcus Johnson',
    handle: '@marcustech',
    text: 'One QR scan per visit. No hardware, no POS integration, no headaches.',
  },
  {
    avatarSrc:
      'https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?auto=format&fit=facearea&facepad=3&w=128&h=128&q=80',
    name: 'David Martinez',
    handle: '@davidcreates',
    text: 'I pay once for my weekly class and just show my phone at the door. Effortless.',
  },
]

export default function LoginPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const { login: startSession } = useAuth()
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const redirectTo =
    (location.state as { from?: { pathname?: string } } | null)?.from?.pathname ??
    '/dashboard'

  const handleSignIn = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setError(null)

    const data = Object.fromEntries(new FormData(event.currentTarget).entries())
    const email = String(data.email ?? '').trim()
    const password = String(data.password ?? '')

    if (!email || !password) {
      setError('Please enter your email and password.')
      return
    }

    setLoading(true)
    try {
      const result = await login(email, password)
      startSession(result.accessToken, result.refreshToken)
      navigate(redirectTo, { replace: true })
    } catch (err) {
      setError(
        err instanceof ApiError
          ? err.message
          : 'Unable to sign in right now. Please try again.',
      )
    } finally {
      setLoading(false)
    }
  }

  const handleGoogleSignIn = async (idToken: string) => {
    setError(null)
    setLoading(true)
    try {
      const result = await googleLogin(idToken)
      startSession(result.accessToken, result.refreshToken)
      navigate(redirectTo, { replace: true })
    } catch (err) {
      setError(
        err instanceof ApiError
          ? err.message
          : 'Unable to sign in with Google right now. Please try again.',
      )
    } finally {
      setLoading(false)
    }
  }

  return (
    <SignInPage
      title={
        <span className="font-medium tracking-tight text-neutral-900">
          Welcome back
        </span>
      }
      description="Sign in to manage your plans and redemptions."
      heroImageSrc="https://images.unsplash.com/photo-1642132652860-471b4228023e?auto=format&fit=crop&w=1600&q=80"
      testimonials={testimonials}
      onSignIn={handleSignIn}
      error={error}
      loading={loading}
      onGoogleSignIn={handleGoogleSignIn}
      onGoogleError={setError}
      onResetPassword={() => navigate('/contact')}
      onCreateAccount={() => navigate('/signup')}
      onBackToHome={() => navigate('/')}
    />
  )
}
