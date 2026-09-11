import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { SignUpPage, type Testimonial } from '@/components/ui/sign-up'
import { useAuth } from '@/hooks/useAuth'
import { ApiError, googleLogin, login, register } from '@/lib/api'

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

export default function RegisterPage() {
  const navigate = useNavigate()
  const { login: startSession } = useAuth()
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const handleSignUp = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setError(null)

    const data = Object.fromEntries(new FormData(event.currentTarget).entries())
    const firstName = String(data.firstName ?? '').trim()
    const lastName = String(data.lastName ?? '').trim()
    const email = String(data.email ?? '').trim()
    const password = String(data.password ?? '')
    const confirmPassword = String(data.confirmPassword ?? '')
    const isMerchantOwner = String(data.role ?? 'customer') === 'merchant'

    if (!firstName || !lastName || !email || !password) {
      setError('Please fill in every field.')
      return
    }
    if (password.length < 8) {
      setError('Password must be at least 8 characters long.')
      return
    }
    if (password !== confirmPassword) {
      setError('Passwords do not match.')
      return
    }

    setLoading(true)
    try {
      await register({
        firstName,
        lastName,
        email,
        password,
        confirmPassword,
        isMerchantOwner,
      })
      // Registration succeeded — sign the new user straight in.
      const session = await login(email, password)
      startSession(session.accessToken, session.refreshToken)
      navigate('/dashboard', { replace: true })
    } catch (err) {
      setError(
        err instanceof ApiError
          ? err.message
          : 'Unable to create your account right now. Please try again.',
      )
    } finally {
      setLoading(false)
    }
  }

  const handleGoogleSignUp = async (idToken: string) => {
    setError(null)
    setLoading(true)
    try {
      // Same endpoint as Google login — it creates the account on first use.
      const session = await googleLogin(idToken)
      startSession(session.accessToken, session.refreshToken)
      navigate('/dashboard', { replace: true })
    } catch (err) {
      setError(
        err instanceof ApiError
          ? err.message
          : 'Unable to sign up with Google right now. Please try again.',
      )
    } finally {
      setLoading(false)
    }
  }

  return (
    <SignUpPage
      title={
        <span className="font-medium tracking-tight text-neutral-900">
          Create your account
        </span>
      }
      description="Launch subscription plans your customers redeem with a single scan."
      heroImageSrc="https://images.unsplash.com/photo-1642132652860-471b4228023e?auto=format&fit=crop&w=1600&q=80"
      testimonials={testimonials}
      onSignUp={handleSignUp}
      error={error}
      loading={loading}
      onGoogleSignUp={handleGoogleSignUp}
      onGoogleError={setError}
      onSignIn={() => navigate('/login')}
      onBackToHome={() => navigate('/')}
    />
  )
}
