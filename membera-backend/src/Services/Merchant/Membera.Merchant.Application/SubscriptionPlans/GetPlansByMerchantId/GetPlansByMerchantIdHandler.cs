using Membera.Merchant.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Membera.Merchant.Application.SubscriptionPlans.GetPlansByMerchantId;

public class GetPlansByMerchantIdHandler
{
    private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
    private readonly ILogger<GetPlansByMerchantIdHandler> _logger;

    public GetPlansByMerchantIdHandler(
        ISubscriptionPlanRepository subscriptionPlanRepository,
        ILogger<GetPlansByMerchantIdHandler> logger)
    {
        _subscriptionPlanRepository = subscriptionPlanRepository;
        _logger = logger;
    }

    public async Task<GetPlansByMerchantIdResult> HandleAsync(GetPlansByMerchantIdQuery query)
    {
        var plans = await _subscriptionPlanRepository.GetByMerchantIdAsync(query.MerchantId);

        var summaries = plans.Select(p => new SubscriptionPlanSummary(
            p.Id, p.Name, p.Description, p.Price, p.DurationInDays, p.UsageLimit, p.ActiveFrom, p.ActiveUntil, p.ImageUrl, p.IsActive
        )).ToList();

        _logger.LogInformation("Retrieved {Count} subscription plans for MerchantId: {MerchantId}", summaries.Count, query.MerchantId);

        return new GetPlansByMerchantIdResult(summaries);
    }
}