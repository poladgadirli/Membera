import { useEffect, useRef, useState, type ChangeEvent, type FormEvent } from 'react'
import { AnimatePresence, motion } from 'motion/react'
import { Modal } from '@/components/Modal'
import { Spinner } from '@/components/Spinner'
import { CameraIcon, ImageIcon } from '@/components/icons'
import { ApiError } from '@/lib/apiClient'
import { SPRING_UI, materializeVariants, motionSafe, usePrefersReducedMotion } from '@/lib/motion'
import {
  BTN_PRIMARY,
  BTN_SECONDARY,
  ERROR_BANNER,
  INPUT,
  LABEL,
  WARNING_BANNER,
} from '@/lib/ui'
import {
  toApiTime,
  toInputTime,
  uploadSubscriptionPlanImage,
  type PlanInput,
  type SubscriptionPlan,
} from '@/lib/merchant'

const fieldClass = INPUT
const labelClass = LABEL

interface FormValues {
  name: string
  description: string
  price: string
  durationInDays: string
  usageLimit: string
  noUsageLimit: boolean
  activeFrom: string
  activeUntil: string
}

function initialValues(plan: SubscriptionPlan | null): FormValues {
  return {
    name: plan?.name ?? '',
    description: plan?.description ?? '',
    price: plan != null ? String(plan.price) : '',
    durationInDays: plan != null ? String(plan.durationInDays) : '',
    usageLimit:
      plan?.usageLimit != null && plan.usageLimit > 0
        ? String(plan.usageLimit)
        : '',
    noUsageLimit: plan != null ? plan.usageLimit == null || plan.usageLimit <= 0 : true,
    activeFrom: toInputTime(plan?.activeFrom) || '09:00',
    activeUntil: toInputTime(plan?.activeUntil) || '17:00',
  }
}

interface PlanFormModalProps {
  /** The plan being edited, or null to create a new one. */
  plan: SubscriptionPlan | null
  onClose: () => void
  /** Creates or updates the plan's fields and returns the saved record (with its id). */
  onSubmit: (input: PlanInput) => Promise<SubscriptionPlan>
  /** Called once the plan (and its image, if one was picked) is fully saved, so the parent can refresh its list. */
  onSaved: () => Promise<void> | void
}

// This component is mounted only while the modal is open (see PlansSection), so
// useState initialisers give every open a fresh form without a reset effect.
export function PlanFormModal({ plan, onClose, onSubmit, onSaved }: PlanFormModalProps) {
  const isEdit = plan != null
  const [values, setValues] = useState<FormValues>(() => initialValues(plan))
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  const [imageFile, setImageFile] = useState<File | null>(null)
  const [imagePreviewUrl, setImagePreviewUrl] = useState<string | null>(null)
  // Set once the plan's fields have been saved this session — distinguishes
  // "haven't saved yet" from "saved, only the image upload is outstanding".
  const [savedPlan, setSavedPlan] = useState<SubscriptionPlan | null>(null)
  const [imageError, setImageError] = useState<string | null>(null)

  useEffect(() => {
    if (!imagePreviewUrl) return
    return () => URL.revokeObjectURL(imagePreviewUrl)
  }, [imagePreviewUrl])

  const set = <K extends keyof FormValues>(key: K, value: FormValues[K]) =>
    setValues((current) => ({ ...current, [key]: value }))

  const pickImage = (file: File) => {
    setImageFile(file)
    setImagePreviewUrl(URL.createObjectURL(file))
  }

  const displayImageUrl = imagePreviewUrl ?? plan?.imageUrl ?? null

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setError(null)
    setImageError(null)

    let saved = savedPlan

    if (!saved) {
      const name = values.name.trim()
      const price = Number(values.price)
      const durationInDays = Number(values.durationInDays)
      const usageLimit = values.noUsageLimit ? null : Number(values.usageLimit)

      if (!name) return setError('Give the plan a name.')
      if (!Number.isFinite(price) || price < 0)
        return setError('Enter a valid price.')
      if (!Number.isInteger(durationInDays) || durationInDays <= 0)
        return setError('Duration must be a whole number of days greater than 0.')
      if (
        !values.noUsageLimit &&
        (!Number.isInteger(usageLimit as number) || (usageLimit as number) <= 0)
      )
        return setError('Usage limit must be a whole number greater than 0.')
      if (!values.activeFrom || !values.activeUntil)
        return setError('Set the active hours for the plan.')

      setSubmitting(true)
      try {
        saved = await onSubmit({
          name,
          description: values.description.trim(),
          price,
          durationInDays,
          usageLimit,
          activeFrom: toApiTime(values.activeFrom),
          activeUntil: toApiTime(values.activeUntil),
        })
        setSavedPlan(saved)
      } catch (err) {
        setError(
          err instanceof ApiError
            ? err.message
            : 'Could not save the plan. Please try again.',
        )
        setSubmitting(false)
        return
      }
    } else {
      setSubmitting(true)
    }

    if (imageFile) {
      try {
        await uploadSubscriptionPlanImage(saved.id, imageFile)
      } catch (err) {
        setImageError(
          err instanceof ApiError
            ? err.message
            : 'Could not upload the image. Please try again.',
        )
        setSubmitting(false)
        return
      }
    }

    await onSaved()
    setSubmitting(false)
    onClose()
  }

  const isRetryingImage = savedPlan != null
  const submitLabel = isRetryingImage
    ? 'Retry image upload'
    : isEdit
      ? 'Save changes'
      : 'Create plan'

  return (
    <Modal
      open
      onClose={onClose}
      title={isEdit ? 'Edit plan' : 'Create a new plan'}
      description={
        isEdit
          ? 'Update the details customers see for this subscription.'
          : 'Define a subscription your customers can buy and redeem by QR code.'
      }
      footer={
        <>
          <button type="button" onClick={onClose} className={BTN_SECONDARY}>
            {isRetryingImage ? 'Close' : 'Cancel'}
          </button>
          <button
            type="submit"
            form="plan-form"
            disabled={submitting}
            className={BTN_PRIMARY}
          >
            {submitting && <Spinner className="h-4 w-4" />}
            {submitLabel}
          </button>
        </>
      }
    >
      <form id="plan-form" onSubmit={handleSubmit} className="space-y-4">
        {error && <div className={ERROR_BANNER}>{error}</div>}
        {imageError && (
          <div className={WARNING_BANNER}>
            The plan was saved, but the image didn&rsquo;t upload: {imageError}
          </div>
        )}

        <PlanImagePicker
          previewUrl={displayImageUrl}
          disabled={submitting}
          onPick={pickImage}
        />

        <fieldset disabled={submitting || isRetryingImage} className="space-y-4">
          <div>
            <label htmlFor="plan-name" className={labelClass}>
              Name
            </label>
            <input
              id="plan-name"
              className={fieldClass}
              value={values.name}
              onChange={(e) => set('name', e.target.value)}
              placeholder="Weekly coffee club"
              maxLength={120}
            />
          </div>

          <div>
            <label htmlFor="plan-description" className={labelClass}>
              Description{' '}
              <span className="font-normal text-neutral-500">(optional)</span>
            </label>
            <textarea
              id="plan-description"
              className={`${fieldClass} min-h-20 resize-y`}
              value={values.description}
              onChange={(e) => set('description', e.target.value)}
              placeholder="One drink per day, valid Monday to Friday."
              maxLength={500}
            />
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div>
              <label htmlFor="plan-price" className={labelClass}>
                Price (USD)
              </label>
              <input
                id="plan-price"
                type="number"
                inputMode="decimal"
                min="0"
                step="0.01"
                className={fieldClass}
                value={values.price}
                onChange={(e) => set('price', e.target.value)}
                placeholder="25.00"
              />
            </div>
            <div>
              <label htmlFor="plan-duration" className={labelClass}>
                Duration (days)
              </label>
              <input
                id="plan-duration"
                type="number"
                inputMode="numeric"
                min="1"
                step="1"
                className={fieldClass}
                value={values.durationInDays}
                onChange={(e) => set('durationInDays', e.target.value)}
                placeholder="30"
              />
            </div>
          </div>

          <div>
            <span className={labelClass}>Usage limit</span>
            <label className="mt-2 flex items-center gap-2 text-sm text-neutral-900">
              <input
                type="checkbox"
                className="custom-checkbox"
                checked={values.noUsageLimit}
                onChange={(e) => set('noUsageLimit', e.target.checked)}
              />
              No limit &mdash; redeem as often as the plan allows
            </label>
            {!values.noUsageLimit && (
              <input
                type="number"
                inputMode="numeric"
                min="1"
                step="1"
                aria-label="Number of redemptions"
                className={`${fieldClass} max-w-40`}
                value={values.usageLimit}
                onChange={(e) => set('usageLimit', e.target.value)}
                placeholder="10"
              />
            )}
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div>
              <label htmlFor="plan-active-from" className={labelClass}>
                Active from
              </label>
              <input
                id="plan-active-from"
                type="time"
                className={fieldClass}
                value={values.activeFrom}
                onChange={(e) => set('activeFrom', e.target.value)}
              />
            </div>
            <div>
              <label htmlFor="plan-active-until" className={labelClass}>
                Active until
              </label>
              <input
                id="plan-active-until"
                type="time"
                className={fieldClass}
                value={values.activeUntil}
                onChange={(e) => set('activeUntil', e.target.value)}
              />
            </div>
          </div>
        </fieldset>
      </form>
    </Modal>
  )
}

function PlanImagePicker({
  previewUrl,
  disabled,
  onPick,
}: {
  previewUrl: string | null
  disabled?: boolean
  onPick: (file: File) => void
}) {
  const inputRef = useRef<HTMLInputElement>(null)
  const reduced = usePrefersReducedMotion()

  const handleChange = (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    event.target.value = '' // let the same file be re-picked later
    if (file) onPick(file)
  }

  return (
    <div>
      <span className={labelClass}>
        Image <span className="font-normal text-neutral-500">(optional)</span>
      </span>
      <button
        type="button"
        onClick={() => inputRef.current?.click()}
        disabled={disabled}
        className={`group relative mt-1.5 block aspect-[3/2] w-full overflow-hidden rounded-xl transition focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-500 disabled:cursor-not-allowed disabled:opacity-60 ${
          previewUrl
            ? 'border border-neutral-200'
            : 'border-2 border-dashed border-neutral-300 bg-linear-to-br from-blue-50 via-white to-white hover:border-blue-300'
        }`}
      >
        <AnimatePresence mode="wait" initial={false}>
          {previewUrl ? (
            <motion.img
              key={previewUrl}
              src={previewUrl}
              alt=""
              variants={materializeVariants}
              initial="initial"
              animate="animate"
              exit="exit"
              transition={motionSafe(SPRING_UI, reduced)}
              className="absolute inset-0 h-full w-full object-cover"
            />
          ) : (
            <motion.div
              key="empty"
              variants={materializeVariants}
              initial="initial"
              animate="animate"
              exit="exit"
              transition={motionSafe(SPRING_UI, reduced)}
              className="absolute inset-0 flex flex-col items-center justify-center gap-2 text-neutral-400 group-hover:text-blue-500"
            >
              <ImageIcon className="h-7 w-7" />
              <span className="text-sm font-medium">Click to add an image</span>
            </motion.div>
          )}
        </AnimatePresence>

        {previewUrl && (
          <div className="absolute inset-0 flex items-center justify-center gap-1.5 bg-neutral-900/0 text-white opacity-0 transition group-hover:bg-neutral-900/40 group-hover:opacity-100">
            <CameraIcon className="h-4 w-4" />
            <span className="text-sm font-medium">Change image</span>
          </div>
        )}
      </button>
      <input
        ref={inputRef}
        type="file"
        accept="image/*"
        className="hidden"
        onChange={handleChange}
      />
    </div>
  )
}
