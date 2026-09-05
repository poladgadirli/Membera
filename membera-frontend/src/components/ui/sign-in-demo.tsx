import type { FormEvent } from 'react'
import { SignInPage, type Testimonial } from '@/components/ui/sign-in'

const sampleTestimonials: Testimonial[] = [
  {
    avatarSrc:
      'https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=facearea&facepad=3&w=128&h=128&q=80',
    name: 'Sarah Chen',
    handle: '@sarahdigital',
    text: 'Amazing platform! The user experience is seamless and the features are exactly what I needed.',
  },
  {
    avatarSrc:
      'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=facearea&facepad=3&w=128&h=128&q=80',
    name: 'Marcus Johnson',
    handle: '@marcustech',
    text: 'This service has transformed how I work. Clean design, powerful features, and excellent support.',
  },
  {
    avatarSrc:
      'https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?auto=format&fit=facearea&facepad=3&w=128&h=128&q=80',
    name: 'David Martinez',
    handle: '@davidcreates',
    text: "I've tried many platforms, but this one stands out. Intuitive, reliable, and genuinely helpful for productivity.",
  },
]

const SignInPageDemo = () => {
  const handleSignIn = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const formData = new FormData(event.currentTarget)
    const data = Object.fromEntries(formData.entries())
    console.log('Sign In submitted:', data)
    alert('Sign In Submitted! Check the browser console for form data.')
  }

  const handleGoogleSignIn = () => {
    console.log('Continue with Google clicked')
    alert('Continue with Google clicked')
  }

  const handleResetPassword = () => {
    alert('Reset Password clicked')
  }

  const handleCreateAccount = () => {
    alert('Create Account clicked')
  }

  return (
    <div className="bg-background text-foreground">
      <SignInPage
        heroImageSrc="https://images.unsplash.com/photo-1642132652860-471b4228023e?auto=format&fit=crop&w=1600&q=80"
        testimonials={sampleTestimonials}
        onSignIn={handleSignIn}
        onGoogleSignIn={handleGoogleSignIn}
        onResetPassword={handleResetPassword}
        onCreateAccount={handleCreateAccount}
      />
    </div>
  )
}

export default SignInPageDemo
