import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { ArrowLeft, Eye, EyeOff } from 'lucide-react'
import { motion } from 'motion/react'
import { OtpInput } from '@/components/ui/otp-input'
import { ApiError, forgotPassword, resetPassword } from '@/lib/api'
import {
  SPRING_MOMENTUM,
  elementVariants,
  motionSafe,
  usePrefersReducedMotion,
} from '@/lib/motion'
import { INFO_BANNER } from '@/lib/ui'

// Same glass field shell as sign-in.tsx/sign-up.tsx's GlassInputWrapper —
// duplicated locally like those two do, rather than shared, since none of
// the three auth screens import from one another.
const GlassInputWrapper = ({ children }: { children: React.ReactNode }) => (
  <div className="mt-1 rounded-2xl border border-border bg-foreground/5 backdrop-blur-sm transition-colors focus-within:border-blue-400/70 focus-within:bg-blue-500/10">
    {children}
  </div>
)

const NEUTRAL_MESSAGE =
  "If an account exists with this email, we've sent a reset code."

type Step = 'email' | 'reset' | 'done'

export default function ForgotPasswordPage() {
  const navigate = useNavigate()
  const reduced = usePrefersReducedMotion()
  const t = (delayMs: number) =>
    motionSafe({ ...SPRING_MOMENTUM, delay: delayMs / 1000 }, reduced)

  const [step, setStep] = useState<Step>('email')
  const [email, setEmail] = useState('')
  const [code, setCode] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [confirmNewPassword, setConfirmNewPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [showConfirmPassword, setShowConfirmPassword] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const handleSendCode = async (event: FormEvent) => {
    event.preventDefault()
    setError(null)

    const trimmedEmail = email.trim()
    if (!trimmedEmail) {
      setError('Enter your email address.')
      return
    }

    setLoading(true)
    try {
      await forgotPassword(trimmedEmail)
      // The backend always returns 204 here regardless of whether the email
      // is registered — never branch this UI on the outcome, only on
      // whether the request itself succeeded.
      setStep('reset')
    } catch (err) {
      setError(
        err instanceof ApiError
          ? err.message
          : 'Unable to reach the server right now. Please try again.',
      )
    } finally {
      setLoading(false)
    }
  }

  const handleReset = async (event: FormEvent) => {
    event.preventDefault()
    setError(null)

    if (code.length !== 6) {
      setError('Enter the 6-digit code we sent you.')
      return
    }
    if (newPassword.length < 8) {
      setError('New password must be at least 8 characters long.')
      return
    }
    if (newPassword !== confirmNewPassword) {
      setError('New passwords do not match.')
      return
    }

    setLoading(true)
    try {
      await resetPassword({ email: email.trim(), code, newPassword, confirmNewPassword })
      setStep('done')
    } catch (err) {
      setError(
        err instanceof ApiError
          ? err.message
          : 'Unable to reset your password right now. Please try again.',
      )
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="relative flex min-h-[100dvh] w-full items-center justify-center overflow-hidden bg-[#f7f9fc] p-4 font-geist">
      <div
        aria-hidden="true"
        className="pointer-events-none absolute inset-x-0 top-0 h-80 bg-linear-to-b from-blue-100/70 via-blue-50/40 to-transparent"
      />
      <div className="relative w-full max-w-md rounded-2xl border border-white bg-white/70 p-8 shadow-sm shadow-blue-500/5 backdrop-blur-xl">
        <motion.button
          type="button"
          onClick={() => (step === 'email' ? navigate('/login') : setStep('email'))}
          variants={elementVariants}
          initial="initial"
          animate="animate"
          transition={t(50)}
          className="-ml-1 flex w-fit items-center gap-1.5 text-sm text-muted-foreground transition-colors hover:text-foreground"
        >
          <ArrowLeft className="h-4 w-4" />
          {step === 'email' ? 'Back to sign in' : 'Use a different email'}
        </motion.button>

        {step === 'email' && (
          <>
            <motion.h1
              variants={elementVariants}
              initial="initial"
              animate="animate"
              transition={t(100)}
              className="mt-4 text-3xl font-medium tracking-tight text-neutral-900"
            >
              Forgot your password?
            </motion.h1>
            <motion.p
              variants={elementVariants}
              initial="initial"
              animate="animate"
              transition={t(200)}
              className="mt-2 text-sm text-muted-foreground"
            >
              Enter the email on your account and we'll send you a reset code.
            </motion.p>

            <form onSubmit={handleSendCode} className="mt-8 space-y-5">
              <motion.div
                variants={elementVariants}
                initial="initial"
                animate="animate"
                transition={t(300)}
              >
                <label className="text-sm font-medium text-muted-foreground">
                  Email Address
                </label>
                <GlassInputWrapper>
                  <input
                    type="email"
                    autoComplete="email"
                    placeholder="Enter your email address"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    className="w-full rounded-2xl bg-transparent p-4 text-sm text-neutral-900 placeholder:text-neutral-400 focus:outline-none"
                  />
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

              <motion.button
                type="submit"
                disabled={loading}
                variants={elementVariants}
                initial="initial"
                animate="animate"
                transition={t(400)}
                whileTap={loading ? undefined : { scale: 0.98 }}
                className="w-full rounded-2xl border border-blue-300 bg-linear-to-br from-blue-500 via-blue-400 to-blue-200 py-4 font-medium text-white shadow-sm shadow-blue-500/30 transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
              >
                {loading ? 'Sending…' : 'Send reset code'}
              </motion.button>
            </form>
          </>
        )}

        {step === 'reset' && (
          <>
            <motion.h1
              variants={elementVariants}
              initial="initial"
              animate="animate"
              transition={t(100)}
              className="mt-4 text-3xl font-medium tracking-tight text-neutral-900"
            >
              Check your email
            </motion.h1>
            <motion.p
              variants={elementVariants}
              initial="initial"
              animate="animate"
              transition={t(200)}
              className={`${INFO_BANNER} mt-3`}
              role="status"
            >
              {NEUTRAL_MESSAGE}
            </motion.p>

            <form onSubmit={handleReset} className="mt-6 space-y-5">
              <motion.div
                variants={elementVariants}
                initial="initial"
                animate="animate"
                transition={t(300)}
                className="flex justify-center"
              >
                <OtpInput value={code} onChange={setCode} disabled={loading} />
              </motion.div>

              <motion.div
                variants={elementVariants}
                initial="initial"
                animate="animate"
                transition={t(400)}
              >
                <label className="text-sm font-medium text-muted-foreground">
                  New Password
                </label>
                <GlassInputWrapper>
                  <div className="relative">
                    <input
                      type={showPassword ? 'text' : 'password'}
                      autoComplete="new-password"
                      placeholder="Create a new password"
                      value={newPassword}
                      onChange={(e) => setNewPassword(e.target.value)}
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
                transition={t(500)}
              >
                <label className="text-sm font-medium text-muted-foreground">
                  Confirm New Password
                </label>
                <GlassInputWrapper>
                  <div className="relative">
                    <input
                      type={showConfirmPassword ? 'text' : 'password'}
                      autoComplete="new-password"
                      placeholder="Re-enter your new password"
                      value={confirmNewPassword}
                      onChange={(e) => setConfirmNewPassword(e.target.value)}
                      className="w-full rounded-2xl bg-transparent p-4 pr-12 text-sm text-neutral-900 placeholder:text-neutral-400 focus:outline-none"
                    />
                    <button
                      type="button"
                      onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                      aria-label={showConfirmPassword ? 'Hide password' : 'Show password'}
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

              <motion.button
                type="submit"
                disabled={loading || code.length !== 6}
                variants={elementVariants}
                initial="initial"
                animate="animate"
                transition={t(600)}
                whileTap={loading ? undefined : { scale: 0.98 }}
                className="w-full rounded-2xl border border-blue-300 bg-linear-to-br from-blue-500 via-blue-400 to-blue-200 py-4 font-medium text-white shadow-sm shadow-blue-500/30 transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
              >
                {loading ? 'Resetting…' : 'Reset password'}
              </motion.button>
            </form>
          </>
        )}

        {step === 'done' && (
          <>
            <motion.h1
              variants={elementVariants}
              initial="initial"
              animate="animate"
              transition={t(100)}
              className="mt-4 text-3xl font-medium tracking-tight text-neutral-900"
            >
              Password reset
            </motion.h1>
            <motion.p
              variants={elementVariants}
              initial="initial"
              animate="animate"
              transition={t(200)}
              className={`${INFO_BANNER} mt-3`}
              role="status"
            >
              Your password has been reset. You can now sign in with your new password.
            </motion.p>
            <motion.button
              type="button"
              onClick={() => navigate('/login')}
              variants={elementVariants}
              initial="initial"
              animate="animate"
              transition={t(300)}
              className="mt-6 w-full rounded-2xl border border-blue-300 bg-linear-to-br from-blue-500 via-blue-400 to-blue-200 py-4 font-medium text-white shadow-sm shadow-blue-500/30 transition hover:brightness-105"
            >
              Go to sign in
            </motion.button>
          </>
        )}
      </div>
    </div>
  )
}
