using Membera.Merchant.Application.Abstractions;
using Membera.Merchant.Domain.Entities;
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

    public async Task<(List<SubscriptionPlan> Plans, int TotalCount)> GetPagedActiveAsync(int page, int pageSize)
    {
        var activePlans = _context.SubscriptionPlans.Where(p => p.IsActive);

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