namespace Membera.Merchant.Application.SubscriptionPlans.UploadSubscriptionPlanImage;

public record UploadSubscriptionPlanImageCommand(
    Guid PlanId,
    Guid MerchantId,
    Stream FileStream,
    string FileName,
    string ContentType
);
