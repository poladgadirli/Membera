namespace Membera.Merchant.Application.Subscriptions.GetMySubscriptions;

public record UserSubscriptionSummary(
    Guid Id,
    Guid SubscriptionPlanId,
    string PlanName,
    string RedemptionCode,
    DateTime StartedAt,
    DateTime ExpiresAt,
    int? UsagesRemaining,
    string Status,
    /// <summary>The Stripe Checkout session id. Lets the post-payment success
    /// page correlate the exact subscription via its ?session_id= query param.</summary>
    string? StripeSessionId
);

public record GetMySubscriptionsResult(List<UserSubscriptionSummary> Subscriptions);
