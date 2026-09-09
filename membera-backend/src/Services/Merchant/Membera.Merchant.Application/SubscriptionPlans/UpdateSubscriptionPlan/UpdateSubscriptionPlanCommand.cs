namespace Membera.Merchant.Application.SubscriptionPlans.UpdateSubscriptionPlan;

public record UpdateSubscriptionPlanCommand(
    Guid PlanId,
    Guid MerchantId,
    string Name,
    string? Description,
    decimal Price,
    int DurationInDays,
    int? UsageLimit,
    TimeOnly ActiveFrom,
    TimeOnly ActiveUntil
);