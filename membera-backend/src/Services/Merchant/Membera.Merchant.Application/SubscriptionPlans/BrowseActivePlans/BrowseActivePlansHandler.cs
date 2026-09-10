using Membera.Merchant.Application.Abstractions;
using Microsoft.Extensions.Logging;
using MerchantEntity = Membera.Merchant.Domain.Entities.Merchant;

namespace Membera.Merchant.Application.SubscriptionPlans.BrowseActivePlans;

public class BrowseActivePlansHandler
{
    private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
    private readonly IMerchantRepository _merchantRepository;
    private readonly ILogger<BrowseActivePlansHandler> _logger;

    public BrowseActivePlansHandler(
        ISubscriptionPlanRepository subscriptionPlanRepository,
        IMerchantRepository merchantRepository,
        ILogger<BrowseActivePlansHandler> logger)
    {
        _subscriptionPlanRepository = subscriptionPlanRepository;
        _merchantRepository = merchantRepository;
        _logger = logger;
    }

    public async Task<BrowseActivePlansResult> HandleAsync(BrowseActivePlansQuery query)
    {
        var plans = await _subscriptionPlanRepository.GetAllActiveAsync();

        // One merchant lookup per distinct merchant, cached so several plans from
        // the same business don't each hit the repository.
        var merchantCache = new Dictionary<Guid, MerchantEntity?>();

        var summaries = new List<BrowsePlanSummary>();
        foreach (var plan in plans.Where(p => p.IsActive))
        {
            if (!merchantCache.TryGetValue(plan.MerchantId, out var merchant))
            {
                merchant = await _merchantRepository.GetByIdAsync(plan.MerchantId);
                merchantCache[plan.MerchantId] = merchant;
            }

            summaries.Add(new BrowsePlanSummary(
                plan.Id,
                plan.MerchantId,
                merchant?.BusinessName ?? string.Empty,
                merchant?.LogoUrl,
                plan.Name,
                plan.Description,
                plan.Price,
                plan.DurationInDays,
                plan.UsageLimit,
                plan.ActiveFrom,
                plan.ActiveUntil,
                plan.ImageUrl,
                plan.IsActive));
        }

        _logger.LogInformation(
            "Retrieved {Count} active subscription plans across all merchants", summaries.Count);

        return new BrowseActivePlansResult(summaries);
    }
}
