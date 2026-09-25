import logoMark from '@/assets/logo-mark.png'
import { cn } from '@/lib/utils'

/** The Membera brand mark. Decorative: always sits next to the "Membera" wordmark. Pass a size (h-8 w-8). */
export function LogoMark({ className }: { className?: string }) {
  return <img src={logoMark} alt="" aria-hidden="true" className={cn('shrink-0 object-contain', className)} />
}
