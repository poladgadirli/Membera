using Membera.Merchant.Domain.Entities;
using Membera.Merchant.Domain.Enums;

namespace Membera.Merchant.Application.Abstractions;

public interface ISubscriptionPlanRepository
{
    Task<SubscriptionPlan?> GetByIdAsync(Guid id);
    Task<List<SubscriptionPlan>> GetByMerchantIdAsync(Guid merchantId);

    /// <summary>One page of active plans across every merchant, plus the total
    /// active count (for the public browse page). When <paramref name="category"/>
    /// is given, only plans whose merchant matches that category are included.</summary>
    Task<(List<SubscriptionPlan> Plans, int TotalCount)> GetPagedActiveAsync(int page, int pageSize, BusinessCategory? category = null);
    Task AddAsync(SubscriptionPlan plan);
    Task UpdateAsync(SubscriptionPlan plan);
}
