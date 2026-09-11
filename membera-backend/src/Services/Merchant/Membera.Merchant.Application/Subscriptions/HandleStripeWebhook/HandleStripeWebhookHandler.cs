using Membera.Merchant.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Membera.Merchant.Application.Subscriptions.HandleStripeWebhook;

public class HandleStripeWebhookHandler
{
    private readonly IUserSubscriptionRepository _userSubscriptionRepository;
    private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
    private readonly ILogger<HandleStripeWebhookHandler> _logger;

    public HandleStripeWebhookHandler(
        IUserSubscriptionRepository userSubscriptionRepository,
        ISubscriptionPlanRepository subscriptionPlanRepository,
        ILogger<HandleStripeWebhookHandler> logger)
    {
        _userSubscriptionRepository = userSubscriptionRepository;
        _subscriptionPlanRepository = subscriptionPlanRepository;
        _logger = logger;
    }

    public async Task HandleAsync(HandleStripeWebhookCommand command)
    {
        var subscription = await _userSubscriptionRepository.GetByStripeSessionIdAsync(command.StripeSessionId);
        if (subscription is null)
        {
            _logger.LogWarning("Stripe webhook received for an unknown session. SessionId: {SessionId}", command.StripeSessionId);
            throw new InvalidOperationException("Subscription not found for this session.");
        }

        var plan = await _subscriptionPlanRepository.GetByIdAsync(subscription.SubscriptionPlanId);
        if (plan is null)
        {
            _logger.LogWarning(
                "Stripe webhook could not resolve the plan for subscription {SubscriptionId}. PlanId: {PlanId}",
                subscription.Id, subscription.SubscriptionPlanId);
            throw new InvalidOperationException("Subscription plan not found.");
        }

        subscription.Activate(plan.DurationInDays, plan.UsageLimit);
        await _userSubscriptionRepository.UpdateAsync(subscription);

        _logger.LogInformation(
            "Subscription activated via Stripe webhook. UserId: {UserId}, RedemptionCode: {RedemptionCode}",
            subscription.UserId, subscription.RedemptionCode);
    }
}
