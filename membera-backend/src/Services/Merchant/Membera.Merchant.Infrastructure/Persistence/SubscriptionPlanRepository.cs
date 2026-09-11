using Membera.Merchant.Application.Abstractions;
using Membera.Merchant.Domain.Entities;
using Membera.Merchant.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Membera.Merchant.Infrastructure.Persistence;

public class SubscriptionPlanRepository : ISubscriptionPlanRepository
{
    private readonly MerchantDbContext _context;

    public SubscriptionPlanRepository(MerchantDbContext context)
    {
        _context = context;
    }

    public async Task<SubscriptionPlan?> GetByIdAsync(Guid id)
    {
        return await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<SubscriptionPlan>> GetByMerchantIdAsync(Guid merchantId)
    {
        return await _context.SubscriptionPlans
            .Where(p => p.MerchantId == merchantId)
            .ToListAsync();
    }

    public async Task<(List<SubscriptionPlan> Plans, int TotalCount)> GetPagedActiveAsync(int page, int pageSize, BusinessCategory? category = null)
    {
        var activePlans = _context.SubscriptionPlans.Where(p => p.IsActive);

        if (category is not null)
        {
            var merchantIdsInCategory = _context.Merchants
                .Where(m => m.Category == category)
                .Select(m => m.Id);

            activePlans = activePlans.Where(p => merchantIdsInCategory.Contains(p.MerchantId));
        }

        var totalCount = await activePlans.CountAsync();

        var plans = await activePlans
            .OrderBy(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (plans, totalCount);
    }

    public async Task AddAsync(SubscriptionPlan plan)
    {
        await _context.SubscriptionPlans.AddAsync(plan);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(SubscriptionPlan plan)
    {
        _context.SubscriptionPlans.Update(plan);
        await _context.SaveChangesAsync();
    }
}