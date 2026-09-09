namespace Membera.Merchant.Application.Subscriptions.RedeemSubscription;

public record RedeemSubscriptionCommand(
    string RedemptionCode,
    Guid MerchantId
);
