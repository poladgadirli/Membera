import React, { useState, type FormEvent } from 'react'
import { ArrowLeft, Eye, EyeOff } from 'lucide-react'
import { GoogleSignInButton } from '@/components/GoogleSignInButton'

// --- TYPE DEFINITIONS ---

export interface Testimonial {
  avatarSrc: string
  name: string
  handle: string
  text: string
}

interface SignInPageProps {
  title?: React.ReactNode
  description?: React.ReactNode
  heroImageSrc?: string
  testimonials?: Testimonial[]
  onSignIn?: (event: FormEvent<HTMLFormElement>) => void
  onGoogleSignIn?: (idToken: string) => void
  onGoogleError?: (message: string) => void
  onResetPassword?: () => void
  onCreateAccount?: () => void
  onBackToHome?: () => void
  error?: string | null
  loading?: boolean
}

// --- SUB-COMPONENTS ---

// The glassmorphic field shell: a frosted, rounded container that lifts to a
// blue tint while focused. Same structure as the original auth design, with the
// violet accent swapped for the app's established blue.
const GlassInputWrapper = ({ children }: { children: React.ReactNode }) => (
  <div className="mt-1 rounded-2xl border border-border bg-foreground/5 backdrop-blur-sm transition-colors focus-within:border-blue-400/70 focus-within:bg-blue-500/10">
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
    className={`animate-testimonial ${delay} flex w-64 items-start gap-3 rounded-3xl border border-white/40 bg-white/40 p-5 shadow-sm shadow-blue-500/10 backdrop-blur-xl`}
  >
    <img
      src={testimonial.avatarSrc}
      className="h-10 w-10 rounded-2xl object-cover"
      alt=""
    />
    <div className="text-sm leading-snug">
      <p className="flex items-center gap-1 font-medium text-neutral-900">
        {testimonial.name}
      </p>
      <p className="text-muted-foreground">{testimonial.handle}</p>
      <p className="mt-1 text-foreground/80">{testimonial.text}</p>
    </div>
  </div>
)

// --- MAIN COMPONENT ---

export const SignInPage: React.FC<SignInPageProps> = ({
  title = (
    <span className="font-medium tracking-tight text-neutral-900">Welcome</span>
  ),
  description = 'Access your account and continue your journey with us',
  heroImageSrc,
  testimonials = [],
  onSignIn,
  onGoogleSignIn,
  onGoogleError,
  onResetPassword,
  onCreateAccount,
  onBackToHome,
  error,
  loading = false,
}) => {
  const [showPassword, setShowPassword] = useState(false)

  return (
    <div className="flex min-h-[100dvh] w-full flex-col font-geist md:flex-row">
      {/* Left column: sign-in form */}
      <section className="flex flex-1 items-center justify-center p-8">
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
            <h1 className="animate-element animate-delay-100 text-4xl font-medium leading-tight tracking-tight text-neutral-900 md:text-5xl">
              {title}
            </h1>
            <p className="animate-element animate-delay-200 text-muted-foreground">
              {description}
            </p>

            <form className="space-y-5" onSubmit={onSignIn}>
              <div className="animate-element animate-delay-300">
                <label className="text-sm font-medium text-muted-foreground">
                  Email Address
                </label>
                <GlassInputWrapper>
                  <input
                    name="email"
                    type="email"
                    autoComplete="email"
                    placeholder="Enter your email address"
                    className="w-full rounded-2xl bg-transparent p-4 text-sm text-neutral-900 placeholder:text-neutral-400 focus:outline-none"
                  />
                </GlassInputWrapper>
              </div>

              <div className="animate-element animate-delay-400">
                <label className="text-sm font-medium text-muted-foreground">
                  Password
                </label>
                <GlassInputWrapper>
                  <div className="relative">
                    <input
                      name="password"
                      type={showPassword ? 'text' : 'password'}
                      autoComplete="current-password"
                      placeholder="Enter your password"
                      className="w-full rounded-2xl bg-transparent p-4 pr-12 text-sm text-neutral-900 placeholder:text-neutral-400 focus:outline-none"
                    />
                    <button
                      type="button"
                      onClick={() => setShowPassword(!showPassword)}
                      aria-label={showPassword ? 'Hide password' : 'Show password'}
                      className="absolute inset-y-0 right-3 flex items-center"
                    >
                      {showPassword ? (
                        <EyeOff className="h-5 w-5 text-muted-foreground transition-colors hover:text-foreground" />
                      ) : (
                        <Eye className="h-5 w-5 text-muted-foreground transition-colors hover:text-foreground" />
                      )}
                    </button>
                  </div>
                </GlassInputWrapper>
              </div>

              <div className="animate-element animate-delay-500 flex items-center justify-between text-sm">
                <label className="flex cursor-pointer items-center gap-3">
                  <input
                    type="checkbox"
                    name="rememberMe"
                    className="custom-checkbox"
                  />
                  <span className="text-foreground/90">Keep me signed in</span>
                </label>
                <a
                  href="#"
                  onClick={(e) => {
                    e.preventDefault()
                    onResetPassword?.()
                  }}
                  className="text-blue-600 transition-colors hover:text-blue-500 hover:underline"
                >
                  Reset password
                </a>
              </div>

              {error && (
                <div className="animate-element rounded-2xl border border-destructive/40 bg-destructive/10 px-4 py-3 text-sm text-destructive">
                  {error}
                </div>
              )}

              <button
                type="submit"
                disabled={loading}
                className="animate-element animate-delay-600 w-full rounded-2xl border border-blue-300 bg-linear-to-br from-blue-500 via-blue-400 to-blue-200 py-4 font-medium text-white shadow-sm shadow-blue-500/30 transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
              >
                {loading ? 'Signing in…' : 'Sign In'}
              </button>
            </form>

            <div className="animate-element animate-delay-700 relative flex items-center justify-center">
              <span className="w-full border-t border-border"></span>
              <span className="absolute bg-background px-4 text-sm text-muted-foreground">
                Or continue with
              </span>
            </div>

            <div className="animate-element animate-delay-800">
              <GoogleSignInButton
                text="signin_with"
                onCredential={(idToken) => onGoogleSignIn?.(idToken)}
                onError={onGoogleError}
              />
            </div>

            <p className="animate-element animate-delay-900 text-center text-sm text-muted-foreground">
              New to Membera?{' '}
              <a
                href="#"
                onClick={(e) => {
                  e.preventDefault()
                  onCreateAccount?.()
                }}
                className="text-blue-600 transition-colors hover:text-blue-500 hover:underline"
              >
                Create Account
              </a>
            </p>
          </div>
        </div>
      </section>

      {/* Right column: hero image + testimonials */}
      {heroImageSrc && (
        <section className="relative hidden flex-1 p-4 md:block">
          <div
            className="animate-slide-right animate-delay-300 absolute inset-4 rounded-3xl bg-cover bg-center"
            style={{ backgroundImage: `url(${heroImageSrc})` }}
          ></div>
          {testimonials.length > 0 && (
            <div className="absolute bottom-8 left-1/2 flex w-full -translate-x-1/2 justify-center gap-4 px-8">
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
