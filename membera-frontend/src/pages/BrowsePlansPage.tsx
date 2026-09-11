import { useEffect, useRef, useState, type ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { DashboardShell } from '@/components/DashboardShell'
import { CalendarIcon, ClockIcon, ImageIcon, RepeatIcon } from '@/components/icons'
import { PageHeading } from '@/components/PageHeading'
import { PageLoadingOverlay } from '@/components/PageLoadingOverlay'
import { Pagination } from '@/components/Pagination'
import { Spinner } from '@/components/Spinner'
import { TimelineAnimation } from '@/components/ui/hero-financial-utils/timeline-animation'
import { SPRING_UI } from '@/lib/motion'
import {
  BUSINESS_CATEGORIES,
  BUSINESS_CATEGORY_LABELS,
  formatDuration,
  formatPrice,
  formatTimeRange,
  formatUsageLimit,
  type BusinessCategory,
} from '@/lib/merchant'
import { BADGE_NEUTRAL, BTN_PRIMARY, CARD, ERROR_BANNER, SECTION_LABEL } from '@/lib/ui'
import {
  ApiError,
  browseActivePlans,
  checkoutSubscription,
  type BrowsePlan,
} from '@/lib/subscriptions'

type Status = 'loading' | 'ready' | 'error'
type CategoryFilter = BusinessCategory | 'All'

// Server-side pagination: GET /subscription-plans?page&pageSize returns just
// this page's plans plus a totalCount, so we ask for one page at a time
// instead of fetching every active plan up front.
// 9 = 3 full rows at the grid's widest (lg:grid-cols-3).
const PAGE_SIZE = 9

export default function BrowsePlansPage() {
  const sectionRef = useRef<HTMLElement>(null)
  const [status, setStatus] = useState<Status>('loading')
  const [plans, setPlans] = useState<BrowsePlan[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [page, setPage] = useState(1)
  const [category, setCategory] = useState<CategoryFilter>('All')
  // The page number whose data `plans`/`totalCount` currently reflect. While
  // it differs from `page` (e.g. right after clicking "Next"), a fetch for
  // the new page is in flight.
  const [loadedPage, setLoadedPage] = useState<number | null>(null)
  const [loadedCategory, setLoadedCategory] = useState<CategoryFilter | null>(null)
  const pageLoading =
    status === 'ready' && (loadedPage !== page || loadedCategory !== category)

  // Guards against a slower, earlier request clobbering a faster, later one
  // when the user changes pages quickly.
  const latestRequestRef = useRef(0)

  useEffect(() => {
    const requestId = ++latestRequestRef.current
    const categoryFilter = category === 'All' ? undefined : category
    browseActivePlans(page, PAGE_SIZE, categoryFilter).then(
      (data) => {
        if (requestId !== latestRequestRef.current) return
        setPlans(data.plans)
        setTotalCount(data.totalCount)
        setLoadedPage(page)
        setLoadedCategory(category)
        setStatus('ready')
      },
      (err) => {
        if (requestId !== latestRequestRef.current) return
        setLoadError(
          err instanceof ApiError
            ? err.message
            : 'Could not load subscription plans.',
        )
        setStatus('error')
      },
    )
  }, [page, category])

  const handleCategoryChange = (next: CategoryFilter) => {
    if (next === category) return
    setCategory(next)
    setPage(1)
  }

  const pageCount = Math.max(1, Math.ceil(totalCount / PAGE_SIZE))

  return (
    <DashboardShell>
      <PageHeading
        eyebrow="Browse plans"
        title="Find a plan to subscribe to"
        description="Buy a subscription once, then redeem it at the counter with a single scan — no app, no card, just your code."
      />

      <section
        ref={sectionRef}
        aria-labelledby="browse-plans-heading"
        className="mt-10"
      >
        <h2 id="browse-plans-heading" className={SECTION_LABEL}>
          Available plans
        </h2>

        <div className="mt-3 flex flex-wrap gap-2">
          <CategoryChip
            label="All"
            active={category === 'All'}
            onClick={() => handleCategoryChange('All')}
          />
          {BUSINESS_CATEGORIES.map((option) => (
            <CategoryChip
              key={option}
              label={BUSINESS_CATEGORY_LABELS[option]}
              active={category === option}
              onClick={() => handleCategoryChange(option)}
            />
          ))}
        </div>

        <div className="mt-4">
          {status === 'loading' && (
            <div className={`${CARD} p-6`}>
              <div className="flex items-center gap-3 text-sm text-neutral-500">
                <Spinner /> Loading plans…
              </div>
            </div>
          )}

          {status === 'error' && (
            <div className={`${CARD} p-6`}>
              <div className={ERROR_BANNER}>{loadError}</div>
            </div>
          )}

          {status === 'ready' && plans.length === 0 && (
            <div
              className={`${CARD} border-dashed border-neutral-300 p-8 text-center`}
            >
              <p className="text-sm font-medium text-neutral-900">
                No plans available right now
              </p>
              <p className="mx-auto mt-1 max-w-sm text-sm text-neutral-500">
                Check back soon — merchants are still setting up their
                subscription plans.
              </p>
            </div>
          )}

          {status === 'ready' && plans.length > 0 && (
            <>
              <PageLoadingOverlay loading={pageLoading}>
                <ul className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
                  {plans.map((plan, index) => (
                    <TimelineAnimation
                      as="li"
                      key={plan.id}
                      animationNum={index}
                      timelineRef={sectionRef}
                      whileHover={{ y: -4, transition: SPRING_UI }}
                    >
                      <BrowsePlanCard plan={plan} />
                    </TimelineAnimation>
                  ))}
                </ul>
              </PageLoadingOverlay>

              <Pagination
                page={page}
                pageCount={pageCount}
                onPageChange={setPage}
                className="mt-6"
              />
            </>
          )}
        </div>
      </section>

      <p className="mt-10 text-sm text-neutral-500">
        Already subscribed?{' '}
        <Link
          to="/dashboard"
          className="font-medium text-blue-600 transition-colors hover:text-blue-500"
        >
          View your subscriptions
        </Link>
        .
      </p>
    </DashboardShell>
  )
}

function BrowsePlanCard({ plan }: { plan: BrowsePlan }) {
  const [redirecting, setRedirecting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const merchantMonogram =
    plan.merchantBusinessName.trim().charAt(0).toUpperCase() || 'M'

  const handleSubscribe = async () => {
    setError(null)
    setRedirecting(true)
    try {
      const { checkoutUrl } = await checkoutSubscription(plan.id)
      // Leave the SPA for Stripe's hosted checkout page.
      window.location.href = checkoutUrl
    } catch (err) {
      setError(
        err instanceof ApiError
          ? err.message
          : 'Could not start checkout. Please try again.',
      )
      setRedirecting(false)
    }
  }

  return (
    <div className={`${CARD} flex flex-col`}>
      {/* Top: plan image (or a gradient placeholder). */}
      <div className="relative aspect-[3/2] overflow-hidden rounded-t-2xl bg-linear-to-br from-blue-100 via-blue-50 to-white">
        {plan.imageUrl ? (
          <img
            src={plan.imageUrl}
            alt=""
            className="h-full w-full object-cover"
          />
        ) : (
          <div className="flex h-full w-full flex-col items-center justify-center gap-2 text-blue-400">
            <ImageIcon className="h-8 w-8" />
          </div>
        )}
      </div>

      {/* Body: merchant, name / price, description, meta. */}
      <div className="flex flex-1 flex-col p-4">
        <div className="flex items-center gap-2 text-xs text-neutral-500">
          {plan.merchantLogoUrl ? (
            <img
              src={plan.merchantLogoUrl}
              alt=""
              className="h-5 w-5 rounded-full object-cover"
            />
          ) : (
            <span className="grid h-5 w-5 place-items-center rounded-full bg-linear-to-br from-blue-500 via-blue-400 to-blue-200 text-[10px] font-bold text-white">
              {merchantMonogram}
            </span>
          )}
          <span className="truncate font-medium text-neutral-700">
            {plan.merchantBusinessName}
          </span>
          <span className={`${BADGE_NEUTRAL} ml-auto shrink-0`}>
            {BUSINESS_CATEGORY_LABELS[plan.merchantBusinessCategory]}
          </span>
        </div>

        <div className="mt-2 flex items-baseline justify-between gap-2">
          <h3 className="truncate text-base font-semibold tracking-tight text-neutral-900">
            {plan.name}
          </h3>
          <span className="shrink-0 text-base font-semibold text-neutral-900">
            {formatPrice(plan.price)}
          </span>
        </div>

        {plan.description?.trim() && (
          <p className="mt-1 line-clamp-2 text-xs text-neutral-500">
            {plan.description}
          </p>
        )}

        <div className="mt-3 flex flex-wrap gap-x-3 gap-y-1 text-xs text-neutral-500">
          <Meta
            icon={<CalendarIcon className="h-3.5 w-3.5" />}
            value={formatDuration(plan.durationInDays)}
          />
          <Meta
            icon={<RepeatIcon className="h-3.5 w-3.5" />}
            value={formatUsageLimit(plan.usageLimit)}
          />
          <Meta
            icon={<ClockIcon className="h-3.5 w-3.5" />}
            value={formatTimeRange(plan.activeFrom, plan.activeUntil)}
          />
        </div>

        {error && (
          <p className="mt-3 text-xs text-red-600" role="alert">
            {error}
          </p>
        )}

        <button
          type="button"
          onClick={handleSubscribe}
          disabled={redirecting}
          className={`${BTN_PRIMARY} mt-4 w-full`}
        >
          {redirecting && <Spinner className="h-4 w-4" />}
          {redirecting ? 'Starting checkout…' : 'Subscribe'}
        </button>
      </div>
    </div>
  )
}

function CategoryChip({
  label,
  active,
  onClick,
}: {
  label: string
  active: boolean
  onClick: () => void
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      aria-pressed={active}
      className={
        active
          ? 'rounded-full border border-blue-300 bg-linear-to-br from-blue-500 via-blue-400 to-blue-200 px-3 py-1.5 text-sm font-medium text-white shadow-sm shadow-blue-500/30 transition'
          : 'rounded-full border border-neutral-200 bg-white px-3 py-1.5 text-sm font-medium text-neutral-600 shadow-sm transition hover:bg-neutral-50'
      }
    >
      {label}
    </button>
  )
}

function Meta({ icon, value }: { icon: ReactNode; value: string }) {
  return (
    <div className="flex items-center gap-1.5">
      <span className="text-neutral-400">{icon}</span>
      <span className="font-medium text-neutral-700">{value}</span>
    </div>
  )
}
