interface SpinnerProps {
  className?: string
  label?: string
}

/** Minimal loading indicator in the landing-page blue. */
export function Spinner({ className = 'h-5 w-5', label = 'Loading' }: SpinnerProps) {
  return (
    <span
      role="status"
      aria-label={label}
      className={`inline-block animate-spin rounded-full border-2 border-blue-200 border-t-blue-500 ${className}`}
    />
  )
}
