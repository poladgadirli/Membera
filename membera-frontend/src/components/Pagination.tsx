import { BTN_SECONDARY_SM, TEXT_MUTED } from '@/lib/ui'

interface PaginationProps {
  /** 1-indexed current page. */
  page: number
  pageCount: number
  onPageChange: (page: number) => void
  className?: string
}

/**
 * Simple Previous/Next pagination bar. Renders nothing when everything fits
 * on one page. Purely presentational — `onPageChange` is called with the new
 * page number, and it's up to the caller to fetch that page (server-side) or
 * re-slice an in-memory array (client-side).
 */
export function Pagination({ page, pageCount, onPageChange, className }: PaginationProps) {
  if (pageCount <= 1) return null

  return (
    <div className={`flex items-center justify-between gap-3 ${className ?? ''}`}>
      <p className={`text-xs ${TEXT_MUTED}`}>
        Page {page} of {pageCount}
      </p>
      <div className="flex items-center gap-2">
        <button
          type="button"
          onClick={() => onPageChange(page - 1)}
          disabled={page <= 1}
          className={BTN_SECONDARY_SM}
        >
          Previous
        </button>
        <button
          type="button"
          onClick={() => onPageChange(page + 1)}
          disabled={page >= pageCount}
          className={BTN_SECONDARY_SM}
        >
          Next
        </button>
      </div>
    </div>
  )
}
