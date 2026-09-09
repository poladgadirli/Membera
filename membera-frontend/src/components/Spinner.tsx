interface SpinnerProps {
  className?: string
  label?: string
}

/** Minimal, token-coloured loading indicator. */
export function Spinner({ className = 'h-5 w-5', label = 'Loading' }: SpinnerProps) {
  return (
    <span
      role="status"
      aria-label={label}
      className={`inline-block animate-spin rounded-full border-2 border-border border-t-foreground ${className}`}
    />
  )
}
