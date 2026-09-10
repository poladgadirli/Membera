import { useCallback, useEffect, useState } from 'react'
import { ConfirmDialog } from '@/components/ConfirmDialog'
import { Spinner } from '@/components/Spinner'
import { StatusBadge } from '@/components/StatusBadge'
import { PlanFormModal } from '@/components/merchant/PlanFormModal'
import { ApiError } from '@/lib/apiClient'
import {
  BTN_PRIMARY,
  BTN_SECONDARY_SM,
  CARD,
  ERROR_BANNER,
  SECTION_LABEL,
} from '@/lib/ui'
import {
  createPlan,
  deactivatePlan,
  formatDuration,
  formatPrice,
  formatTimeRange,
  formatUsageLimit,
  getMyPlans,
  updatePlan,
  type PlanInput,
  type SubscriptionPlan,
} from '@/lib/merchant'

const primaryButton = BTN_PRIMARY
const secondaryButton = BTN_SECONDARY_SM

type Status = 'loading' | 'ready' | 'error'
type FormTarget = { mode: 'create' } | { mode: 'edit'; plan: SubscriptionPlan }

export function PlansSection() {
  const [status, setStatus] = useState<Status>('loading')
  const [plans, setPlans] = useState<SubscriptionPlan[]>([])
  const [loadError, setLoadError] = useState<string | null>(null)

  const [formTarget, setFormTarget] = useState<FormTarget | null>(null)
  const [planToDeactivate, setPlanToDeactivate] =
    useState<SubscriptionPlan | null>(null)

  // All setState happens after `await`, so this is safe to call from an effect.
  const load = useCallback(async () => {
    try {
      const data = await getMyPlans()
      setPlans(data)
      setStatus('ready')
    } catch (err) {
      setLoadError(
        err instanceof ApiError
          ? err.message
          : 'Could not load your subscription plans.',
      )
      setStatus('error')
    }
  }, [])

  useEffect(() => {
    let active = true
    getMyPlans().then(
      (data) => {
        if (!active) return
        setPlans(data)
        setStatus('ready')
      },
      (err) => {
        if (!active) return
        setLoadError(
          err instanceof ApiError
            ? err.message
            : 'Could not load your subscription plans.',
        )
        setStatus('error')
      },
    )
    return () => {
      active = false
    }
  }, [])

  const retry = () => {
    setStatus('loading')
    setLoadError(null)
    void load()
  }

  const handleSubmit = async (input: PlanInput) => {
    if (formTarget?.mode === 'edit') {
      await updatePlan(formTarget.plan.id, input)
    } else {
      await createPlan(input)
    }
    await load()
  }

  return (
    <section aria-labelledby="plans-heading" className="mt-10">
      <div className="flex items-center justify-between gap-4">
        <h2 id="plans-heading" className={SECTION_LABEL}>
          Subscription plans
        </h2>
        <button
          type="button"
          onClick={() => setFormTarget({ mode: 'create' })}
          className={primaryButton}
        >
          Create new plan
        </button>
      </div>

      <div className="mt-3">
        {status === 'loading' && (
          <div className={`${CARD} p-6`}>
            <div className="flex items-center gap-3 text-sm text-neutral-500">
              <Spinner /> Loading plans…
            </div>
          </div>
        )}

        {status === 'error' && (
          <div className={`${CARD} space-y-3 p-6`}>
            <div className={ERROR_BANNER}>{loadError}</div>
            <button type="button" onClick={retry} className={secondaryButton}>
              Try again
            </button>
          </div>
        )}

        {status === 'ready' && plans.length === 0 && (
          <div className={`${CARD} border-dashed border-neutral-300 p-8 text-center`}>
            <p className="text-sm font-medium text-neutral-900">No plans yet</p>
            <p className="mx-auto mt-1 max-w-sm text-sm text-neutral-500">
              Create your first subscription plan so customers can buy and redeem
              it with a QR code.
            </p>
            <button
              type="button"
              onClick={() => setFormTarget({ mode: 'create' })}
              className={`${primaryButton} mt-4`}
            >
              Create new plan
            </button>
          </div>
        )}

        {status === 'ready' && plans.length > 0 && (
          <ul className="space-y-3">
            {plans.map((plan) => (
              <li key={plan.id} className={`${CARD} p-5`}>
                <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
                  <div className="min-w-0">
                    <div className="flex flex-wrap items-center gap-2">
                      <h3 className="text-base font-semibold tracking-tight text-neutral-900">
                        {plan.name}
                      </h3>
                      <StatusBadge active={plan.isActive} />
                    </div>
                    {plan.description?.trim() && (
                      <p className="mt-1 max-w-prose text-sm text-neutral-500">
                        {plan.description}
                      </p>
                    )}
                    <dl className="mt-3 flex flex-wrap gap-x-6 gap-y-1 text-sm">
                      <Detail label="Price" value={formatPrice(plan.price)} />
                      <Detail
                        label="Duration"
                        value={formatDuration(plan.durationInDays)}
                      />
                      <Detail
                        label="Usage"
                        value={formatUsageLimit(plan.usageLimit)}
                      />
                      <Detail
                        label="Active hours"
                        value={formatTimeRange(plan.activeFrom, plan.activeUntil)}
                      />
                    </dl>
                  </div>

                  <div className="flex shrink-0 items-center gap-2">
                    <button
                      type="button"
                      onClick={() => setFormTarget({ mode: 'edit', plan })}
                      className={secondaryButton}
                    >
                      Edit
                    </button>
                    <button
                      type="button"
                      onClick={() => setPlanToDeactivate(plan)}
                      disabled={!plan.isActive}
                      className={`${secondaryButton} disabled:cursor-not-allowed disabled:opacity-50`}
                    >
                      Deactivate
                    </button>
                  </div>
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>

      {formTarget && (
        <PlanFormModal
          plan={formTarget.mode === 'edit' ? formTarget.plan : null}
          onClose={() => setFormTarget(null)}
          onSubmit={handleSubmit}
        />
      )}

      {planToDeactivate && (
        <ConfirmDialog
          open
          title="Deactivate this plan?"
          description={`"${planToDeactivate.name}" will stop being available for new customers to buy. Existing subscriptions are not affected.`}
          confirmLabel="Deactivate"
          destructive
          onConfirm={async () => {
            await deactivatePlan(planToDeactivate.id)
            await load()
          }}
          onClose={() => setPlanToDeactivate(null)}
        />
      )}
    </section>
  )
}

function Detail({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex items-baseline gap-1.5">
      <dt className="text-neutral-500">{label}</dt>
      <dd className="font-medium text-neutral-900">{value}</dd>
    </div>
  )
}
