using Membera.Merchant.Application.Abstractions;

namespace Membera.Merchant.Application.Merchants.UpdateMerchant;

public class UpdateMerchantHandler
{
    private readonly IMerchantRepository _merchantRepository;

    public UpdateMerchantHandler(IMerchantRepository merchantRepository)
    {
        _merchantRepository = merchantRepository;
    }

    public async Task HandleAsync(UpdateMerchantCommand command)
    {
        var merchant = await _merchantRepository.GetByOwnerIdAsync(command.OwnerId);
        if (merchant is null)
            throw new InvalidOperationException("Merchant profile not found.");

        merchant.UpdateProfile(command.BusinessName, command.Description);

        await _merchantRepository.UpdateAsync(merchant);
    }
}