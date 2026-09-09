using Membera.Merchant.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Membera.Merchant.Application.SubscriptionPlans.DeactivateSubscriptionPlan;

public class DeactivateSubscriptionPlanHandler
{
    private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
    private readonly ILogger<DeactivateSubscriptionPlanHandler> _logger;

    public DeactivateSubscriptionPlanHandler(
        ISubscriptionPlanRepository subscriptionPlanRepository,
        ILogger<DeactivateSubscriptionPlanHandler> logger)
    {
        _subscriptionPlanRepository = subscriptionPlanRepository;
        _logger = logger;
    }

    public async Task HandleAsync(DeactivateSubscriptionPlanCommand command)
    {
        var plan = await _subscriptionPlanRepository.GetByIdAsync(command.PlanId);
        if (plan is null)
        {
            _logger.LogWarning("Deactivate attempt for missing subscription plan. PlanId: {PlanId}", command.PlanId);
            throw new InvalidOperationException("Subscription plan not found.");
        }

        if (plan.MerchantId != command.MerchantId)
        {
            _logger.LogWarning("Merchant {MerchantId} attempted to deactivate a plan belonging to a different merchant. PlanId: {PlanId}", command.MerchantId, command.PlanId);
            throw new InvalidOperationException("You do not have permission to deactivate this plan.");
        }

        if (!plan.IsActive)
        {
            _logger.LogWarning("Attempt to deactivate an already-inactive plan. PlanId: {PlanId}", command.PlanId);
            throw new InvalidOperationException("Subscription plan is already inactive.");
        }

        plan.Deactivate();

        await _subscriptionPlanRepository.UpdateAsync(plan);

        _logger.LogInformation("Subscription plan deactivated: {PlanId}", plan.Id);
    }
}