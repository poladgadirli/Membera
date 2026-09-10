import {
  useCallback,
  useEffect,
  useRef,
  useState,
  type ChangeEvent,
  type ReactNode,
} from 'react'
import { ConfirmDialog } from '@/components/ConfirmDialog'
import {
  CalendarIcon,
  CameraIcon,
  ClockIcon,
  ImageIcon,
  PencilIcon,
  RepeatIcon,
} from '@/components/icons'
import { Spinner } from '@/components/Spinner'
import { StatusBadge } from '@/components/StatusBadge'
import { PlanFormModal } from '@/components/merchant/PlanFormModal'
import { ApiError } from '@/lib/apiClient'
import { BTN_PRIMARY, CARD, ERROR_BANNER, SECTION_LABEL } from '@/lib/ui'
import {
  createPlan,
  deactivatePlan,
  formatDuration,
  formatPrice,
  formatTimeRange,
  formatUsageLimit,
  getMyPlans,
  updatePlan,
  uploadSubscriptionPlanImage,
  type PlanInput,
  type SubscriptionPlan,
} from '@/lib/merchant'

const primaryButton = BTN_PRIMARY

/** Quiet text button for a plan card's secondary actions. */
const cardAction =
  'inline-flex items-center gap-1.5 rounded-md px-2 py-1 text-xs font-medium text-neutral-500 transition-colors hover:bg-neutral-100 hover:text-neutral-900 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-500 disabled:cursor-not-allowed disabled:opacity-50'

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
            <button
              type="button"
              onClick={retry}
              className="inline-flex items-center justify-center gap-2 rounded-lg border border-neutral-200 bg-white px-3 py-1.5 text-sm font-semibold text-neutral-900 shadow-sm transition hover:bg-neutral-50 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-500"
            >
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
          <ul className="grid gap-4">
            {plans.map((plan) => (
              <li
                key={plan.id}
                className={`${CARD} group flex flex-col overflow-hidden transition-all duration-300 ease-out hover:-translate-y-0.5 hover:shadow-md hover:shadow-blue-500/10 sm:flex-row`}
              >
                {/* Prominent product image — ~34% width on desktop, full-bleed on mobile. */}
                <div className="relative aspect-[16/10] shrink-0 overflow-hidden bg-linear-to-br from-blue-100 via-blue-50 to-white sm:aspect-auto sm:w-[34%] sm:max-w-[300px]">
                  {plan.imageUrl ? (
                    <img
                      src={plan.imageUrl}
                      alt=""
                      className="h-full w-full object-cover transition-transform duration-500 ease-out group-hover:scale-105"
                    />
                  ) : (
                    <div className="grid h-full w-full place-items-center text-blue-300">
                      <ImageIcon className="h-10 w-10" />
                    </div>
                  )}
                </div>

                {/* Details */}
                <div className="flex min-w-0 flex-1 flex-col p-5 sm:p-6">
                  <div className="flex flex-wrap items-start gap-x-3 gap-y-1.5">
                    <h3 className="text-lg font-semibold tracking-tight text-neutral-900">
                      {plan.name}
                    </h3>
                    <StatusBadge active={plan.isActive} />
                  </div>

                  {plan.description?.trim() && (
                    <p className="mt-1.5 line-clamp-2 max-w-prose text-sm leading-relaxed text-neutral-500">
                      {plan.description}
                    </p>
                  )}

                  <p className="mt-3 text-2xl font-semibold tracking-tight text-neutral-900">
                    {formatPrice(plan.price)}
                    <span className="ml-1.5 text-sm font-medium text-neutral-400">
                      / {formatDuration(plan.durationInDays)}
                    </span>
                  </p>

                  <dl className="mt-3 flex flex-wrap gap-x-5 gap-y-1.5 text-sm text-neutral-500">
                    <Meta
                      icon={<CalendarIcon className="h-4 w-4" />}
                      label="Duration"
                      value={formatDuration(plan.durationInDays)}
                    />
                    <Meta
                      icon={<RepeatIcon className="h-4 w-4" />}
                      label="Usage"
                      value={formatUsageLimit(plan.usageLimit)}
                    />
                    <Meta
                      icon={<ClockIcon className="h-4 w-4" />}
                      label="Active hours"
                      value={formatTimeRange(plan.activeFrom, plan.activeUntil)}
                    />
                  </dl>

                  {/* Secondary actions — small and quiet under the content. */}
                  <div className="mt-auto flex flex-wrap items-center gap-1 pt-4">
                    <PlanImageButton plan={plan} onUploaded={load} />
                    <button
                      type="button"
                      onClick={() => setFormTarget({ mode: 'edit', plan })}
                      className={cardAction}
                    >
                      <PencilIcon className="h-4 w-4" />
                      Edit
                    </button>
                    <button
                      type="button"
                      onClick={() => setPlanToDeactivate(plan)}
                      disabled={!plan.isActive}
                      className={`${cardAction} hover:bg-red-50 hover:text-red-600`}
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

function PlanImageButton({
  plan,
  onUploaded,
}: {
  plan: SubscriptionPlan
  onUploaded: () => Promise<void> | void
}) {
  const inputRef = useRef<HTMLInputElement>(null)
  const [uploading, setUploading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleChange = async (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    event.target.value = '' // let the same file be re-picked later
    if (!file) return

    setError(null)
    setUploading(true)
    try {
      await uploadSubscriptionPlanImage(plan.id, file)
      await onUploaded()
    } catch (err) {
      setError(
        err instanceof ApiError ? err.message : 'Could not upload the image.',
      )
    } finally {
      setUploading(false)
    }
  }

  return (
    <>
      <button
        type="button"
        onClick={() => inputRef.current?.click()}
        disabled={uploading}
        className="inline-flex items-center gap-1.5 rounded-md px-2 py-1 text-xs font-medium text-neutral-500 transition-colors hover:bg-neutral-100 hover:text-neutral-900 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-500 disabled:cursor-not-allowed disabled:opacity-50"
      >
        {uploading ? (
          <Spinner className="h-3.5 w-3.5" />
        ) : (
          <CameraIcon className="h-4 w-4" />
        )}
        {uploading
          ? 'Uploading…'
          : plan.imageUrl
            ? 'Change image'
            : 'Add image'}
      </button>
      {error && (
        <p className="w-full text-xs text-red-600" role="alert">
          {error}
        </p>
      )}
      <input
        ref={inputRef}
        type="file"
        accept="image/*"
        className="hidden"
        onChange={handleChange}
      />
    </>
  )
}

function Meta({
  icon,
  label,
  value,
}: {
  icon: ReactNode
  label: string
  value: string
}) {
  return (
    <div className="flex items-center gap-1.5" title={label}>
      <span className="text-neutral-400">{icon}</span>
      <span className="font-medium text-neutral-700">{value}</span>
    </div>
  )
}
