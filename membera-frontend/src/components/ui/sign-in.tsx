import React, { useState, type FormEvent } from 'react'
import { ArrowLeft, Eye, EyeOff } from 'lucide-react'
import { GoogleSignInButton } from '@/components/GoogleSignInButton'
import { ERROR_BANNER, PAGE_BG, PAGE_WASH } from '@/lib/ui'

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

// --- SHARED CLASSES (landing-page visual language) ---

const fieldClass =
  'w-full rounded-xl border border-neutral-200 bg-white px-4 py-3 text-sm text-neutral-900 shadow-sm placeholder:text-neutral-400 focus:outline-none focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-500'
const labelClass = 'text-sm font-medium text-neutral-600'
const linkClass =
  'font-medium text-blue-600 transition-colors hover:text-blue-500'
const submitClass =
  'w-full rounded-xl border border-blue-300 bg-linear-to-br from-blue-500 via-blue-400 to-blue-200 py-3.5 text-sm font-semibold text-white shadow-sm shadow-blue-500/30 transition hover:brightness-105 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-500 disabled:cursor-not-allowed disabled:opacity-60'

// --- SUB-COMPONENTS ---

const TestimonialCard = ({
  testimonial,
  delay,
}: {
  testimonial: Testimonial
  delay: string
}) => (
  <div
    className={`animate-testimonial ${delay} flex w-64 items-start gap-3 rounded-2xl border border-white bg-white/70 p-5 shadow-sm shadow-blue-500/10 backdrop-blur-xl`}
  >
    <img
      src={testimonial.avatarSrc}
      className="h-10 w-10 rounded-xl object-cover"
      alt=""
    />
    <div className="text-sm leading-snug">
      <p className="font-semibold text-neutral-900">{testimonial.name}</p>
      <p className="text-neutral-500">{testimonial.handle}</p>
      <p className="mt-1 text-neutral-600">{testimonial.text}</p>
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
    <div
      className={`flex min-h-[100dvh] w-full flex-col md:flex-row ${PAGE_BG} text-[#1e293b]`}
    >
      {/* Left column: sign-in form */}
      <section className="relative flex flex-1 items-center justify-center overflow-hidden p-8">
        <div aria-hidden="true" className={PAGE_WASH} />
        <div className="relative z-10 w-full max-w-md">
          <div className="flex flex-col gap-6">
            {onBackToHome && (
              <button
                type="button"
                onClick={onBackToHome}
                className="animate-element animate-delay-100 -ml-1 flex w-fit items-center gap-1.5 text-sm text-neutral-500 transition-colors hover:text-neutral-900"
              >
                <ArrowLeft className="h-4 w-4" />
                Back to home
              </button>
            )}
            <h1 className="animate-element animate-delay-100 text-4xl font-medium leading-tight tracking-tight text-neutral-900 md:text-5xl">
              {title}
            </h1>
            <p className="animate-element animate-delay-200 text-neutral-500">
              {description}
            </p>

            <form className="space-y-5" onSubmit={onSignIn}>
              <div className="animate-element animate-delay-300 space-y-1.5">
                <label className={labelClass}>Email address</label>
                <input
                  name="email"
                  type="email"
                  autoComplete="email"
                  placeholder="Enter your email address"
                  className={fieldClass}
                />
              </div>

              <div className="animate-element animate-delay-400 space-y-1.5">
                <label className={labelClass}>Password</label>
                <div className="relative">
                  <input
                    name="password"
                    type={showPassword ? 'text' : 'password'}
                    autoComplete="current-password"
                    placeholder="Enter your password"
                    className={`${fieldClass} pr-12`}
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword(!showPassword)}
                    aria-label={showPassword ? 'Hide password' : 'Show password'}
                    className="absolute inset-y-0 right-3 flex items-center text-neutral-400 transition-colors hover:text-neutral-700"
                  >
                    {showPassword ? (
                      <EyeOff className="h-5 w-5" />
                    ) : (
                      <Eye className="h-5 w-5" />
                    )}
                  </button>
                </div>
              </div>

              <div className="animate-element animate-delay-500 flex items-center justify-between text-sm">
                <label className="flex cursor-pointer items-center gap-3">
                  <input
                    type="checkbox"
                    name="rememberMe"
                    className="custom-checkbox"
                  />
                  <span className="text-neutral-700">Keep me signed in</span>
                </label>
                <a
                  href="#"
                  onClick={(e) => {
                    e.preventDefault()
                    onResetPassword?.()
                  }}
                  className={linkClass}
                >
                  Reset password
                </a>
              </div>

              {error && (
                <div className={`animate-element ${ERROR_BANNER}`}>{error}</div>
              )}

              <button
                type="submit"
                disabled={loading}
                className={`animate-element animate-delay-600 ${submitClass}`}
              >
                {loading ? 'Signing in…' : 'Sign In'}
              </button>
            </form>

            <div className="animate-element animate-delay-700 relative flex items-center justify-center">
              <span className="w-full border-t border-neutral-200"></span>
              <span className={`absolute px-4 text-sm text-neutral-500 ${PAGE_BG}`}>
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

            <p className="animate-element animate-delay-900 text-center text-sm text-neutral-500">
              New to Membera?{' '}
              <a
                href="#"
                onClick={(e) => {
                  e.preventDefault()
                  onCreateAccount?.()
                }}
                className={linkClass}
              >
                Create account
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
