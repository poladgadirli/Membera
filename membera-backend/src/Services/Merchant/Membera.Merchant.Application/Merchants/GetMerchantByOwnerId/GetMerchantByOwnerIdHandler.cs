using Membera.Merchant.Application.Abstractions;
using Membera.Shared.Caching;
using Microsoft.Extensions.Logging;

namespace Membera.Merchant.Application.Merchants.GetMerchantByOwnerId;

public class GetMerchantByOwnerIdHandler
{
    private readonly IMerchantRepository _merchantRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetMerchantByOwnerIdHandler> _logger;

    public GetMerchantByOwnerIdHandler(
        IMerchantRepository merchantRepository,
        ICacheService cacheService,
        ILogger<GetMerchantByOwnerIdHandler> logger)
    {
        _merchantRepository = merchantRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<GetMerchantByOwnerIdResult> HandleAsync(GetMerchantByOwnerIdQuery query)
    {
        var cacheKey = $"merchant:owner:{query.OwnerId}";

        var cachedResult = await _cacheService.GetAsync<GetMerchantByOwnerIdResult>(cacheKey);
        if (cachedResult is not null)
        {
            _logger.LogInformation("Cache hit for merchant profile, OwnerId: {OwnerId}", query.OwnerId);
            return cachedResult;
        }

        _logger.LogInformation("Cache miss for merchant profile, OwnerId: {OwnerId}", query.OwnerId);

        var merchant = await _merchantRepository.GetByOwnerIdAsync(query.OwnerId);
        if (merchant is null)
        {
            _logger.LogWarning("Merchant profile not found for OwnerId: {OwnerId}", query.OwnerId);
            throw new InvalidOperationException("Merchant profile not found.");
        }

        var result = new GetMerchantByOwnerIdResult(
            merchant.Id, merchant.OwnerId, merchant.BusinessName, merchant.Description, merchant.LogoUrl,
            merchant.Category, merchant.IsActive);

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }
}