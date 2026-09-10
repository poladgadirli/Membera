import { useState } from 'react'
import { Modal } from '@/components/Modal'
import { Spinner } from '@/components/Spinner'
import { ApiError } from '@/lib/apiClient'
import { BTN_DESTRUCTIVE, BTN_PRIMARY, BTN_SECONDARY, ERROR_BANNER } from '@/lib/ui'

interface ConfirmDialogProps {
  open: boolean
  title: string
  description: string
  confirmLabel?: string
  /** Style the confirm button as a destructive action. */
  destructive?: boolean
  onConfirm: () => Promise<void>
  onClose: () => void
}

export function ConfirmDialog({
  open,
  title,
  description,
  confirmLabel = 'Confirm',
  destructive = false,
  onConfirm,
  onClose,
}: ConfirmDialogProps) {
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleConfirm = async () => {
    setError(null)
    setBusy(true)
    try {
      await onConfirm()
      onClose()
    } catch (err) {
      setError(
        err instanceof ApiError
          ? err.message
          : 'The action could not be completed. Please try again.',
      )
    } finally {
      setBusy(false)
    }
  }

  const confirmClass = destructive ? BTN_DESTRUCTIVE : BTN_PRIMARY

  return (
    <Modal
      open={open}
      onClose={onClose}
      title={title}
      description={description}
      footer={
        <>
          <button
            type="button"
            onClick={onClose}
            disabled={busy}
            className={BTN_SECONDARY}
          >
            Cancel
          </button>
          <button
            type="button"
            onClick={handleConfirm}
            disabled={busy}
            className={confirmClass}
          >
            {busy && <Spinner className="h-4 w-4" />}
            {confirmLabel}
          </button>
        </>
      }
    >
      {error && <div className={ERROR_BANNER}>{error}</div>}
    </Modal>
  )
}
