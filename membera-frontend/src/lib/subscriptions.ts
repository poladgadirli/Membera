// Subscription + checkout + redemption calls. Thin wrappers over the shared api
// client (src/lib/apiClient.ts), which attaches the bearer token, unwraps
// BaseResponse<T> and throws ApiError.
//
// Backend routes (through the gateway, prefix /api):
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
 * NOTE: the backend currently hardcodes the Stripe success/cancel URLs to
 * `https://localhost:7241/api/subscriptions/success|cancel` (placeholder API
 * routes that don't exist). Until the backend is changed to point at the
 * frontend routes /subscription-success and /subscription-cancel (or to accept
 * them from this request body), the post-payment redirect will not land the user
 * back in the app. See the report / SubscriptionController.Checkout.
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

// --- Browse ALL active plans across ALL merchants ---
// ⚠️ BACKEND GAP: there is no endpoint that lists active plans across merchants,
// and no endpoint that joins the merchant business name onto a plan. The
// SubscriptionPlanController only exposes GET /subscription-plans/mine (the
// current merchant's own plans). Everything below is a PLACEHOLDER shaped like
// what such an endpoint SHOULD return, so BrowsePlansPage is ready to wire up.
//
// Expected real endpoint (to be added by the backend team), e.g.:
//   GET /subscription-plans            (public or [Authorize], paged/filterable)
//   -> { plans: BrowsePlan[] }

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
  /** The owning merchant — NOT currently returned by any endpoint. */
  merchantId: string
  merchantBusinessName: string
  merchantLogoUrl: string | null
}

/** Flip to false and delete the mock branch once the real endpoint exists. */
export const BROWSE_PLANS_USES_PLACEHOLDER = true

const PLACEHOLDER_BROWSE_PLANS: BrowsePlan[] = [
  {
    id: '00000000-0000-0000-0000-000000000001',
    name: 'Daily Espresso Club',
    description:
      'One specialty espresso or filter coffee every day, redeemed with a quick scan at the counter.',
    price: 29.0,
    durationInDays: 30,
    usageLimit: 30,
    activeFrom: '07:00:00',
    activeUntil: '18:00:00',
    imageUrl:
      'https://images.unsplash.com/photo-1511920170033-f8396924c348?auto=format&fit=crop&w=1200&q=80',
    isActive: true,
    merchantId: '00000000-0000-0000-0000-0000000000a1',
    merchantBusinessName: 'Blue Bottle Coffee',
    merchantLogoUrl: null,
  },
  {
    id: '00000000-0000-0000-0000-000000000002',
    name: 'Unlimited Yoga — Monthly',
    description:
      'Attend any scheduled class, as often as you like, for a full month. Mat included.',
    price: 89.0,
    durationInDays: 30,
    usageLimit: null,
    activeFrom: null,
    activeUntil: null,
    imageUrl:
      'https://images.unsplash.com/photo-1544367567-0f2fcb009e0b?auto=format&fit=crop&w=1200&q=80',
    isActive: true,
    merchantId: '00000000-0000-0000-0000-0000000000a2',
    merchantBusinessName: 'Still Point Studio',
    merchantLogoUrl: null,
  },
  {
    id: '00000000-0000-0000-0000-000000000003',
    name: 'Lunch Pass — 10 Visits',
    description:
      'Ten weekday lunches from the seasonal set menu. Valid for three months from purchase.',
    price: 120.0,
    durationInDays: 90,
    usageLimit: 10,
    activeFrom: '11:30:00',
    activeUntil: '15:00:00',
    imageUrl:
      'https://images.unsplash.com/photo-1546069901-ba9599a7e63c?auto=format&fit=crop&w=1200&q=80',
    isActive: true,
    merchantId: '00000000-0000-0000-0000-0000000000a3',
    merchantBusinessName: 'Greenhouse Kitchen',
    merchantLogoUrl: null,
  },
]

/**
 * PLACEHOLDER. Replace the body with a real call once the backend endpoint
 * exists, e.g.:
 *   const data = await api.get<{ plans?: BrowsePlan[] }>('/subscription-plans')
 *   return Array.isArray(data) ? data : (data.plans ?? [])
 */
export async function browseActivePlans(): Promise<BrowsePlan[]> {
  // Simulate a network round-trip so loading states are exercised.
  await new Promise((resolve) => setTimeout(resolve, 400))
  return PLACEHOLDER_BROWSE_PLANS.filter((plan) => plan.isActive)
}
