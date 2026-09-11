namespace Membera.Merchant.Application.Subscriptions.CreateCheckoutSession;

public record CreateCheckoutSessionCommand(
    Guid UserId,
    Guid SubscriptionPlanId,
    string SuccessUrl,
    string CancelUrl
);
