namespace Membera.Merchant.Application.SubscriptionPlans.GetPlansByMerchantId;

public record SubscriptionPlanSummary(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int DurationInDays,
    int? UsageLimit,
    TimeOnly ActiveFrom,
    TimeOnly ActiveUntil,
    string? ImageUrl,
    bool IsActive
);

public record GetPlansByMerchantIdResult(List<SubscriptionPlanSummary> Plans);