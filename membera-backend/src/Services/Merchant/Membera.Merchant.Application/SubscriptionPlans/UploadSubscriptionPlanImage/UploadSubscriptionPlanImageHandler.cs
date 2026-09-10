using Membera.Merchant.Application.Abstractions;
using Membera.Shared.Storage;
using Microsoft.Extensions.Logging;

namespace Membera.Merchant.Application.SubscriptionPlans.UploadSubscriptionPlanImage;

public class UploadSubscriptionPlanImageHandler
{
    private const string BucketName = "subscription-plan-images";

    private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<UploadSubscriptionPlanImageHandler> _logger;

    public UploadSubscriptionPlanImageHandler(
        ISubscriptionPlanRepository subscriptionPlanRepository,
        IFileStorageService fileStorageService,
        ILogger<UploadSubscriptionPlanImageHandler> logger)
    {
        _subscriptionPlanRepository = subscriptionPlanRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<string> HandleAsync(UploadSubscriptionPlanImageCommand command)
    {
        var plan = await _subscriptionPlanRepository.GetByIdAsync(command.PlanId);
        if (plan is null)
        {
            _logger.LogWarning("Image upload attempt for missing subscription plan. PlanId: {PlanId}", command.PlanId);
            throw new InvalidOperationException("Subscription plan not found.");
        }

        if (plan.MerchantId != command.MerchantId)
        {
            _logger.LogWarning("Merchant {MerchantId} attempted to upload an image for a plan belonging to a different merchant. PlanId: {PlanId}", command.MerchantId, command.PlanId);
            throw new InvalidOperationException("You do not have permission to update this plan.");
        }

        if (!command.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Subscription plan image upload rejected for non-image content type '{ContentType}'. PlanId: {PlanId}", command.ContentType, command.PlanId);
            throw new InvalidOperationException("Only image files are allowed.");
        }

        var objectName = $"{command.PlanId}/{Guid.NewGuid()}{Path.GetExtension(command.FileName)}";

        await _fileStorageService.UploadFileAsync(BucketName, objectName, command.FileStream, command.ContentType);

        var url = _fileStorageService.GetFileUrl(BucketName, objectName);

        plan.UpdateImage(url);
        await _subscriptionPlanRepository.UpdateAsync(plan);

        _logger.LogInformation("Subscription plan image updated. PlanId: {PlanId}, ObjectName: {ObjectName}", command.PlanId, objectName);

        return url;
    }
}
