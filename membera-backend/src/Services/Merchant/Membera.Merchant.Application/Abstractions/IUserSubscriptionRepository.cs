using Membera.Merchant.Domain.Entities;

namespace Membera.Merchant.Application.Abstractions;

public interface IUserSubscriptionRepository
{
    Task<UserSubscription?> GetByIdAsync(Guid id);
    Task<UserSubscription?> GetByStripeSessionIdAsync(string stripeSessionId);
    Task<UserSubscription?> GetByRedemptionCodeAsync(string redemptionCode);
    Task<List<UserSubscription>> GetByUserIdAsync(Guid userId);
    Task AddAsync(UserSubscription subscription);
    Task UpdateAsync(UserSubscription subscription);
}
