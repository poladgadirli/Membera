namespace Membera.Merchant.Application.SubscriptionPlans.BrowseActivePlans;

/// <summary>
/// One active plan plus the basic info about the merchant that owns it. Field
/// names match the frontend's BrowsePlan interface (src/lib/subscriptions.ts)
/// after camelCase JSON serialization.
/// </summary>
public record BrowsePlanSummary(
    Guid Id,
    Guid MerchantId,
    string MerchantBusinessName,
    string? MerchantLogoUrl,
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

public record BrowseActivePlansResult(List<BrowsePlanSummary> Plans, int TotalCount);
