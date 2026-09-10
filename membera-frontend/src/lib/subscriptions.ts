// Subscription + checkout + redemption calls. Thin wrappers over the shared api
// client (src/lib/apiClient.ts), which attaches the bearer token, unwraps
// BaseResponse<T> and throws ApiError.
//
// Backend routes (through the gateway, prefix /api):
//   GET  /subscription-plans       -> { plans: [...] }   (all active, all merchants)
//   POST /subscriptions/checkout   { subscriptionPlanId } -> { checkoutUrl }
//   GET  /subscriptions/mine       -> { subscriptions: [...] }
//   POST /subscriptions/redeem     { redemptionCode }      -> { subscriptionId, usagesRemaining, planName }

import { api } from '@/lib/apiClient'

export { ApiError } from '@/lib/apiClient'

// --- My subscriptions (GET /subscriptions/mine) ---

/** Matches the backend SubscriptionStatus enum (Membera.Merchant.Domain.Enums). */
export type SubscriptionStatus =
  | 'Pending'
  | 'Active'
  | 'Expired'
  | 'Cancelled'

export interface UserSubscription {
  id: string
  subscriptionPlanId: string
  planName: string
  /** Human-readable code the user reads out at the merchant's counter, e.g. "MBR-4X7K9P". */
  redemptionCode: string
  /** ISO timestamp. Zero-value ("0001-01-01T00:00:00") until the plan is activated by the webhook. */
  startedAt: string
  /** ISO timestamp. */
  expiresAt: string
  /** null = unlimited redemptions. */
  usagesRemaining: number | null
  status: SubscriptionStatus | string
  /** Stripe Checkout session id — lets the success page match ?session_id=. Null on legacy rows. */
  stripeSessionId: string | null
}

export async function getMySubscriptions(): Promise<UserSubscription[]> {
  // BaseResponse<GetMySubscriptionsResult> -> client strips the envelope ->
  // { subscriptions: [...] } (NOT a bare array). Accept both shapes defensively,
  // same as getMyPlans in src/lib/merchant.ts.
  const data = await api.get<
    UserSubscription[] | { subscriptions?: UserSubscription[] }
  >('/subscriptions/mine')
  if (Array.isArray(data)) return data
  if (data && Array.isArray(data.subscriptions)) return data.subscriptions
  return []
}

// --- Stripe checkout (POST /subscriptions/checkout) ---

export interface CheckoutResult {
  checkoutUrl: string
}

/**
 * Creates a Stripe Checkout session for a plan and returns the hosted checkout
 * URL. The caller redirects the browser to it (window.location.href = ...).
 *
 * The backend builds the Stripe success/cancel URLs from FRONTEND_BASE_URL and
 * points them at /subscription-success?session_id={CHECKOUT_SESSION_ID} and
 * /subscription-cancel, so the post-payment redirect lands back in the app.
 */
export function checkoutSubscription(
  subscriptionPlanId: string,
): Promise<CheckoutResult> {
  return api.post<CheckoutResult>('/subscriptions/checkout', {
    subscriptionPlanId,
  })
}

// --- Redemption (POST /subscriptions/redeem) — MerchantOwner only ---

export interface RedeemResult {
  subscriptionId: string
  usagesRemaining: number | null
  planName: string
}

/**
 * Redeems one usage of a subscription by its redemption code. Requires the
 * caller to have a merchant profile (the backend resolves the merchant from the
 * current user and rejects codes that don't belong to it).
 */
export function redeemSubscription(
  redemptionCode: string,
): Promise<RedeemResult> {
  return api.post<RedeemResult>('/subscriptions/redeem', { redemptionCode })
}

// --- Browse ALL active plans across ALL merchants (GET /subscription-plans) ---
// Backed by BrowseActivePlansHandler: every active plan, joined with its owning
// merchant's business name / logo.

export interface BrowsePlan {
  id: string
  name: string
  description: string | null
  price: number
  durationInDays: number
  usageLimit: number | null
  /** "HH:mm:ss" or null */
  activeFrom: string | null
  /** "HH:mm:ss" or null */
  activeUntil: string | null
  imageUrl: string | null
  isActive: boolean
  merchantId: string
  merchantBusinessName: string
  merchantLogoUrl: string | null
}

export async function browseActivePlans(): Promise<BrowsePlan[]> {
  // BaseResponse<BrowseActivePlansResult> -> client strips the envelope ->
  // { plans: [...] }. Accept a bare array too, defensively.
  const data = await api.get<BrowsePlan[] | { plans?: BrowsePlan[] }>(
    '/subscription-plans',
  )
  if (Array.isArray(data)) return data
  return data.plans ?? []
}
