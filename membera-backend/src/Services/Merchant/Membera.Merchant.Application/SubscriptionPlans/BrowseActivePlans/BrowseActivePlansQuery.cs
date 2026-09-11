namespace Membera.Merchant.Application.SubscriptionPlans.BrowseActivePlans;

/// <summary>Lists active plans across all merchants, one page at a time.</summary>
public record BrowseActivePlansQuery(int Page = 1, int PageSize = 9);
