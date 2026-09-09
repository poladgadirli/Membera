using Membera.Merchant.Domain.Entities;

namespace Membera.Merchant.Application.Abstractions;

public interface ISubscriptionPlanRepository
{
    Task<SubscriptionPlan?> GetByIdAsync(Guid id);
    Task<List<SubscriptionPlan>> GetByMerchantIdAsync(Guid merchantId);
    Task AddAsync(SubscriptionPlan plan);
    Task UpdateAsync(SubscriptionPlan plan);
}