import { useEffect, useState, type FormEvent } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { motion } from 'motion/react'
import { OtpInput } from '@/components/ui/otp-input'
import { useAuth } from '@/hooks/useAuth'
import { ApiError, resendVerification, verifyEmail } from '@/lib/api'
import {
  SPRING_MOMENTUM,
  elementVariants,
  motionSafe,
  usePrefersReducedMotion,
} from '@/lib/motion'
import { INFO_BANNER } from '@/lib/ui'

const RESEND_COOLDOWN_SECONDS = 30

/**
 * Shown right after registration (verify-email / resend-verification both
 * require a session, so this can only run while already signed in). Reached
 * via `navigate('/verify-email', { state: { email } })` from RegisterPage;
 * falls back to the JWT's own email claim if opened without that state.
 */
export default function VerifyEmailPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const { user, markEmailVerified } = useAuth()
  const email = (location.state as { email?: string } | null)?.email ?? user?.email ?? ''

  const [code, setCode] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const [resending, setResending] = useState(false)
  const [resent, setResent] = useState(false)
  const [cooldown, setCooldown] = useState(0)

  const reduced = usePrefersReducedMotion()
  const t = (delayMs: number) =>
    motionSafe({ ...SPRING_MOMENTUM, delay: delayMs / 1000 }, reduced)

  useEffect(() => {
    if (cooldown <= 0) return
    const timer = setInterval(() => setCooldown((s) => Math.max(0, s - 1)), 1000)
    return () => clearInterval(timer)
  }, [cooldown])

  const handleVerify = async (event: FormEvent) => {
    event.preventDefault()
    setError(null)

    if (code.length !== 6) {
      setError('Enter the 6-digit code we sent you.')
      return
    }

    setLoading(true)
    try {
      await verifyEmail(code)
      markEmailVerified()
      navigate('/dashboard', { replace: true })
    } catch (err) {
      setError(
        err instanceof ApiError
          ? err.message
          : 'That code didn’t work. Please try again.',
      )
    } finally {
      setLoading(false)
    }
  }

  const handleResend = async () => {
    setError(null)
    setResent(false)
    setResending(true)
    try {
      await resendVerification()
      setResent(true)
      setCooldown(RESEND_COOLDOWN_SECONDS)
    } catch (err) {
      setError(
        err instanceof ApiError
          ? err.message
          : 'Could not resend the code. Please try again.',
      )
    } finally {
      setResending(false)
    }
  }

  return (
    <div className="relative flex min-h-[100dvh] w-full items-center justify-center overflow-hidden bg-[#f7f9fc] p-4 font-geist">
      <div
        aria-hidden="true"
        className="pointer-events-none absolute inset-x-0 top-0 h-80 bg-linear-to-b from-blue-100/70 via-blue-50/40 to-transparent"
      />
      <div className="relative w-full max-w-md rounded-2xl border border-white bg-white/70 p-8 shadow-sm shadow-blue-500/5 backdrop-blur-xl">
        <motion.h1
          variants={elementVariants}
          initial="initial"
          animate="animate"
          transition={t(100)}
          className="text-3xl font-medium tracking-tight text-neutral-900"
        >
          Verify your email
        </motion.h1>
        <motion.p
          variants={elementVariants}
          initial="initial"
          animate="animate"
          transition={t(200)}
          className="mt-2 text-sm text-muted-foreground"
        >
          {email ? (
            <>
              We sent a code to{' '}
              <span className="font-medium text-neutral-900">{email}</span>.
            </>
          ) : (
            'We sent a code to your email.'
          )}{' '}
          Enter it below to verify your account.
        </motion.p>

        <form onSubmit={handleVerify} className="mt-8 space-y-5">
          <motion.div
            variants={elementVariants}
            initial="initial"
            animate="animate"
            transition={t(300)}
            className="flex justify-center"
          >
            <OtpInput value={code} onChange={setCode} disabled={loading} />
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
          {resent && !error && (
            <motion.div
              variants={elementVariants}
              initial="initial"
              animate="animate"
              transition={t(0)}
              className={INFO_BANNER}
              role="status"
            >
              We sent a new code.
            </motion.div>
          )}

          <motion.button
            type="submit"
            disabled={loading || code.length !== 6}
            variants={elementVariants}
            initial="initial"
            animate="animate"
            transition={t(400)}
            whileTap={loading ? undefined : { scale: 0.98 }}
            className="w-full rounded-2xl border border-blue-300 bg-linear-to-br from-blue-500 via-blue-400 to-blue-200 py-4 font-medium text-white shadow-sm shadow-blue-500/30 transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {loading ? 'Verifying…' : 'Verify email'}
          </motion.button>
        </form>

        <motion.div
          variants={elementVariants}
          initial="initial"
          animate="animate"
          transition={t(500)}
          className="mt-6 flex items-center justify-between text-sm"
        >
          <button
            type="button"
            onClick={handleResend}
            disabled={resending || cooldown > 0}
            className="text-blue-600 transition-colors hover:text-blue-500 hover:underline disabled:cursor-not-allowed disabled:text-muted-foreground disabled:no-underline"
          >
            {cooldown > 0
              ? `Resend code (${cooldown}s)`
              : resending
                ? 'Resending…'
                : 'Resend code'}
          </button>
          <button
            type="button"
            onClick={() => navigate('/dashboard', { replace: true })}
            className="text-muted-foreground transition-colors hover:text-foreground"
          >
            Skip for now
          </button>
        </motion.div>
      </div>
    </div>
  )
}
