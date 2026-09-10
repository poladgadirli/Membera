import React, { useState, type FormEvent } from 'react'
import { ArrowLeft, Eye, EyeOff } from 'lucide-react'
import type { Testimonial } from '@/components/ui/sign-in'
import { GoogleSignInButton } from '@/components/GoogleSignInButton'
import { ERROR_BANNER, PAGE_BG, PAGE_WASH } from '@/lib/ui'

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

const PasswordField = ({
  name,
  label,
  placeholder,
  autoComplete,
}: {
  name: string
  label: string
  placeholder: string
  autoComplete: string
}) => {
  const [show, setShow] = useState(false)
  return (
    <div className="space-y-1.5">
      <label className={labelClass}>{label}</label>
      <div className="relative">
        <input
          name={name}
          type={show ? 'text' : 'password'}
          autoComplete={autoComplete}
          placeholder={placeholder}
          className={`${fieldClass} pr-12`}
        />
        <button
          type="button"
          onClick={() => setShow(!show)}
          aria-label={show ? 'Hide password' : 'Show password'}
          className="absolute inset-y-0 right-3 flex items-center text-neutral-400 transition-colors hover:text-neutral-700"
        >
          {show ? <EyeOff className="h-5 w-5" /> : <Eye className="h-5 w-5" />}
        </button>
      </div>
    </div>
  )
}

// --- MAIN COMPONENT ---

export const SignUpPage: React.FC<SignUpPageProps> = ({
  title = (
    <span className="font-medium tracking-tight text-neutral-900">
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
  return (
    <div
      className={`flex min-h-[100dvh] w-full flex-col md:flex-row ${PAGE_BG} text-[#1e293b]`}
    >
      {/* Left column: sign-up form */}
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

            <form className="space-y-5" onSubmit={onSignUp}>
              <div className="animate-element animate-delay-300 grid grid-cols-2 gap-4">
                <div className="space-y-1.5">
                  <label className={labelClass}>First name</label>
                  <input
                    name="firstName"
                    type="text"
                    autoComplete="given-name"
                    placeholder="First name"
                    className={fieldClass}
                  />
                </div>
                <div className="space-y-1.5">
                  <label className={labelClass}>Last name</label>
                  <input
                    name="lastName"
                    type="text"
                    autoComplete="family-name"
                    placeholder="Last name"
                    className={fieldClass}
                  />
                </div>
              </div>

              <div className="animate-element animate-delay-400 space-y-1.5">
                <label className={labelClass}>Email address</label>
                <input
                  name="email"
                  type="email"
                  autoComplete="email"
                  placeholder="Enter your email address"
                  className={fieldClass}
                />
              </div>

              <div className="animate-element animate-delay-500">
                <PasswordField
                  name="password"
                  label="Password"
                  placeholder="Create a password"
                  autoComplete="new-password"
                />
              </div>

              <div className="animate-element animate-delay-600">
                <PasswordField
                  name="confirmPassword"
                  label="Confirm password"
                  placeholder="Re-enter your password"
                  autoComplete="new-password"
                />
              </div>

              {error && (
                <div className={`animate-element ${ERROR_BANNER}`}>{error}</div>
              )}

              <label className="animate-element animate-delay-700 flex cursor-pointer items-center gap-3 text-sm">
                <input
                  type="checkbox"
                  name="acceptTerms"
                  required
                  className="custom-checkbox"
                />
                <span className="text-neutral-700">
                  I agree to the Terms of Service and Privacy Policy
                </span>
              </label>

              <button
                type="submit"
                disabled={loading}
                className={`animate-element animate-delay-800 ${submitClass}`}
              >
                {loading ? 'Creating account…' : 'Create Account'}
              </button>
            </form>

            <div className="animate-element animate-delay-800 relative flex items-center justify-center">
              <span className="w-full border-t border-neutral-200"></span>
              <span className={`absolute px-4 text-sm text-neutral-500 ${PAGE_BG}`}>
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

            <p className="animate-element animate-delay-1000 text-center text-sm text-neutral-500">
              Already have an account?{' '}
              <a
                href="#"
                onClick={(e) => {
                  e.preventDefault()
                  onSignIn?.()
                }}
                className={linkClass}
              >
                Sign In
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
