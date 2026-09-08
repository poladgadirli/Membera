using Membera.Merchant.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Membera.Merchant.Application.Merchants.GetMerchantByOwnerId;

public class GetMerchantByOwnerIdHandler
{
    private readonly IMerchantRepository _merchantRepository;
    private readonly ILogger<GetMerchantByOwnerIdHandler> _logger;

    public GetMerchantByOwnerIdHandler(IMerchantRepository merchantRepository, ILogger<GetMerchantByOwnerIdHandler> logger)
    {
        _merchantRepository = merchantRepository;
        _logger = logger;
    }

    public async Task<GetMerchantByOwnerIdResult> HandleAsync(GetMerchantByOwnerIdQuery query)
    {
        var merchant = await _merchantRepository.GetByOwnerIdAsync(query.OwnerId);
        if (merchant is null)
        {
            _logger.LogWarning("Merchant profile not found for OwnerId: {OwnerId}", query.OwnerId);
            throw new InvalidOperationException("Merchant profile not found.");
        }

        return new GetMerchantByOwnerIdResult(
            merchant.Id, merchant.OwnerId, merchant.BusinessName, merchant.Description, merchant.IsActive);
    }
}