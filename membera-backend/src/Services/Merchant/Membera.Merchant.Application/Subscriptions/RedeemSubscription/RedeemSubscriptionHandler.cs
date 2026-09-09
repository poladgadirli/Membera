using Membera.Merchant.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Membera.Merchant.Application.Subscriptions.RedeemSubscription;

public class RedeemSubscriptionHandler
{
    private readonly IUserSubscriptionRepository _userSubscriptionRepository;
    private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
    private readonly ILogger<RedeemSubscriptionHandler> _logger;

    public RedeemSubscriptionHandler(
        IUserSubscriptionRepository userSubscriptionRepository,
        ISubscriptionPlanRepository subscriptionPlanRepository,
        ILogger<RedeemSubscriptionHandler> logger)
    {
        _userSubscriptionRepository = userSubscriptionRepository;
        _subscriptionPlanRepository = subscriptionPlanRepository;
        _logger = logger;
    }

    public async Task<RedeemSubscriptionResult> HandleAsync(RedeemSubscriptionCommand command)
    {
        var subscription = await _userSubscriptionRepository.GetByRedemptionCodeAsync(command.RedemptionCode);
        if (subscription is null)
        {
            _logger.LogWarning(
                "Redemption attempt with an unknown code by MerchantId: {MerchantId}", command.MerchantId);
            throw new InvalidOperationException("Invalid redemption code.");
        }

        var plan = await _subscriptionPlanRepository.GetByIdAsync(subscription.SubscriptionPlanId);
        if (plan is null)
        {
            _logger.LogWarning(
                "Redemption blocked: plan {PlanId} for subscription {SubscriptionId} no longer exists.",
                subscription.SubscriptionPlanId, subscription.Id);
            throw new InvalidOperationException("Invalid redemption code.");
        }

        if (plan.MerchantId != command.MerchantId)
        {
            _logger.LogWarning(
                "Merchant {MerchantId} attempted to redeem a code belonging to merchant {OwnerMerchantId}. SubscriptionId: {SubscriptionId}",
                command.MerchantId, plan.MerchantId, subscription.Id);
            throw new InvalidOperationException("This code does not belong to your business.");
        }

        // The plan is only redeemable during its daily active window. ActiveUntil
        // earlier than ActiveFrom means the window wraps past midnight.
        var now = TimeOnly.FromDateTime(DateTime.Now);
        var withinWindow = plan.ActiveFrom <= plan.ActiveUntil
            ? now >= plan.ActiveFrom && now <= plan.ActiveUntil
            : now >= plan.ActiveFrom || now <= plan.ActiveUntil;

        if (!withinWindow)
        {
            _logger.LogWarning(
                "Redemption blocked: outside the plan's active hours ({From}-{Until}). SubscriptionId: {SubscriptionId}",
                plan.ActiveFrom, plan.ActiveUntil, subscription.Id);
            throw new InvalidOperationException("This plan is not active at this hour.");
        }

        subscription.Redeem();
        await _userSubscriptionRepository.UpdateAsync(subscription);

        _logger.LogInformation(
            "Subscription redeemed. SubscriptionId: {SubscriptionId}, UsagesRemaining: {UsagesRemaining}",
            subscription.Id, subscription.UsagesRemaining);

        return new RedeemSubscriptionResult(subscription.Id, subscription.UsagesRemaining, plan.Name);
    }
}
