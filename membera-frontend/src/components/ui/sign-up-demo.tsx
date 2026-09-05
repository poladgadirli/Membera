import type { FormEvent } from 'react'
import { SignUpPage, type Testimonial } from '@/components/ui/sign-up'

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

const SignUpPageDemo = () => {
  const handleSignUp = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const data = Object.fromEntries(new FormData(event.currentTarget).entries())
    console.log('Sign Up submitted:', data)
    alert('Sign Up Submitted! Check the browser console for form data.')
  }

  return (
    <div className="bg-background text-foreground">
      <SignUpPage
        heroImageSrc="https://images.unsplash.com/photo-1642132652860-471b4228023e?auto=format&fit=crop&w=1600&q=80"
        testimonials={sampleTestimonials}
        onSignUp={handleSignUp}
        onGoogleSignUp={() => alert('Continue with Google clicked')}
        onSignIn={() => alert('Sign In clicked')}
        onBackToHome={() => alert('Back to home clicked')}
      />
    </div>
  )
}

export default SignUpPageDemo
