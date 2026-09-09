namespace Membera.Merchant.Application.SubscriptionPlans.CreateSubscriptionPlan;

public record CreateSubscriptionPlanResult(
    Guid Id,
    string Name,
    decimal Price
);