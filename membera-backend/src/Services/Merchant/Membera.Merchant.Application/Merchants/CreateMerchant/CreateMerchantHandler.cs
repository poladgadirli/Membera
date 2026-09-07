using Membera.Merchant.Application.Abstractions;
using MerchantEntity = Membera.Merchant.Domain.Entities.Merchant;

namespace Membera.Merchant.Application.Merchants.CreateMerchant;

public class CreateMerchantHandler
{
    private readonly IMerchantRepository _merchantRepository;

    public CreateMerchantHandler(IMerchantRepository merchantRepository)
    {
        _merchantRepository = merchantRepository;
    }

    public async Task<CreateMerchantResult> HandleAsync(CreateMerchantCommand command)
    {
        var existingMerchant = await _merchantRepository.GetByOwnerIdAsync(command.OwnerId);
        if (existingMerchant is not null)
            throw new InvalidOperationException("This user already has a merchant profile.");

        var merchant = new MerchantEntity(command.OwnerId, command.BusinessName);

        await _merchantRepository.AddAsync(merchant);

        return new CreateMerchantResult(merchant.Id, merchant.BusinessName);
    }
}