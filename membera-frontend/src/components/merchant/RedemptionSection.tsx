import { useState, type FormEvent } from 'react'
import { AnimatePresence, motion } from 'motion/react'
import { CheckIcon } from '@/components/icons'
import { Spinner } from '@/components/Spinner'
import { SPRING_UI, materializeVariants, motionSafe, usePrefersReducedMotion } from '@/lib/motion'
import { CARD, SECTION_LABEL } from '@/lib/ui'
import {
  ApiError,
  redeemSubscription,
  type RedeemResult,
} from '@/lib/subscriptions'

type Outcome =
  | { kind: 'idle' }
  | { kind: 'success'; result: RedeemResult }
  | { kind: 'error'; message: string }

/**
 * Counter tool for merchant staff: type a customer's redemption code, hit
 * Redeem, and get a big, unambiguous success or failure state. Deliberately
 * plain and large — it's used at a physical till, often quickly.
 */
export function RedemptionSection() {
  const [code, setCode] = useState('')
  const [busy, setBusy] = useState(false)
  const [outcome, setOutcome] = useState<Outcome>({ kind: 'idle' })
  const reduced = usePrefersReducedMotion()

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const trimmed = code.trim().toUpperCase()
    if (!trimmed || busy) return

    setBusy(true)
    setOutcome({ kind: 'idle' })
    try {
      const result = await redeemSubscription(trimmed)
      setOutcome({ kind: 'success', result })
      setCode('')
    } catch (err) {
      setOutcome({
        kind: 'error',
        message:
          err instanceof ApiError
            ? err.message
            : 'Could not redeem this code. Please try again.',
      })
    } finally {
      setBusy(false)
    }
  }

  return (
    <section aria-labelledby="redeem-heading" className="mt-10">
      <h2 id="redeem-heading" className={SECTION_LABEL}>
        Redeem a code
      </h2>

      <div className={`${CARD} mt-3 p-6`}>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label
              htmlFor="redemption-code"
              className="block text-sm font-medium text-neutral-900"
            >
              Customer&rsquo;s redemption code
            </label>
            <input
              id="redemption-code"
              value={code}
              onChange={(e) => {
                setCode(e.target.value)
                if (outcome.kind !== 'idle') setOutcome({ kind: 'idle' })
              }}
              autoComplete="off"
              autoCapitalize="characters"
              spellCheck={false}
              placeholder="MBR-XXXXXX"
              className="mt-1.5 w-full rounded-xl border border-neutral-200 bg-white px-4 py-4 text-center font-mono text-2xl font-semibold uppercase tracking-[0.3em] text-neutral-900 shadow-sm placeholder:tracking-normal placeholder:text-neutral-300 focus:outline-none focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-500"
            />
          </div>

          <button
            type="submit"
            disabled={busy || !code.trim()}
            className="flex w-full items-center justify-center gap-2 rounded-xl border border-blue-300 bg-linear-to-br from-blue-500 via-blue-400 to-blue-200 py-4 text-base font-semibold text-white shadow-sm shadow-blue-500/30 transition duration-150 ease-out active:scale-[0.98] hover:brightness-105 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-500 disabled:cursor-not-allowed disabled:opacity-60 disabled:active:scale-100"
          >
            {busy && <Spinner className="h-5 w-5" />}
            {busy ? 'Redeeming…' : 'Redeem'}
          </button>
        </form>

        <AnimatePresence mode="wait" initial={false}>
          {outcome.kind === 'success' && (
            <motion.div
              key="success"
              role="status"
              variants={materializeVariants}
              initial="initial"
              animate="animate"
              exit="exit"
              transition={motionSafe(SPRING_UI, reduced)}
              className="mt-5 rounded-xl border border-blue-200 bg-blue-50 p-5 text-center"
            >
              <span className="mx-auto grid h-12 w-12 place-items-center rounded-full bg-linear-to-br from-blue-500 via-blue-400 to-blue-200 text-white shadow-sm shadow-blue-500/30">
                <CheckIcon className="h-6 w-6" />
              </span>
              <p className="mt-3 text-lg font-semibold text-neutral-900">
                Redeemed &mdash; {outcome.result.planName}
              </p>
              <p className="mt-1 text-sm text-neutral-600">
                {outcome.result.usagesRemaining == null
                  ? 'Unlimited redemptions remaining.'
                  : `${outcome.result.usagesRemaining} ${
                      outcome.result.usagesRemaining === 1 ? 'redemption' : 'redemptions'
                    } remaining.`}
              </p>
            </motion.div>
          )}

          {outcome.kind === 'error' && (
            <motion.div
              key="error"
              role="alert"
              variants={materializeVariants}
              initial="initial"
              animate="animate"
              exit="exit"
              transition={motionSafe(SPRING_UI, reduced)}
              className="mt-5 rounded-xl border border-red-200 bg-red-50 p-5 text-center"
            >
              <span
                aria-hidden="true"
                className="mx-auto grid h-12 w-12 place-items-center rounded-full border border-red-200 bg-white text-2xl font-semibold text-red-500"
              >
                !
              </span>
              <p className="mt-3 text-lg font-semibold text-red-700">
                Not redeemed
              </p>
              <p className="mt-1 text-sm text-red-600">{outcome.message}</p>
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    </section>
  )
}
