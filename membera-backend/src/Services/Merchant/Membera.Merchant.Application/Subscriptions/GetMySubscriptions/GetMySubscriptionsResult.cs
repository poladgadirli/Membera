namespace Membera.Merchant.Application.Subscriptions.GetMySubscriptions;

public record UserSubscriptionSummary(
    Guid Id,
    Guid SubscriptionPlanId,
    string PlanName,
    string RedemptionCode,
    DateTime StartedAt,
    DateTime ExpiresAt,
    int? UsagesRemaining,
    string Status
);

public record GetMySubscriptionsResult(List<UserSubscriptionSummary> Subscriptions);
