import React, { useState, type FormEvent } from 'react'
import { ArrowLeft, Eye, EyeOff } from 'lucide-react'
import type { Testimonial } from '@/components/ui/sign-in'
import { GoogleSignInButton } from '@/components/GoogleSignInButton'

// --- TYPE DEFINITIONS ---

export type { Testimonial }

interface SignUpPageProps {
  title?: React.ReactNode
  description?: React.ReactNode
  heroImageSrc?: string
  testimonials?: Testimonial[]
  onSignUp?: (event: FormEvent<HTMLFormElement>) => void
  onGoogleSignUp?: (idToken: string) => void
  onGoogleError?: (message: string) => void
  onSignIn?: () => void
  onBackToHome?: () => void
  error?: string | null
  loading?: boolean
}

// --- SUB-COMPONENTS ---

const GlassInputWrapper = ({ children }: { children: React.ReactNode }) => (
  <div className="rounded-2xl border border-border bg-foreground/5 backdrop-blur-sm transition-colors focus-within:border-violet-400/70 focus-within:bg-violet-500/10">
    {children}
  </div>
)

const TestimonialCard = ({
  testimonial,
  delay,
}: {
  testimonial: Testimonial
  delay: string
}) => (
  <div
    className={`animate-testimonial ${delay} flex items-start gap-3 rounded-3xl bg-card/40 dark:bg-zinc-800/40 backdrop-blur-xl border border-white/10 p-5 w-64`}
  >
    <img
      src={testimonial.avatarSrc}
      className="h-10 w-10 object-cover rounded-2xl"
      alt="avatar"
    />
    <div className="text-sm leading-snug">
      <p className="flex items-center gap-1 font-medium">{testimonial.name}</p>
      <p className="text-muted-foreground">{testimonial.handle}</p>
      <p className="mt-1 text-foreground/80">{testimonial.text}</p>
    </div>
  </div>
)

// --- MAIN COMPONENT ---

export const SignUpPage: React.FC<SignUpPageProps> = ({
  title = (
    <span className="font-light text-foreground tracking-tighter">
      Create your account
    </span>
  ),
  description = 'Start selling subscriptions your customers redeem with a QR code',
  heroImageSrc,
  testimonials = [],
  onSignUp,
  onGoogleSignUp,
  onGoogleError,
  onSignIn,
  onBackToHome,
  error,
  loading = false,
}) => {
  const [showPassword, setShowPassword] = useState(false)
  const [showConfirmPassword, setShowConfirmPassword] = useState(false)

  return (
    <div className="h-[100dvh] flex flex-col md:flex-row font-geist w-[100dvw]">
      {/* Left column: sign-up form */}
      <section className="flex-1 flex items-center justify-center p-8">
        <div className="w-full max-w-md">
          <div className="flex flex-col gap-6">
            {onBackToHome && (
              <button
                type="button"
                onClick={onBackToHome}
                className="animate-element animate-delay-100 -ml-1 flex w-fit items-center gap-1.5 text-sm text-muted-foreground transition-colors hover:text-foreground"
              >
                <ArrowLeft className="h-4 w-4" />
                Back to home
              </button>
            )}
            <h1 className="animate-element animate-delay-100 text-4xl md:text-5xl font-semibold leading-tight">
              {title}
            </h1>
            <p className="animate-element animate-delay-200 text-muted-foreground">
              {description}
            </p>

            <form className="space-y-5" onSubmit={onSignUp}>
              <div className="animate-element animate-delay-300 grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-medium text-muted-foreground">
                    First Name
                  </label>
                  <GlassInputWrapper>
                    <input
                      name="firstName"
                      type="text"
                      autoComplete="given-name"
                      placeholder="First name"
                      className="w-full bg-transparent text-sm p-4 rounded-2xl focus:outline-none"
                    />
                  </GlassInputWrapper>
                </div>
                <div>
                  <label className="text-sm font-medium text-muted-foreground">
                    Last Name
                  </label>
                  <GlassInputWrapper>
                    <input
                      name="lastName"
                      type="text"
                      autoComplete="family-name"
                      placeholder="Last name"
                      className="w-full bg-transparent text-sm p-4 rounded-2xl focus:outline-none"
                    />
                  </GlassInputWrapper>
                </div>
              </div>

              <div className="animate-element animate-delay-400">
                <label className="text-sm font-medium text-muted-foreground">
                  Email Address
                </label>
                <GlassInputWrapper>
                  <input
                    name="email"
                    type="email"
                    autoComplete="email"
                    placeholder="Enter your email address"
                    className="w-full bg-transparent text-sm p-4 rounded-2xl focus:outline-none"
                  />
                </GlassInputWrapper>
              </div>

              <div className="animate-element animate-delay-500">
                <label className="text-sm font-medium text-muted-foreground">
                  Password
                </label>
                <GlassInputWrapper>
                  <div className="relative">
                    <input
                      name="password"
                      type={showPassword ? 'text' : 'password'}
                      autoComplete="new-password"
                      placeholder="Create a password"
                      className="w-full bg-transparent text-sm p-4 pr-12 rounded-2xl focus:outline-none"
                    />
                    <button
                      type="button"
                      onClick={() => setShowPassword(!showPassword)}
                      className="absolute inset-y-0 right-3 flex items-center"
                    >
                      {showPassword ? (
                        <EyeOff className="w-5 h-5 text-muted-foreground hover:text-foreground transition-colors" />
                      ) : (
                        <Eye className="w-5 h-5 text-muted-foreground hover:text-foreground transition-colors" />
                      )}
                    </button>
                  </div>
                </GlassInputWrapper>
              </div>

              <div className="animate-element animate-delay-600">
                <label className="text-sm font-medium text-muted-foreground">
                  Confirm Password
                </label>
                <GlassInputWrapper>
                  <div className="relative">
                    <input
                      name="confirmPassword"
                      type={showConfirmPassword ? 'text' : 'password'}
                      autoComplete="new-password"
                      placeholder="Re-enter your password"
                      className="w-full bg-transparent text-sm p-4 pr-12 rounded-2xl focus:outline-none"
                    />
                    <button
                      type="button"
                      onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                      className="absolute inset-y-0 right-3 flex items-center"
                    >
                      {showConfirmPassword ? (
                        <EyeOff className="w-5 h-5 text-muted-foreground hover:text-foreground transition-colors" />
                      ) : (
                        <Eye className="w-5 h-5 text-muted-foreground hover:text-foreground transition-colors" />
                      )}
                    </button>
                  </div>
                </GlassInputWrapper>
              </div>

              {error && (
                <div className="animate-element rounded-2xl border border-destructive/40 bg-destructive/10 px-4 py-3 text-sm text-destructive">
                  {error}
                </div>
              )}

              <label className="animate-element animate-delay-700 flex items-center gap-3 text-sm cursor-pointer">
                <input
                  type="checkbox"
                  name="acceptTerms"
                  required
                  className="custom-checkbox"
                />
                <span className="text-foreground/90">
                  I agree to the Terms of Service and Privacy Policy
                </span>
              </label>

              <button
                type="submit"
                disabled={loading}
                className="animate-element animate-delay-800 w-full rounded-2xl bg-primary py-4 font-medium text-primary-foreground hover:bg-primary/90 transition-colors disabled:cursor-not-allowed disabled:opacity-60"
              >
                {loading ? 'Creating account…' : 'Create Account'}
              </button>
            </form>

            <div className="animate-element animate-delay-800 relative flex items-center justify-center">
              <span className="w-full border-t border-border"></span>
              <span className="px-4 text-sm text-muted-foreground bg-background absolute">
                Or continue with
              </span>
            </div>

            <div className="animate-element animate-delay-900">
              <GoogleSignInButton
                text="signup_with"
                onCredential={(idToken) => onGoogleSignUp?.(idToken)}
                onError={onGoogleError}
              />
            </div>

            <p className="animate-element animate-delay-1000 text-center text-sm text-muted-foreground">
              Already have an account?{' '}
              <a
                href="#"
                onClick={(e) => {
                  e.preventDefault()
                  onSignIn?.()
                }}
                className="text-violet-400 hover:underline transition-colors"
              >
                Sign In
              </a>
            </p>
          </div>
        </div>
      </section>

      {/* Right column: hero image + testimonials */}
      {heroImageSrc && (
        <section className="hidden md:block flex-1 relative p-4">
          <div
            className="animate-slide-right animate-delay-300 absolute inset-4 rounded-3xl bg-cover bg-center"
            style={{ backgroundImage: `url(${heroImageSrc})` }}
          ></div>
          {testimonials.length > 0 && (
            <div className="absolute bottom-8 left-1/2 -translate-x-1/2 flex gap-4 px-8 w-full justify-center">
              <TestimonialCard
                testimonial={testimonials[0]}
                delay="animate-delay-1000"
              />
              {testimonials[1] && (
                <div className="hidden xl:flex">
                  <TestimonialCard
                    testimonial={testimonials[1]}
                    delay="animate-delay-1200"
                  />
                </div>
              )}
              {testimonials[2] && (
                <div className="hidden 2xl:flex">
                  <TestimonialCard
                    testimonial={testimonials[2]}
                    delay="animate-delay-1400"
                  />
                </div>
              )}
            </div>
          )}
        </section>
      )}
    </div>
  )
}
