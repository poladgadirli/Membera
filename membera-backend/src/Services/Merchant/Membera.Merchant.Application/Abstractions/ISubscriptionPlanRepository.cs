using Membera.Merchant.Domain.Entities;

namespace Membera.Merchant.Application.Abstractions;

public interface ISubscriptionPlanRepository
{
    Task<SubscriptionPlan?> GetByIdAsync(Guid id);
    Task<List<SubscriptionPlan>> GetByMerchantIdAsync(Guid merchantId);

    /// <summary>All active plans across every merchant (for the public browse page).</summary>
    Task<List<SubscriptionPlan>> GetAllActiveAsync();
    Task AddAsync(SubscriptionPlan plan);
    Task UpdateAsync(SubscriptionPlan plan);
}