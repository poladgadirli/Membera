namespace Membera.Merchant.Application.Subscriptions.RedeemSubscription;

public record RedeemSubscriptionResult(
    Guid SubscriptionId,
    int? UsagesRemaining,
    string PlanName
);
