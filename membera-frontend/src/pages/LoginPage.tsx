import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { SignInPage, type Testimonial } from '@/components/ui/sign-in'

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

  const handleSignIn = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const data = Object.fromEntries(new FormData(event.currentTarget).entries())
    console.log('Sign in submitted:', data)
    // TODO: call the auth endpoint, then redirect on success.
    navigate('/')
  }

  return (
    <SignInPage
      title={
        <span className="font-light tracking-tighter text-foreground">
          Welcome back
        </span>
      }
      description="Sign in to manage your plans and redemptions."
      heroImageSrc="https://images.unsplash.com/photo-1642132652860-471b4228023e?auto=format&fit=crop&w=1600&q=80"
      testimonials={testimonials}
      onSignIn={handleSignIn}
      onGoogleSignIn={() => console.log('Continue with Google')}
      onResetPassword={() => navigate('/contact')}
      onCreateAccount={() => navigate('/signup')}
    />
  )
}
