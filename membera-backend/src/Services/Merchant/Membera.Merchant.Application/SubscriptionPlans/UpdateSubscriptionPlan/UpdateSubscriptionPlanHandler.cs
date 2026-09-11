using Membera.Merchant.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Membera.Merchant.Application.SubscriptionPlans.UpdateSubscriptionPlan;

public class UpdateSubscriptionPlanHandler
{
    private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
    private readonly ILogger<UpdateSubscriptionPlanHandler> _logger;

    public UpdateSubscriptionPlanHandler(
        ISubscriptionPlanRepository subscriptionPlanRepository,
        ILogger<UpdateSubscriptionPlanHandler> logger)
    {
        _subscriptionPlanRepository = subscriptionPlanRepository;
        _logger = logger;
    }

    public async Task HandleAsync(UpdateSubscriptionPlanCommand command)
    {
        var plan = await _subscriptionPlanRepository.GetByIdAsync(command.PlanId);
        if (plan is null)
        {
            _logger.LogWarning("Update attempt for missing subscription plan. PlanId: {PlanId}", command.PlanId);
            throw new InvalidOperationException("Subscription plan not found.");
        }

        if (plan.MerchantId != command.MerchantId)
        {
            _logger.LogWarning("Merchant {MerchantId} attempted to update a plan belonging to a different merchant. PlanId: {PlanId}", command.MerchantId, command.PlanId);
            throw new InvalidOperationException("You do not have permission to update this plan.");
        }

        plan.UpdateDetails(
            command.Name,
            command.Description,
            command.Price,
            command.DurationInDays,
            command.UsageLimit,
            command.ActiveFrom,
            command.ActiveUntil);

        await _subscriptionPlanRepository.UpdateAsync(plan);

        _logger.LogInformation("Subscription plan updated: {PlanId}", plan.Id);
    }
}