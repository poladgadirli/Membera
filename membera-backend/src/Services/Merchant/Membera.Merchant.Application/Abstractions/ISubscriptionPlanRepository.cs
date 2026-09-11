using Membera.Merchant.Domain.Entities;

namespace Membera.Merchant.Application.Abstractions;

public interface ISubscriptionPlanRepository
{
    Task<SubscriptionPlan?> GetByIdAsync(Guid id);
    Task<List<SubscriptionPlan>> GetByMerchantIdAsync(Guid merchantId);

    /// <summary>One page of active plans across every merchant, plus the total
    /// active count (for the public browse page).</summary>
    Task<(List<SubscriptionPlan> Plans, int TotalCount)> GetPagedActiveAsync(int page, int pageSize);
    Task AddAsync(SubscriptionPlan plan);
    Task UpdateAsync(SubscriptionPlan plan);
}