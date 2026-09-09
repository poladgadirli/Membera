using Membera.Merchant.Application.Abstractions;
using Membera.Merchant.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Membera.Merchant.Application.Subscriptions.CreateCheckoutSession;

public class CreateCheckoutSessionHandler
{
    private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
    private readonly IUserSubscriptionRepository _userSubscriptionRepository;
    private readonly IStripeService _stripeService;
    private readonly ILogger<CreateCheckoutSessionHandler> _logger;

    public CreateCheckoutSessionHandler(
        ISubscriptionPlanRepository subscriptionPlanRepository,
        IUserSubscriptionRepository userSubscriptionRepository,
        IStripeService stripeService,
        ILogger<CreateCheckoutSessionHandler> logger)
    {
        _subscriptionPlanRepository = subscriptionPlanRepository;
        _userSubscriptionRepository = userSubscriptionRepository;
        _stripeService = stripeService;
        _logger = logger;
    }

    public async Task<CreateCheckoutSessionResult> HandleAsync(CreateCheckoutSessionCommand command)
    {
        var plan = await _subscriptionPlanRepository.GetByIdAsync(command.SubscriptionPlanId);
        if (plan is null)
        {
            _logger.LogWarning("Checkout attempt for a non-existent subscription plan. PlanId: {PlanId}", command.SubscriptionPlanId);
            throw new InvalidOperationException("Subscription plan not found.");
        }

        if (!plan.IsActive)
        {
            _logger.LogWarning("Checkout attempt for an inactive subscription plan. PlanId: {PlanId}", command.SubscriptionPlanId);
            throw new InvalidOperationException("This plan is not active.");
        }

        var (sessionId, checkoutUrl) = await _stripeService.CreateCheckoutSessionAsync(
            plan.Name, plan.Price, command.SuccessUrl, command.CancelUrl);

        var subscription = new UserSubscription(command.UserId, command.SubscriptionPlanId, sessionId);
        await _userSubscriptionRepository.AddAsync(subscription);

        _logger.LogInformation(
            "Checkout session created. UserId: {UserId}, PlanId: {PlanId}, SessionId: {SessionId}",
            command.UserId, command.SubscriptionPlanId, sessionId);

        return new CreateCheckoutSessionResult(checkoutUrl);
    }
}
