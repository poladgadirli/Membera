using Membera.Merchant.Application.Abstractions;
using Membera.Merchant.Domain.Enums;
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
        var (plans, totalCount) = await _subscriptionPlanRepository.GetPagedActiveAsync(query.Page, query.PageSize, query.Category);

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
                merchant?.BusinessCategory ?? BusinessCategory.Other,
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
            "Retrieved page {Page} (size {PageSize}) of active subscription plans. Count: {Count}, TotalCount: {TotalCount}",
            query.Page, query.PageSize, summaries.Count, totalCount);

        return new BrowseActivePlansResult(summaries, totalCount);
    }
}
