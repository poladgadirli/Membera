import { useState, type FormEvent } from 'react'
import { DashboardShell } from '@/components/DashboardShell'
import { PageHeading } from '@/components/PageHeading'
import { Spinner } from '@/components/Spinner'
import { ApiError } from '@/lib/apiClient'
import { changeEmail, changePassword } from '@/lib/account'
import {
  BTN_PRIMARY,
  CARD,
  ERROR_BANNER,
  INFO_BANNER,
  INPUT,
  LABEL,
  SECTION_LABEL,
} from '@/lib/ui'

function ErrorNote({ message }: { message: string }) {
  return <div className={ERROR_BANNER}>{message}</div>
}

function SuccessNote({ message }: { message: string }) {
  return (
    <div className={INFO_BANNER} role="status">
      {message}
    </div>
  )
}

function ChangePasswordCard() {
  const [currentPassword, setCurrentPassword] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [confirmNewPassword, setConfirmNewPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)
  const [saving, setSaving] = useState(false)

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setError(null)
    setSuccess(false)

    if (!currentPassword || !newPassword) {
      setError('Fill in every field.')
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

    setSaving(true)
    try {
      await changePassword({ currentPassword, newPassword })
      setSuccess(true)
      setCurrentPassword('')
      setNewPassword('')
      setConfirmNewPassword('')
    } catch (err) {
      setError(
        err instanceof ApiError
          ? err.message
          : 'Could not change your password. Please try again.',
      )
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className={`${CARD} p-6`}>
      <form onSubmit={handleSubmit} className="space-y-4">
        <h3 className="text-base font-semibold text-neutral-900">
          Change password
        </h3>

        {error && <ErrorNote message={error} />}
        {success && (
          <SuccessNote message="Your password has been changed." />
        )}

        <div>
          <label htmlFor="current-password" className={LABEL}>
            Current password
          </label>
          <input
            id="current-password"
            type="password"
            autoComplete="current-password"
            className={INPUT}
            value={currentPassword}
            onChange={(e) => setCurrentPassword(e.target.value)}
          />
        </div>

        <div>
          <label htmlFor="new-password" className={LABEL}>
            New password
          </label>
          <input
            id="new-password"
            type="password"
            autoComplete="new-password"
            className={INPUT}
            value={newPassword}
            onChange={(e) => setNewPassword(e.target.value)}
          />
        </div>

        <div>
          <label htmlFor="confirm-new-password" className={LABEL}>
            Confirm new password
          </label>
          <input
            id="confirm-new-password"
            type="password"
            autoComplete="new-password"
            className={INPUT}
            value={confirmNewPassword}
            onChange={(e) => setConfirmNewPassword(e.target.value)}
          />
        </div>

        <button type="submit" disabled={saving} className={BTN_PRIMARY}>
          {saving && <Spinner className="h-4 w-4" />}
          {saving ? 'Saving…' : 'Change password'}
        </button>
      </form>
    </div>
  )
}

function ChangeEmailCard() {
  const [newEmail, setNewEmail] = useState('')
  const [currentPassword, setCurrentPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)
  const [saving, setSaving] = useState(false)

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setError(null)
    setSuccess(false)

    if (!newEmail.trim() || !currentPassword) {
      setError('Fill in every field.')
      return
    }

    setSaving(true)
    try {
      await changeEmail({ newEmail: newEmail.trim(), currentPassword })
      setSuccess(true)
      setNewEmail('')
      setCurrentPassword('')
    } catch (err) {
      setError(
        err instanceof ApiError
          ? err.message
          : 'Could not change your email. Please try again.',
      )
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className={`${CARD} p-6`}>
      <form onSubmit={handleSubmit} className="space-y-4">
        <h3 className="text-base font-semibold text-neutral-900">
          Change email
        </h3>

        {error && <ErrorNote message={error} />}
        {success && (
          <SuccessNote message="Your email has been changed. Sign out and back in to see it reflected everywhere in the app." />
        )}

        <div>
          <label htmlFor="new-email" className={LABEL}>
            New email address
          </label>
          <input
            id="new-email"
            type="email"
            autoComplete="email"
            className={INPUT}
            value={newEmail}
            onChange={(e) => setNewEmail(e.target.value)}
          />
        </div>

        <div>
          <label htmlFor="email-current-password" className={LABEL}>
            Current password
          </label>
          <input
            id="email-current-password"
            type="password"
            autoComplete="current-password"
            className={INPUT}
            value={currentPassword}
            onChange={(e) => setCurrentPassword(e.target.value)}
          />
        </div>

        <button type="submit" disabled={saving} className={BTN_PRIMARY}>
          {saving && <Spinner className="h-4 w-4" />}
          {saving ? 'Saving…' : 'Change email'}
        </button>
      </form>
    </div>
  )
}

export default function AccountSettingsPage() {
  return (
    <DashboardShell>
      <PageHeading
        eyebrow="Account settings"
        title="Your account"
        description="Update the password or email you sign in with."
      />

      <div className="mt-10 space-y-8">
        <section aria-labelledby="account-password-heading">
          <h2 id="account-password-heading" className={SECTION_LABEL}>
            Password
          </h2>
          <div className="mt-3">
            <ChangePasswordCard />
          </div>
        </section>

        <section aria-labelledby="account-email-heading">
          <h2 id="account-email-heading" className={SECTION_LABEL}>
            Email
          </h2>
          <div className="mt-3">
            <ChangeEmailCard />
          </div>
        </section>
      </div>
    </DashboardShell>
  )
}
