using Membera.Merchant.Application.Abstractions;

namespace Membera.Merchant.Application.Merchants.GetMerchantByOwnerId;

public class GetMerchantByOwnerIdHandler
{
    private readonly IMerchantRepository _merchantRepository;

    public GetMerchantByOwnerIdHandler(IMerchantRepository merchantRepository)
    {
        _merchantRepository = merchantRepository;
    }

    public async Task<GetMerchantByOwnerIdResult> HandleAsync(GetMerchantByOwnerIdQuery query)
    {
        var merchant = await _merchantRepository.GetByOwnerIdAsync(query.OwnerId);
        if (merchant is null)
            throw new InvalidOperationException("Merchant profile not found.");

        return new GetMerchantByOwnerIdResult(
            merchant.Id, merchant.OwnerId, merchant.BusinessName, merchant.Description, merchant.IsActive);
    }
}