using Membera.Merchant.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Membera.Merchant.Application.Subscriptions.GetMySubscriptions;

public class GetMySubscriptionsHandler
{
    private readonly IUserSubscriptionRepository _userSubscriptionRepository;
    private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
    private readonly ILogger<GetMySubscriptionsHandler> _logger;

    public GetMySubscriptionsHandler(
        IUserSubscriptionRepository userSubscriptionRepository,
        ISubscriptionPlanRepository subscriptionPlanRepository,
        ILogger<GetMySubscriptionsHandler> logger)
    {
        _userSubscriptionRepository = userSubscriptionRepository;
        _subscriptionPlanRepository = subscriptionPlanRepository;
        _logger = logger;
    }

    public async Task<GetMySubscriptionsResult> HandleAsync(GetMySubscriptionsQuery query)
    {
        var subscriptions = await _userSubscriptionRepository.GetByUserIdAsync(query.UserId);

        var summaries = new List<UserSubscriptionSummary>();
        foreach (var subscription in subscriptions)
        {
            // One plan lookup per subscription. Fine at this scale; revisit with a
            // join if a user can ever accumulate a large number of subscriptions.
            var plan = await _subscriptionPlanRepository.GetByIdAsync(subscription.SubscriptionPlanId);

            summaries.Add(new UserSubscriptionSummary(
                subscription.Id,
                subscription.SubscriptionPlanId,
                plan?.Name ?? string.Empty,
                subscription.RedemptionCode,
                subscription.StartedAt,
                subscription.ExpiresAt,
                subscription.UsagesRemaining,
                subscription.Status.ToString(),
                subscription.StripeSessionId));
        }

        _logger.LogInformation(
            "Retrieved {Count} subscriptions for UserId: {UserId}", summaries.Count, query.UserId);

        return new GetMySubscriptionsResult(summaries);
    }
}
