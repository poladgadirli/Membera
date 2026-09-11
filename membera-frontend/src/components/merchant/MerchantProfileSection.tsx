import {
  useCallback,
  useEffect,
  useId,
  useRef,
  useState,
  type ChangeEvent,
  type FormEvent,
  type ReactNode,
} from 'react'
import { motion } from 'motion/react'
import { CameraIcon, ChevronDownIcon, PencilIcon } from '@/components/icons'
import { Spinner } from '@/components/Spinner'
import { StatusBadge } from '@/components/StatusBadge'
import { ApiError } from '@/lib/apiClient'
import { SPRING_UI } from '@/lib/motion'
import {
  BTN_PRIMARY,
  BTN_SECONDARY,
  CARD,
  ERROR_BANNER,
  INPUT,
  LABEL,
  SECTION_LABEL,
} from '@/lib/ui'
import {
  createMerchant,
  getMyMerchant,
  isMerchantNotFound,
  updateMerchant,
  uploadMerchantLogo,
  type MerchantProfile,
} from '@/lib/merchant'

const fieldClass = INPUT
const labelClass = LABEL
const primaryButton = BTN_PRIMARY
const secondaryButton = BTN_SECONDARY

type Status = 'loading' | 'setup' | 'ready' | 'error'

function ErrorNote({ message }: { message: string }) {
  return <div className={ERROR_BANNER}>{message}</div>
}

/** Two-letter monogram from the business name, for the avatar fallback. */
function initials(name: string): string {
  const words = name.trim().split(/\s+/).filter(Boolean)
  if (words.length === 0) return '—'
  if (words.length === 1) return words[0].slice(0, 2).toUpperCase()
  return (words[0][0] + words[words.length - 1][0]).toUpperCase()
}

/** Small, low-emphasis icon button for the profile card's secondary actions. */
function IconAction({
  label,
  onClick,
  disabled,
  children,
}: {
  label: string
  onClick: () => void
  disabled?: boolean
  children: ReactNode
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      disabled={disabled}
      aria-label={label}
      title={label}
      className="rounded-lg p-2 text-neutral-400 transition-colors hover:bg-neutral-100 hover:text-neutral-900 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-500 disabled:cursor-not-allowed disabled:opacity-50"
    >
      {children}
    </button>
  )
}

export function MerchantProfileSection() {
  const [status, setStatus] = useState<Status>('loading')
  const [profile, setProfile] = useState<MerchantProfile | null>(null)
  const [loadError, setLoadError] = useState<string | null>(null)

  const [editing, setEditing] = useState(false)
  const [businessName, setBusinessName] = useState('')
  const [description, setDescription] = useState('')
  const [formError, setFormError] = useState<string | null>(null)
  const [saving, setSaving] = useState(false)

  const logoInputRef = useRef<HTMLInputElement>(null)
  const [logoUploading, setLogoUploading] = useState(false)
  const [logoError, setLogoError] = useState<string | null>(null)

  // The description is hidden until the card is hovered (desktop) or the card /
  // chevron is tapped (mobile). `expanded` drives the tap path; hover/focus are
  // handled with group-* variants so they need no state.
  const [expanded, setExpanded] = useState(false)
  const detailsId = useId()

  // All setState happens after `await`, so this is safe to call from an effect.
  const load = useCallback(async () => {
    try {
      const data = await getMyMerchant()
      setProfile(data)
      setStatus('ready')
    } catch (err) {
      if (isMerchantNotFound(err)) {
        setProfile(null)
        setStatus('setup')
        return
      }
      setLoadError(
        err instanceof ApiError
          ? err.message
          : 'Could not load your business profile.',
      )
      setStatus('error')
    }
  }, [])

  useEffect(() => {
    let active = true
    getMyMerchant().then(
      (data) => {
        if (!active) return
        setProfile(data)
        setStatus('ready')
      },
      (err) => {
        if (!active) return
        if (isMerchantNotFound(err)) {
          setProfile(null)
          setStatus('setup')
          return
        }
        setLoadError(
          err instanceof ApiError
            ? err.message
            : 'Could not load your business profile.',
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

  const startEditing = () => {
    setBusinessName(profile?.businessName ?? '')
    setDescription(profile?.description ?? '')
    setFormError(null)
    setEditing(true)
  }

  const handleCreate = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setFormError(null)
    const name = businessName.trim()
    if (!name) return setFormError('Enter your business name.')

    setSaving(true)
    try {
      await createMerchant(name)
      setBusinessName('')
      setDescription('')
      await load()
    } catch (err) {
      setFormError(
        err instanceof ApiError
          ? err.message
          : 'Could not create your business profile.',
      )
    } finally {
      setSaving(false)
    }
  }

  const handleLogoChange = async (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    event.target.value = '' // let the same file be re-picked later
    if (!file) return

    setLogoError(null)
    setLogoUploading(true)
    try {
      await uploadMerchantLogo(file)
      await load()
    } catch (err) {
      setLogoError(
        err instanceof ApiError ? err.message : 'Could not upload the logo.',
      )
    } finally {
      setLogoUploading(false)
    }
  }

  const handleUpdate = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setFormError(null)
    const name = businessName.trim()
    if (!name) return setFormError('Business name cannot be empty.')

    setSaving(true)
    try {
      await updateMerchant({ businessName: name, description: description.trim() })
      setEditing(false)
      await load()
    } catch (err) {
      setFormError(
        err instanceof ApiError
          ? err.message
          : 'Could not save your changes.',
      )
    } finally {
      setSaving(false)
    }
  }

  return (
    <section aria-labelledby="business-profile-heading">
      <h2 id="business-profile-heading" className={SECTION_LABEL}>
        Business profile
      </h2>

      <div className="mt-3">
        {status === 'loading' && (
          <div className={`${CARD} p-6`}>
            <div className="flex items-center gap-3 text-sm text-neutral-500">
              <Spinner /> Loading your business profile…
            </div>
          </div>
        )}

        {status === 'error' && (
          <div className={`${CARD} space-y-3 p-6`}>
            <ErrorNote message={loadError ?? 'Something went wrong.'} />
            <button type="button" onClick={retry} className={secondaryButton}>
              Try again
            </button>
          </div>
        )}

        {status === 'setup' && (
          <div className={`${CARD} p-6`}>
            <form onSubmit={handleCreate} className="space-y-4">
              <div>
                <h3 className="text-base font-semibold text-neutral-900">
                  Set up your business profile
                </h3>
                <p className="mt-1 text-sm text-neutral-500">
                  Add your business name to start creating subscription plans.
                </p>
              </div>

              {formError && <ErrorNote message={formError} />}

              <div>
                <label htmlFor="setup-business-name" className={labelClass}>
                  Business name
                </label>
                <input
                  id="setup-business-name"
                  className={fieldClass}
                  value={businessName}
                  onChange={(e) => setBusinessName(e.target.value)}
                  placeholder="Blue Bottle Coffee"
                  maxLength={120}
                />
              </div>

              <button type="submit" disabled={saving} className={primaryButton}>
                {saving && <Spinner className="h-4 w-4" />}
                Create profile
              </button>
            </form>
          </div>
        )}

        {status === 'ready' && profile && !editing && (
          <motion.div
            onClick={(e) => {
              // Mobile: a tap anywhere on the card that isn't a control toggles
              // the description. Desktop reveal is pure CSS (group-hover).
              if ((e.target as HTMLElement).closest('button, a, input')) return
              setExpanded((v) => !v)
            }}
            className={`${CARD} group relative cursor-default p-6 hover:shadow-md hover:shadow-blue-500/10`}
            whileHover={{ y: -4 }}
            transition={SPRING_UI}
          >
            <div className="flex items-start gap-5">
              {/* Circular profile avatar */}
              {profile.logoUrl ? (
                <img
                  src={profile.logoUrl}
                  alt=""
                  className="h-20 w-20 shrink-0 rounded-full object-cover shadow-sm ring-4 ring-white sm:h-24 sm:w-24"
                />
              ) : (
                <div className="grid h-20 w-20 shrink-0 place-items-center rounded-full bg-linear-to-br from-blue-500 via-blue-400 to-blue-200 text-2xl font-semibold text-white shadow-sm shadow-blue-500/30 ring-4 ring-white sm:h-24 sm:w-24">
                  {initials(profile.businessName)}
                </div>
              )}

              <div className="min-w-0 flex-1 pt-1">
                <div className="flex flex-wrap items-center gap-x-3 gap-y-1.5">
                  <h3 className="text-xl font-semibold tracking-tight text-neutral-900">
                    {profile.businessName}
                  </h3>
                  <StatusBadge active={profile.isActive} />
                  <button
                    type="button"
                    onClick={() => setExpanded((v) => !v)}
                    aria-expanded={expanded}
                    aria-controls={detailsId}
                    className="ml-auto inline-flex items-center gap-1 rounded-md px-1.5 py-1 text-xs font-medium text-neutral-400 transition-colors hover:text-neutral-700 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-500 sm:ml-0"
                  >
                    Details
                    <ChevronDownIcon
                      className={`h-3.5 w-3.5 transition-transform duration-300 group-hover:rotate-180 ${
                        expanded ? 'rotate-180' : ''
                      }`}
                    />
                  </button>
                </div>

                {/* Collapsible description: 0fr → 1fr grid-row animates height. */}
                <div
                  id={detailsId}
                  className={`grid transition-[grid-template-rows] duration-300 ease-out group-hover:grid-rows-[1fr] group-focus-within:grid-rows-[1fr] ${
                    expanded ? 'grid-rows-[1fr]' : 'grid-rows-[0fr]'
                  }`}
                >
                  <div className="overflow-hidden">
                    <p className="max-w-prose pt-2 text-sm leading-relaxed text-neutral-500">
                      {profile.description?.trim()
                        ? profile.description
                        : 'No description yet.'}
                    </p>
                  </div>
                </div>

                {logoError && (
                  <p className="mt-2 text-xs text-red-600">{logoError}</p>
                )}
              </div>

              {/* Secondary actions — deliberately quiet next to the name. */}
              <div className="flex shrink-0 items-center gap-0.5">
                <IconAction
                  label={
                    logoUploading
                      ? 'Uploading logo…'
                      : profile.logoUrl
                        ? 'Change logo'
                        : 'Upload logo'
                  }
                  onClick={() => logoInputRef.current?.click()}
                  disabled={logoUploading}
                >
                  {logoUploading ? (
                    <Spinner className="h-4 w-4" />
                  ) : (
                    <CameraIcon className="h-4 w-4" />
                  )}
                </IconAction>
                <IconAction label="Edit profile" onClick={startEditing}>
                  <PencilIcon className="h-4 w-4" />
                </IconAction>
              </div>
            </div>

            <input
              ref={logoInputRef}
              type="file"
              accept="image/*"
              className="hidden"
              onChange={handleLogoChange}
            />
          </motion.div>
        )}

        {status === 'ready' && profile && editing && (
          <div className={`${CARD} p-6`}>
            <form onSubmit={handleUpdate} className="space-y-4">
              <h3 className="text-base font-semibold text-neutral-900">
                Edit business profile
              </h3>

              {formError && <ErrorNote message={formError} />}

              <div>
                <label htmlFor="edit-business-name" className={labelClass}>
                  Business name
                </label>
                <input
                  id="edit-business-name"
                  className={fieldClass}
                  value={businessName}
                  onChange={(e) => setBusinessName(e.target.value)}
                  maxLength={120}
                />
              </div>

              <div>
                <label htmlFor="edit-business-description" className={labelClass}>
                  Description{' '}
                  <span className="font-normal text-neutral-500">
                    (optional)
                  </span>
                </label>
                <textarea
                  id="edit-business-description"
                  className={`${fieldClass} min-h-20 resize-y`}
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  maxLength={500}
                />
              </div>

              <div className="flex items-center gap-3">
                <button type="submit" disabled={saving} className={primaryButton}>
                  {saving && <Spinner className="h-4 w-4" />}
                  Save changes
                </button>
                <button
                  type="button"
                  onClick={() => setEditing(false)}
                  disabled={saving}
                  className={secondaryButton}
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        )}
      </div>
    </section>
  )
}
