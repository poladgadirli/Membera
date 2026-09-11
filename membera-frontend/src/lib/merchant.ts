// Merchant service calls (plans, merchant profile). Thin wrappers over the
// shared api client (src/lib/apiClient.ts), which attaches the bearer token,
// unwraps BaseResponse<T> and throws ApiError.

import { ApiError, api } from '@/lib/apiClient'

/** Matches the backend BusinessCategory enum (Membera.Merchant.Domain.Enums). */
export type BusinessCategory =
  | 'Restaurant'
  | 'Cafe'
  | 'Barbershop'
  | 'BeautySalon'
  | 'Gym'
  | 'Other'

export const BUSINESS_CATEGORIES: BusinessCategory[] = [
  'Restaurant',
  'Cafe',
  'Barbershop',
  'BeautySalon',
  'Gym',
  'Other',
]

export const BUSINESS_CATEGORY_LABELS: Record<BusinessCategory, string> = {
  Restaurant: 'Restaurant',
  Cafe: 'Café',
  Barbershop: 'Barbershop',
  BeautySalon: 'Beauty Salon',
  Gym: 'Gym',
  Other: 'Other',
}

export interface MerchantProfile {
  businessName: string
  description: string | null
  logoUrl: string | null
  businessCategory: BusinessCategory
  isActive: boolean
}

export interface SubscriptionPlan {
  id: string
  name: string
  description: string | null
  price: number
  durationInDays: number
  usageLimit: number | null
  /** "HH:mm:ss" */
  activeFrom: string | null
  /** "HH:mm:ss" */
  activeUntil: string | null
  imageUrl: string | null
  isActive: boolean
}

export interface PlanInput {
  name: string
  description: string
  price: number
  durationInDays: number
  usageLimit: number | null
  /** "HH:mm:ss" */
  activeFrom: string
  /** "HH:mm:ss" */
  activeUntil: string
}

// --- Merchant profile ---

export function getMyMerchant(): Promise<MerchantProfile> {
  return api.get<MerchantProfile>('/Merchant/me')
}

export function createMerchant(
  businessName: string,
  businessCategory: BusinessCategory,
): Promise<MerchantProfile> {
  return api.post<MerchantProfile>('/Merchant', {
    businessName,
    category: businessCategory,
  })
}

export function updateMerchant(input: {
  businessName: string
  description: string
  businessCategory: BusinessCategory
}): Promise<MerchantProfile> {
  return api.put<MerchantProfile>('/Merchant', {
    businessName: input.businessName,
    description: input.description,
    category: input.businessCategory,
  })
}

/**
 * Uploads a new merchant logo (multipart/form-data) and returns its public URL.
 * The API stores the URL on the merchant, so callers should refetch the profile
 * afterwards to pick up the change.
 */
export function uploadMerchantLogo(file: File): Promise<string> {
  const form = new FormData()
  form.append('file', file)
  return api.postForm<string>('/merchant/logo', form)
}

/** The API answers a missing profile with a 400 + "Merchant profile not found." */
export function isMerchantNotFound(error: unknown): boolean {
  return (
    error instanceof ApiError &&
    /merchant profile not found/i.test(error.message)
  )
}

// --- Subscription plans ---

export async function getMyPlans(): Promise<SubscriptionPlan[]> {
  // GET /subscription-plans/mine answers with BaseResponse<GetPlansByMerchantIdResult>.
  // The shared client strips the BaseResponse envelope, which leaves the inner
  // result object `{ plans: [...] }` — NOT a bare array. Reading it as an array
  // (and silently falling back to `[]`) is why a freshly created plan never
  // showed up until a full reload. Accept both shapes so we're robust to either.
  const data = await api.get<SubscriptionPlan[] | { plans?: SubscriptionPlan[] }>(
    '/subscription-plans/mine',
  )
  if (Array.isArray(data)) return data
  if (data && Array.isArray(data.plans)) return data.plans
  return []
}

export function createPlan(input: PlanInput): Promise<SubscriptionPlan> {
  return api.post<SubscriptionPlan>('/subscription-plans', input)
}

export function updatePlan(
  planId: string,
  input: PlanInput,
): Promise<SubscriptionPlan> {
  return api.put<SubscriptionPlan>(
    `/subscription-plans/${planId}`,
    input,
  )
}

export function deactivatePlan(planId: string): Promise<void> {
  return api.post<void>(`/subscription-plans/${planId}/deactivate`)
}

/**
 * Uploads an image for a single subscription plan (multipart/form-data) and
 * returns its public URL. Callers should refetch the plans list afterwards.
 */
export function uploadSubscriptionPlanImage(
  planId: string,
  file: File,
): Promise<string> {
  const form = new FormData()
  form.append('file', file)
  return api.postForm<string>(`/subscription-plans/${planId}/image`, form)
}

// --- Formatting helpers ---

/** "09:00" (from <input type="time">) -> "09:00:00" for the API. */
export function toApiTime(value: string): string {
  if (!value) return ''
  const parts = value.split(':')
  while (parts.length < 3) parts.push('00')
  return parts
    .slice(0, 3)
    .map((part) => part.padStart(2, '0'))
    .join(':')
}

/** "09:00:00" (from the API) -> "09:00" for <input type="time">. */
export function toInputTime(value: string | null | undefined): string {
  if (!value) return ''
  const [hours = '00', minutes = '00'] = value.split(':')
  return `${hours.padStart(2, '0')}:${minutes.padStart(2, '0')}`
}

export function formatTimeRange(
  from: string | null | undefined,
  until: string | null | undefined,
): string {
  const start = toInputTime(from)
  const end = toInputTime(until)
  if (!start && !end) return 'Any time'
  return `${start || '00:00'} – ${end || '00:00'}`
}

export function formatPrice(price: number): string {
  return `$${price.toFixed(2)}`
}

export function formatDuration(days: number): string {
  return days === 1 ? '1 day' : `${days} days`
}

export function formatUsageLimit(limit: number | null): string {
  if (limit == null || limit <= 0) return 'Unlimited redemptions'
  return limit === 1 ? '1 redemption' : `${limit} redemptions`
}
