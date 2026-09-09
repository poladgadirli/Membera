using Membera.Merchant.Application.Abstractions;
using Membera.Merchant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Membera.Merchant.Infrastructure.Persistence;

public class UserSubscriptionRepository : IUserSubscriptionRepository
{
    private readonly MerchantDbContext _context;

    public UserSubscriptionRepository(MerchantDbContext context)
    {
        _context = context;
    }

    public async Task<UserSubscription?> GetByIdAsync(Guid id)
    {
        return await _context.UserSubscriptions.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<UserSubscription?> GetByStripeSessionIdAsync(string stripeSessionId)
    {
        return await _context.UserSubscriptions
            .FirstOrDefaultAsync(s => s.StripeSessionId == stripeSessionId);
    }

    public async Task<UserSubscription?> GetByRedemptionCodeAsync(string redemptionCode)
    {
        return await _context.UserSubscriptions
            .FirstOrDefaultAsync(s => s.RedemptionCode == redemptionCode);
    }

    public async Task<List<UserSubscription>> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserSubscriptions
            .Where(s => s.UserId == userId)
            .ToListAsync();
    }

    public async Task AddAsync(UserSubscription subscription)
    {
        await _context.UserSubscriptions.AddAsync(subscription);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(UserSubscription subscription)
    {
        _context.UserSubscriptions.Update(subscription);
        await _context.SaveChangesAsync();
    }
}
