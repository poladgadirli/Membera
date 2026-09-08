using Membera.Merchant.Application.Abstractions;
using Microsoft.Extensions.Logging;
using MerchantEntity = Membera.Merchant.Domain.Entities.Merchant;

namespace Membera.Merchant.Application.Merchants.CreateMerchant;

public class CreateMerchantHandler
{
    private readonly IMerchantRepository _merchantRepository;
    private readonly ILogger<CreateMerchantHandler> _logger;

    public CreateMerchantHandler(IMerchantRepository merchantRepository, ILogger<CreateMerchantHandler> logger)
    {
        _merchantRepository = merchantRepository;
        _logger = logger;
    }

    public async Task<CreateMerchantResult> HandleAsync(CreateMerchantCommand command)
    {
        var existingMerchant = await _merchantRepository.GetByOwnerIdAsync(command.OwnerId);
        if (existingMerchant is not null)
        {
            _logger.LogWarning("Merchant creation attempt for owner who already has a profile. OwnerId: {OwnerId}", command.OwnerId);
            throw new InvalidOperationException("This user already has a merchant profile.");
        }

        var merchant = new MerchantEntity(command.OwnerId, command.BusinessName);

        await _merchantRepository.AddAsync(merchant);

        _logger.LogInformation("New merchant profile created: {MerchantId}, OwnerId: {OwnerId}", merchant.Id, merchant.OwnerId);

        return new CreateMerchantResult(merchant.Id, merchant.BusinessName);
    }
}