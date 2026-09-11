// Shared Tailwind class presets for the whole app, matching the landing page's
// visual language: the #f7f9fc canvas with a soft blue wash, frosted white cards
// (border-white bg-white/70 backdrop-blur-xl), blue gradient accents, neutral
// typography (text-neutral-900 / text-neutral-500), and pill eyebrows.
// The marketing pages are light-only, so the whole app commits to light too.

export const PAGE_BG = 'bg-[#f7f9fc]'

/** The soft blue gradient that washes down from the top of every page. */
export const PAGE_WASH =
  'pointer-events-none absolute inset-x-0 top-0 h-80 bg-linear-to-b from-blue-100/70 via-blue-50/40 to-transparent'

/** Frosted glass card — the default surface for content on the #f7f9fc canvas. */
export const CARD =
  'rounded-2xl border border-white bg-white/70 shadow-sm shadow-blue-500/5 backdrop-blur-xl'

/** Opaque white card — for surfaces that sit on top of other cards or modals. */
export const CARD_SOLID =
  'rounded-2xl border border-neutral-200/70 bg-white shadow-sm'

// Tracking is size-specific: Tailwind's flat `tracking-tight` (-0.025em) is
// fine at 4xl, but reads slightly loose once the same heading grows to 5xl —
// so the largest reusable heading gets an explicit, tighter step at that
// breakpoint instead of inheriting one flat value at every size.
export const HEADING_XL =
  'text-4xl font-medium tracking-tight text-neutral-900 sm:text-5xl sm:tracking-[-0.03em]'
export const HEADING_LG = 'text-2xl font-medium tracking-tight text-neutral-900'
export const HEADING_MD = 'text-lg font-semibold text-neutral-900'

export const TEXT_MUTED = 'text-neutral-500'

/** Small uppercase-ish section label used above card groups. */
export const SECTION_LABEL = 'text-sm font-medium text-neutral-500'

export const EYEBROW =
  'inline-flex w-fit items-center gap-2 rounded-full border-2 border-white bg-white px-1.5 py-1 text-black shadow-lg shadow-blue-500/20'
export const EYEBROW_TAG =
  'rounded-full bg-linear-to-br from-blue-500 to-blue-200 px-2 py-0.5 text-xs font-medium uppercase tracking-widest text-white'

/** The gradient "M" brand mark. Append a size (h-8 w-8, text-sm) at the call site. */
export const LOGO_MARK =
  'grid shrink-0 place-items-center rounded-lg bg-linear-to-br from-blue-500 via-blue-400 to-blue-200 font-bold text-white shadow-sm shadow-blue-500/30'

export const ICON_BADGE =
  'grid shrink-0 place-items-center rounded-xl bg-linear-to-br from-blue-500 via-blue-400 to-blue-200 text-white shadow-sm shadow-blue-500/30'

// active: fires on pointer-down (not click/release), so this is the "respond
// instantly to a press" feedback the whole app's buttons get for free.
const BUTTON_BASE =
  'inline-flex items-center justify-center gap-2 rounded-lg text-sm font-semibold transition duration-150 ease-out active:scale-[0.97] focus-visible:outline-2 focus-visible:outline-offset-2 disabled:cursor-not-allowed disabled:opacity-60 disabled:active:scale-100'

export const BTN_PRIMARY = `${BUTTON_BASE} border border-blue-300 bg-linear-to-br from-blue-500 via-blue-400 to-blue-200 px-4 py-2.5 text-white shadow-sm shadow-blue-500/30 hover:brightness-105 focus-visible:outline-blue-500`

export const BTN_SECONDARY = `${BUTTON_BASE} border border-neutral-200 bg-white px-4 py-2.5 text-neutral-900 shadow-sm hover:bg-neutral-50 focus-visible:outline-blue-500`

export const BTN_SECONDARY_SM = `${BUTTON_BASE} border border-neutral-200 bg-white px-3 py-1.5 text-neutral-900 shadow-sm hover:bg-neutral-50 focus-visible:outline-blue-500`

export const BTN_DARK = `${BUTTON_BASE} border border-neutral-800 bg-neutral-900 px-4 py-2.5 text-white shadow-sm hover:brightness-110 focus-visible:outline-blue-500`

export const BTN_DESTRUCTIVE = `${BUTTON_BASE} border border-red-200 bg-white px-4 py-2.5 text-red-600 shadow-sm hover:bg-red-50 focus-visible:outline-red-500`

export const INPUT =
  'mt-1 w-full rounded-lg border border-neutral-200 bg-white px-3 py-2.5 text-sm text-neutral-900 shadow-sm placeholder:text-neutral-400 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-500'

export const LABEL = 'block text-sm font-medium text-neutral-900'

export const ERROR_BANNER =
  'rounded-lg border border-red-200 bg-red-50 px-3 py-2.5 text-sm text-red-700'

/** Partial-success notice — e.g. "the rest saved, but this one part failed." */
export const WARNING_BANNER =
  'rounded-lg border border-amber-200 bg-amber-50 px-3 py-2.5 text-sm text-amber-800'

/** Active / inactive status pills. */
export const BADGE_ACTIVE =
  'inline-flex items-center gap-1.5 rounded-full border border-blue-200 bg-blue-50 px-2.5 py-0.5 text-xs font-medium text-blue-700'
export const BADGE_INACTIVE =
  'inline-flex items-center gap-1.5 rounded-full border border-neutral-200 bg-neutral-100 px-2.5 py-0.5 text-xs font-medium text-neutral-500'

/** Plain label pill — same shape as the active/inactive badges, no status dot.
 * Used for category tags and other non-status metadata. */
export const BADGE_NEUTRAL =
  'inline-flex items-center rounded-full border border-neutral-200 bg-neutral-100 px-2.5 py-0.5 text-xs font-medium text-neutral-600'

/** Modal backdrop + panel. */
export const MODAL_BACKDROP =
  'fixed inset-0 z-50 flex items-end justify-center overflow-y-auto bg-neutral-900/40 p-4 backdrop-blur-sm sm:items-center'
export const MODAL_PANEL =
  'my-8 w-full max-w-lg rounded-2xl border border-white bg-white p-6 shadow-xl shadow-blue-500/10'
