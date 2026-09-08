using MerchantEntity = Membera.Merchant.Domain.Entities.Merchant;

namespace Membera.Merchant.Application.Abstractions;

public interface IMerchantRepository
{
    Task<MerchantEntity?> GetByIdAsync(Guid id);
    Task<MerchantEntity?> GetByOwnerIdAsync(Guid ownerId);
    Task AddAsync(MerchantEntity merchant);
    Task UpdateAsync(MerchantEntity merchant);
}