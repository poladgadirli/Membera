using Membera.Merchant.Application.Abstractions;
using Membera.Shared.Storage;
using Microsoft.Extensions.Logging;

namespace Membera.Merchant.Application.Merchants.UploadMerchantLogo;

public class UploadMerchantLogoHandler
{
    private const string BucketName = "merchant-logos";

    private readonly IMerchantRepository _merchantRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<UploadMerchantLogoHandler> _logger;

    public UploadMerchantLogoHandler(
        IMerchantRepository merchantRepository,
        IFileStorageService fileStorageService,
        ILogger<UploadMerchantLogoHandler> logger)
    {
        _merchantRepository = merchantRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<string> HandleAsync(UploadMerchantLogoCommand command)
    {
        var merchant = await _merchantRepository.GetByIdAsync(command.MerchantId);
        if (merchant is null)
        {
            _logger.LogWarning("Logo upload attempt for missing merchant. MerchantId: {MerchantId}", command.MerchantId);
            throw new InvalidOperationException("Merchant not found.");
        }

        if (!command.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Logo upload rejected for non-image content type '{ContentType}'. MerchantId: {MerchantId}", command.ContentType, command.MerchantId);
            throw new InvalidOperationException("Only image files are allowed.");
        }

        var objectName = $"{command.MerchantId}/{Guid.NewGuid()}{Path.GetExtension(command.FileName)}";

        await _fileStorageService.UploadFileAsync(BucketName, objectName, command.FileStream, command.ContentType);

        var url = _fileStorageService.GetFileUrl(BucketName, objectName);

        merchant.UpdateLogo(url);
        await _merchantRepository.UpdateAsync(merchant);

        _logger.LogInformation("Merchant logo updated. MerchantId: {MerchantId}, ObjectName: {ObjectName}", command.MerchantId, objectName);

        return url;
    }
}
