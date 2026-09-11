using Membera.Merchant.Domain.Enums;

namespace Membera.Merchant.Application.SubscriptionPlans.BrowseActivePlans;

/// <summary>Lists active plans across all merchants, one page at a time,
/// optionally filtered to merchants of a given business category.</summary>
public record BrowseActivePlansQuery(int Page = 1, int PageSize = 9, BusinessCategory? Category = null);
