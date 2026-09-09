using Membera.Merchant.Application.Abstractions;
using Membera.Merchant.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Membera.Merchant.Application.SubscriptionPlans.CreateSubscriptionPlan;

public class CreateSubscriptionPlanHandler
{
    private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
    private readonly IMerchantRepository _merchantRepository;
    private readonly ILogger<CreateSubscriptionPlanHandler> _logger;

    public CreateSubscriptionPlanHandler(
        ISubscriptionPlanRepository subscriptionPlanRepository,
        IMerchantRepository merchantRepository,
        ILogger<CreateSubscriptionPlanHandler> logger)
    {
        _subscriptionPlanRepository = subscriptionPlanRepository;
        _merchantRepository = merchantRepository;
        _logger = logger;
    }

    public async Task<CreateSubscriptionPlanResult> HandleAsync(CreateSubscriptionPlanCommand command)
    {
        var merchant = await _merchantRepository.GetByIdAsync(command.MerchantId);
        if (merchant is null)
        {
            _logger.LogWarning("Attempt to create a plan for a non-existent merchant. MerchantId: {MerchantId}", command.MerchantId);
            throw new InvalidOperationException("Merchant not found.");
        }

        var plan = new SubscriptionPlan(
            command.MerchantId,
            command.Name,
            command.Description,
            command.Price,
            command.DurationInDays,
            command.UsageLimit,
            command.ActiveFrom,
            command.ActiveUntil);

        await _subscriptionPlanRepository.AddAsync(plan);

        _logger.LogInformation("Subscription plan created: {PlanId} for MerchantId: {MerchantId}", plan.Id, command.MerchantId);

        return new CreateSubscriptionPlanResult(plan.Id, plan.Name, plan.Price);
    }
}