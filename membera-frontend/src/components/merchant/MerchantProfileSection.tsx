import {
  useCallback,
  useEffect,
  useRef,
  useState,
  type ChangeEvent,
  type FormEvent,
} from 'react'
import { Spinner } from '@/components/Spinner'
import { StatusBadge } from '@/components/StatusBadge'
import { ApiError } from '@/lib/apiClient'
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

      <div className={`mt-3 ${CARD} p-6`}>
        {status === 'loading' && (
          <div className="flex items-center gap-3 text-sm text-neutral-500">
            <Spinner /> Loading your business profile…
          </div>
        )}

        {status === 'error' && (
          <div className="space-y-3">
            <ErrorNote message={loadError ?? 'Something went wrong.'} />
            <button type="button" onClick={retry} className={secondaryButton}>
              Try again
            </button>
          </div>
        )}

        {status === 'setup' && (
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
        )}

        {status === 'ready' && profile && !editing && (
          <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
            <div className="flex min-w-0 gap-4">
              {profile.logoUrl ? (
                <img
                  src={profile.logoUrl}
                  alt=""
                  className="h-14 w-14 shrink-0 rounded-xl border border-neutral-200 object-cover"
                />
              ) : (
                <div className="grid h-14 w-14 shrink-0 place-items-center rounded-xl border border-dashed border-neutral-300 bg-neutral-50 text-[11px] font-medium text-neutral-400">
                  Logo
                </div>
              )}

              <div className="min-w-0">
                <div className="flex flex-wrap items-center gap-2">
                  <h3 className="text-lg font-semibold tracking-tight text-neutral-900">
                    {profile.businessName}
                  </h3>
                  <StatusBadge active={profile.isActive} />
                </div>
                <p className="mt-1 max-w-prose text-sm text-neutral-500">
                  {profile.description?.trim()
                    ? profile.description
                    : 'No description yet.'}
                </p>

                <div className="mt-2">
                  <button
                    type="button"
                    onClick={() => logoInputRef.current?.click()}
                    disabled={logoUploading}
                    className="inline-flex items-center gap-1.5 text-sm font-medium text-blue-600 transition-colors hover:text-blue-500 disabled:opacity-60"
                  >
                    {logoUploading && <Spinner className="h-3.5 w-3.5" />}
                    {logoUploading
                      ? 'Uploading…'
                      : profile.logoUrl
                        ? 'Change logo'
                        : 'Upload logo'}
                  </button>
                </div>
                {logoError && (
                  <div className="mt-2">
                    <ErrorNote message={logoError} />
                  </div>
                )}
                <input
                  ref={logoInputRef}
                  type="file"
                  accept="image/*"
                  className="hidden"
                  onChange={handleLogoChange}
                />
              </div>
            </div>
            <button
              type="button"
              onClick={startEditing}
              className={`${secondaryButton} shrink-0`}
            >
              Edit
            </button>
          </div>
        )}

        {status === 'ready' && profile && editing && (
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
        )}
      </div>
    </section>
  )
}
