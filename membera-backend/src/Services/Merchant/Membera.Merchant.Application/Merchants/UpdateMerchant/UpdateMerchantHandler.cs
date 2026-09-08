using Membera.Merchant.Application.Abstractions;
using Membera.Shared.Caching;
using Microsoft.Extensions.Logging;

namespace Membera.Merchant.Application.Merchants.UpdateMerchant;

public class UpdateMerchantHandler
{
    private readonly IMerchantRepository _merchantRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<UpdateMerchantHandler> _logger;

    public UpdateMerchantHandler(
        IMerchantRepository merchantRepository,
        ICacheService cacheService,
        ILogger<UpdateMerchantHandler> logger)
    {
        _merchantRepository = merchantRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task HandleAsync(UpdateMerchantCommand command)
    {
        var merchant = await _merchantRepository.GetByOwnerIdAsync(command.OwnerId);
        if (merchant is null)
        {
            _logger.LogWarning("Merchant profile update attempt for missing profile. OwnerId: {OwnerId}", command.OwnerId);
            throw new InvalidOperationException("Merchant profile not found.");
        }

        merchant.UpdateProfile(command.BusinessName, command.Description);

        await _merchantRepository.UpdateAsync(merchant);

        var cacheKey = $"merchant:owner:{command.OwnerId}";
        await _cacheService.RemoveAsync(cacheKey);

        _logger.LogInformation("Merchant profile updated for OwnerId: {OwnerId}, BusinessName: {BusinessName}", command.OwnerId, merchant.BusinessName);
    }
}