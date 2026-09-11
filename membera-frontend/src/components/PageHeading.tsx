import type { ReactNode } from 'react'
import { EYEBROW, EYEBROW_TAG, HEADING_XL, TEXT_MUTED } from '@/lib/ui'

interface PageHeadingProps {
  eyebrow: string
  title: string
  description?: ReactNode
}

/** Landing-page style page header: pill eyebrow, large heading, muted lede. */
export function PageHeading({ eyebrow, title, description }: PageHeadingProps) {
  return (
    <div className="flex flex-col gap-5">
      <span className={EYEBROW}>
        <span className={EYEBROW_TAG}>Membera</span>
        <span className="text-sm font-medium">{eyebrow}</span>
      </span>
      <h1 className={HEADING_XL}>{title}</h1>
      {description && (
        <p className={`max-w-xl text-lg font-medium ${TEXT_MUTED}`}>
          {description}
        </p>
      )}
    </div>
  )
}
