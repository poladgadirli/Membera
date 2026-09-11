import React, { useState, type FormEvent } from 'react'
import { ArrowLeft, Eye, EyeOff, Store, User } from 'lucide-react'
import { motion } from 'motion/react'
import type { Testimonial } from '@/components/ui/sign-in'
import { GoogleSignInButton } from '@/components/GoogleSignInButton'
import {
  SPRING_MOMENTUM,
  elementVariants,
  motionSafe,
  slideRightVariants,
  testimonialVariants,
  usePrefersReducedMotion,
} from '@/lib/motion'

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

// The glassmorphic field shell: a frosted, rounded container that lifts to a
// blue tint while focused. Same structure as the original auth design, with the
// violet accent swapped for the app's established blue.
const GlassInputWrapper = ({ children }: { children: React.ReactNode }) => (
  <div className="mt-1 rounded-2xl border border-border bg-foreground/5 backdrop-blur-sm transition-colors focus-within:border-blue-400/70 focus-within:bg-blue-500/10">
    {children}
  </div>
)

export type SignUpRole = 'customer' | 'merchant'

const RoleOptionCard = ({
  active,
  icon,
  title,
  description,
  onSelect,
}: {
  active: boolean
  icon: React.ReactNode
  title: string
  description: string
  onSelect: () => void
}) => (
  <button
    type="button"
    onClick={onSelect}
    aria-pressed={active}
    className={`flex flex-col items-start gap-2 rounded-2xl border p-4 text-left backdrop-blur-sm transition-colors ${
      active
        ? 'border-blue-400/70 bg-blue-500/10'
        : 'border-border bg-foreground/5 hover:bg-foreground/10'
    }`}
  >
    <span
      className={`grid h-9 w-9 shrink-0 place-items-center rounded-xl ${
        active
          ? 'bg-linear-to-br from-blue-500 via-blue-400 to-blue-200 text-white shadow-sm shadow-blue-500/30'
          : 'bg-foreground/10 text-muted-foreground'
      }`}
    >
      {icon}
    </span>
    <span className="text-sm font-medium text-neutral-900">{title}</span>
    <span className="text-xs leading-snug text-muted-foreground">
      {description}
    </span>
  </button>
)

const TestimonialCard = ({
  testimonial,
  delayMs,
  reduced,
}: {
  testimonial: Testimonial
  delayMs: number
  reduced: boolean
}) => (
  <motion.div
    variants={testimonialVariants}
    initial="initial"
    animate="animate"
    transition={motionSafe({ ...SPRING_MOMENTUM, delay: delayMs / 1000 }, reduced)}
    className="flex w-64 items-start gap-3 rounded-3xl border border-white/40 bg-white/40 p-5 shadow-sm shadow-blue-500/10 backdrop-blur-xl"
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
  </motion.div>
)

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
  const [showPassword, setShowPassword] = useState(false)
  const [showConfirmPassword, setShowConfirmPassword] = useState(false)
  const [role, setRole] = useState<SignUpRole>('customer')
  const reduced = usePrefersReducedMotion()
  const t = (delayMs: number) =>
    motionSafe({ ...SPRING_MOMENTUM, delay: delayMs / 1000 }, reduced)

  return (
    <div className="flex min-h-[100dvh] w-full flex-col font-geist md:flex-row">
      {/* Left column: sign-up form */}
      <section className="flex flex-1 items-center justify-center p-8">
        <div className="w-full max-w-md">
          <div className="flex flex-col gap-6">
            {onBackToHome && (
              <motion.button
                type="button"
                onClick={onBackToHome}
                variants={elementVariants}
                initial="initial"
                animate="animate"
                transition={t(100)}
                className="-ml-1 flex w-fit items-center gap-1.5 text-sm text-muted-foreground transition-colors hover:text-foreground"
              >
                <ArrowLeft className="h-4 w-4" />
                Back to home
              </motion.button>
            )}
            <motion.h1
              variants={elementVariants}
              initial="initial"
              animate="animate"
              transition={t(100)}
              className="text-4xl font-medium leading-tight tracking-tight text-neutral-900 md:text-5xl"
            >
              {title}
            </motion.h1>
            <motion.p
              variants={elementVariants}
              initial="initial"
              animate="animate"
              transition={t(200)}
              className="text-muted-foreground"
            >
              {description}
            </motion.p>

            <form className="space-y-5" onSubmit={onSignUp}>
              <input type="hidden" name="role" value={role} />

              <motion.div
                variants={elementVariants}
                initial="initial"
                animate="animate"
                transition={t(300)}
              >
                <label className="text-sm font-medium text-muted-foreground">
                  I'm signing up as
                </label>
                <div className="mt-1 grid grid-cols-2 gap-3">
                  <RoleOptionCard
                    active={role === 'customer'}
                    icon={<User className="h-4.5 w-4.5" />}
                    title="A customer"
                    description="Browse and subscribe to plans"
                    onSelect={() => setRole('customer')}
                  />
                  <RoleOptionCard
                    active={role === 'merchant'}
                    icon={<Store className="h-4.5 w-4.5" />}
                    title="A business"
                    description="List my business and sell plans"
                    onSelect={() => setRole('merchant')}
                  />
                </div>
              </motion.div>

              <motion.div
                variants={elementVariants}
                initial="initial"
                animate="animate"
                transition={t(400)}
                className="grid grid-cols-2 gap-4"
              >
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
                      className="w-full rounded-2xl bg-transparent p-4 text-sm text-neutral-900 placeholder:text-neutral-400 focus:outline-none"
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
                      className="w-full rounded-2xl bg-transparent p-4 text-sm text-neutral-900 placeholder:text-neutral-400 focus:outline-none"
                    />
                  </GlassInputWrapper>
                </div>
              </motion.div>

              <motion.div
                variants={elementVariants}
                initial="initial"
                animate="animate"
                transition={t(500)}
              >
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
              </motion.div>

              <motion.div
                variants={elementVariants}
                initial="initial"
                animate="animate"
                transition={t(600)}
              >
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
              </motion.div>

              <motion.div
                variants={elementVariants}
                initial="initial"
                animate="animate"
                transition={t(700)}
              >
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
                      className="w-full rounded-2xl bg-transparent p-4 pr-12 text-sm text-neutral-900 placeholder:text-neutral-400 focus:outline-none"
                    />
                    <button
                      type="button"
                      onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                      aria-label={
                        showConfirmPassword ? 'Hide password' : 'Show password'
                      }
                      className="absolute inset-y-0 right-3 flex items-center"
                    >
                      {showConfirmPassword ? (
                        <EyeOff className="h-5 w-5 text-muted-foreground transition-colors hover:text-foreground" />
                      ) : (
                        <Eye className="h-5 w-5 text-muted-foreground transition-colors hover:text-foreground" />
                      )}
                    </button>
                  </div>
                </GlassInputWrapper>
              </motion.div>

              {error && (
                <motion.div
                  variants={elementVariants}
                  initial="initial"
                  animate="animate"
                  transition={t(0)}
                  className="rounded-2xl border border-destructive/40 bg-destructive/10 px-4 py-3 text-sm text-destructive"
                >
                  {error}
                </motion.div>
              )}

              <motion.label
                variants={elementVariants}
                initial="initial"
                animate="animate"
                transition={t(800)}
                className="flex cursor-pointer items-center gap-3 text-sm"
              >
                <input
                  type="checkbox"
                  name="acceptTerms"
                  required
                  className="custom-checkbox"
                />
                <span className="text-foreground/90">
                  I agree to the Terms of Service and Privacy Policy
                </span>
              </motion.label>

              <motion.button
                type="submit"
                disabled={loading}
                variants={elementVariants}
                initial="initial"
                animate="animate"
                transition={t(900)}
                whileTap={loading ? undefined : { scale: 0.98 }}
                className="w-full rounded-2xl border border-blue-300 bg-linear-to-br from-blue-500 via-blue-400 to-blue-200 py-4 font-medium text-white shadow-sm shadow-blue-500/30 transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
              >
                {loading ? 'Creating account…' : 'Create Account'}
              </motion.button>
            </form>

            <motion.div
              variants={elementVariants}
              initial="initial"
              animate="animate"
              transition={t(900)}
              className="relative flex items-center justify-center"
            >
              <span className="w-full border-t border-border"></span>
              <span className="absolute bg-background px-4 text-sm text-muted-foreground">
                Or continue with
              </span>
            </motion.div>

            <motion.div
              variants={elementVariants}
              initial="initial"
              animate="animate"
              transition={t(1000)}
            >
              <GoogleSignInButton
                text="signup_with"
                onCredential={(idToken) => onGoogleSignUp?.(idToken)}
                onError={onGoogleError}
              />
            </motion.div>

            <motion.p
              variants={elementVariants}
              initial="initial"
              animate="animate"
              transition={t(1100)}
              className="text-center text-sm text-muted-foreground"
            >
              Already have an account?{' '}
              <a
                href="#"
                onClick={(e) => {
                  e.preventDefault()
                  onSignIn?.()
                }}
                className="text-blue-600 transition-colors hover:text-blue-500 hover:underline"
              >
                Sign In
              </a>
            </motion.p>
          </div>
        </div>
      </section>

      {/* Right column: hero image + testimonials */}
      {heroImageSrc && (
        <section className="relative hidden flex-1 p-4 md:block">
          <motion.div
            variants={slideRightVariants}
            initial="initial"
            animate="animate"
            transition={t(300)}
            className="absolute inset-4 rounded-3xl bg-cover bg-center"
            style={{ backgroundImage: `url(${heroImageSrc})` }}
          ></motion.div>
          {testimonials.length > 0 && (
            <div className="absolute bottom-8 left-1/2 flex w-full -translate-x-1/2 justify-center gap-4 px-8">
              <TestimonialCard
                testimonial={testimonials[0]}
                delayMs={1000}
                reduced={reduced}
              />
              {testimonials[1] && (
                <div className="hidden xl:flex">
                  <TestimonialCard
                    testimonial={testimonials[1]}
                    delayMs={1200}
                    reduced={reduced}
                  />
                </div>
              )}
              {testimonials[2] && (
                <div className="hidden 2xl:flex">
                  <TestimonialCard
                    testimonial={testimonials[2]}
                    delayMs={1400}
                    reduced={reduced}
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
