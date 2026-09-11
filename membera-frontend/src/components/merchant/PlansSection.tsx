import { useCallback, useEffect, useRef, useState, type ReactNode } from 'react'
import { motion } from 'motion/react'
import { ConfirmDialog } from '@/components/ConfirmDialog'
import {
  CalendarIcon,
  ClockIcon,
  ImageIcon,
  MoreVerticalIcon,
  PencilIcon,
  PowerIcon,
  RepeatIcon,
} from '@/components/icons'
import { Spinner } from '@/components/Spinner'
import { StatusBadge } from '@/components/StatusBadge'
import { PlanFormModal } from '@/components/merchant/PlanFormModal'
import { ApiError } from '@/lib/apiClient'
import { SPRING_UI } from '@/lib/motion'
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
  type PlanInput,
  type SubscriptionPlan,
} from '@/lib/merchant'

const primaryButton = BTN_PRIMARY

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

  const handleSubmit = (input: PlanInput): Promise<SubscriptionPlan> => {
    return formTarget?.mode === 'edit'
      ? updatePlan(formTarget.plan.id, input)
      : createPlan(input)
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
          <ul className="grid gap-5 sm:grid-cols-2">
            {plans.map((plan) => (
              <PlanCard
                key={plan.id}
                plan={plan}
                onEdit={() => setFormTarget({ mode: 'edit', plan })}
                onDeactivate={() => setPlanToDeactivate(plan)}
              />
            ))}
          </ul>
        )}
      </div>

      {formTarget && (
        <PlanFormModal
          plan={formTarget.mode === 'edit' ? formTarget.plan : null}
          onClose={() => setFormTarget(null)}
          onSubmit={handleSubmit}
          onSaved={load}
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

function PlanCard({
  plan,
  onEdit,
  onDeactivate,
}: {
  plan: SubscriptionPlan
  onEdit: () => void
  onDeactivate: () => void
}) {
  const [menuOpen, setMenuOpen] = useState(false)
  const menuRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!menuOpen) return
    const onDocPointer = (event: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setMenuOpen(false)
      }
    }
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') setMenuOpen(false)
    }
    document.addEventListener('mousedown', onDocPointer)
    document.addEventListener('keydown', onKeyDown)
    return () => {
      document.removeEventListener('mousedown', onDocPointer)
      document.removeEventListener('keydown', onKeyDown)
    }
  }, [menuOpen])

  const monogram = plan.name.trim().charAt(0).toUpperCase() || 'P'

  return (
    <motion.li
      className={`${CARD} group relative flex flex-col hover:shadow-md hover:shadow-blue-500/10`}
      whileHover={{ y: -4 }}
      transition={SPRING_UI}
    >
      {/* Top ~70%: the plan image, or a placeholder. Image management now
          lives entirely in the Edit form (PlanFormModal) — this is display-only. */}
      <div className="relative aspect-[3/2] overflow-hidden rounded-t-2xl bg-linear-to-br from-blue-100 via-blue-50 to-white">
        {plan.imageUrl ? (
          <img
            src={plan.imageUrl}
            alt=""
            className="h-full w-full object-cover transition-transform duration-500 ease-out group-hover:scale-105"
          />
        ) : (
          <div className="flex h-full w-full flex-col items-center justify-center gap-2 text-blue-300">
            <ImageIcon className="h-8 w-8" />
          </div>
        )}

        <span className="absolute left-3 top-3 z-10 drop-shadow-sm">
          <StatusBadge active={plan.isActive} />
        </span>
      </div>

      {/* Overflow menu — kept outside the image's overflow-hidden so it can drop down. */}
      <div ref={menuRef} className="absolute right-3 top-3 z-20">
        <button
          type="button"
          onClick={() => setMenuOpen((v) => !v)}
          aria-haspopup="menu"
          aria-expanded={menuOpen}
          aria-label="Plan actions"
          className="grid h-8 w-8 place-items-center rounded-full bg-white/85 text-neutral-700 shadow-sm backdrop-blur transition duration-150 ease-out active:scale-[0.9] hover:bg-white focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-500"
        >
          <MoreVerticalIcon className="h-4 w-4" />
        </button>

        {menuOpen && (
          <div
            role="menu"
            className="absolute right-0 top-full mt-1.5 w-48 overflow-hidden rounded-xl border border-neutral-200 bg-white py-1 text-sm shadow-lg shadow-blue-500/10"
          >
            <MenuItem
              icon={<PencilIcon className="h-4 w-4" />}
              onClick={() => {
                setMenuOpen(false)
                onEdit()
              }}
            >
              Edit plan
            </MenuItem>
            <MenuItem
              icon={<PowerIcon className="h-4 w-4" />}
              destructive
              disabled={!plan.isActive}
              onClick={() => {
                setMenuOpen(false)
                onDeactivate()
              }}
            >
              Deactivate
            </MenuItem>
          </div>
        )}
      </div>

      {/* Bottom ~30%: author-style avatar + plan name / price / details. */}
      <div className="flex items-start gap-3 p-4">
        <div className="grid h-10 w-10 shrink-0 place-items-center rounded-full bg-linear-to-br from-blue-500 via-blue-400 to-blue-200 text-sm font-bold text-white shadow-sm shadow-blue-500/30">
          {monogram}
        </div>

        <div className="min-w-0 flex-1">
          <div className="flex items-baseline justify-between gap-2">
            <h3 className="truncate text-base font-semibold tracking-tight text-neutral-900">
              {plan.name}
            </h3>
            <span className="shrink-0 text-base font-semibold text-neutral-900">
              {formatPrice(plan.price)}
            </span>
          </div>

          {plan.description?.trim() && (
            <p className="mt-0.5 line-clamp-1 text-xs text-neutral-500">
              {plan.description}
            </p>
          )}

          <div className="mt-2 flex flex-wrap gap-x-3 gap-y-1 text-xs text-neutral-500">
            <Meta
              icon={<CalendarIcon className="h-3.5 w-3.5" />}
              label="Duration"
              value={formatDuration(plan.durationInDays)}
            />
            <Meta
              icon={<RepeatIcon className="h-3.5 w-3.5" />}
              label="Usage"
              value={formatUsageLimit(plan.usageLimit)}
            />
            <Meta
              icon={<ClockIcon className="h-3.5 w-3.5" />}
              label="Active hours"
              value={formatTimeRange(plan.activeFrom, plan.activeUntil)}
            />
          </div>
        </div>
      </div>
    </motion.li>
  )
}

function MenuItem({
  icon,
  children,
  onClick,
  disabled,
  destructive,
}: {
  icon: ReactNode
  children: ReactNode
  onClick: () => void
  disabled?: boolean
  destructive?: boolean
}) {
  return (
    <button
      type="button"
      role="menuitem"
      onClick={onClick}
      disabled={disabled}
      className={`flex w-full items-center gap-2.5 px-3 py-2 text-left transition-colors disabled:cursor-not-allowed disabled:opacity-40 ${
        destructive
          ? 'text-red-600 hover:bg-red-50'
          : 'text-neutral-700 hover:bg-neutral-50'
      }`}
    >
      <span className={destructive ? 'text-red-400' : 'text-neutral-400'}>
        {icon}
      </span>
      {children}
    </button>
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
