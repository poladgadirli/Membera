namespace Membera.Merchant.Application.SubscriptionPlans.DeactivateSubscriptionPlan;

public record DeactivateSubscriptionPlanCommand(Guid PlanId, Guid MerchantId);