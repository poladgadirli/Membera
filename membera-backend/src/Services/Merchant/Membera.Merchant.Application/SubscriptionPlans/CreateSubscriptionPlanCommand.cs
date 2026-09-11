namespace Membera.Merchant.Application.SubscriptionPlans.CreateSubscriptionPlan;

public record CreateSubscriptionPlanCommand(
    Guid MerchantId,
    string Name,
    string? Description,
    decimal Price,
    int DurationInDays,
    int? UsageLimit,
    TimeOnly ActiveFrom,
    TimeOnly ActiveUntil
);