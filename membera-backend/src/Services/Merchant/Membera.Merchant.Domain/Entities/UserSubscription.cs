using Membera.Merchant.Domain.Enums;
using Membera.Shared.Domain;

namespace Membera.Merchant.Domain.Entities;

public class UserSubscription : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid SubscriptionPlanId { get; private set; }
    public string RedemptionCode { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public int? UsagesRemaining { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public string? StripeSessionId { get; private set; }

    private UserSubscription() { }

    // Called when the user initiates checkout, before payment is confirmed.
    // Status starts as Pending until the Stripe webhook confirms payment.
    public UserSubscription(Guid userId, Guid subscriptionPlanId, string stripeSessionId)
    {
        UserId = userId;
        SubscriptionPlanId = subscriptionPlanId;
        StripeSessionId = stripeSessionId;
        Status = SubscriptionStatus.Pending;
        RedemptionCode = GenerateRedemptionCode();
    }

    // Called by the webhook handler once Stripe confirms the payment succeeded.
    public void Activate(int durationInDays, int? usageLimit)
    {
        Status = SubscriptionStatus.Active;
        StartedAt = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.AddDays(durationInDays);
        UsagesRemaining = usageLimit;
        MarkAsUpdated();
    }

    // Called each time the merchant redeems one usage from this subscription.
    public void Redeem()
    {
        if (Status != SubscriptionStatus.Active)
            throw new InvalidOperationException("This subscription is not active.");

        if (DateTime.UtcNow > ExpiresAt)
            throw new InvalidOperationException("This subscription has expired.");

        if (UsagesRemaining.HasValue)
        {
            if (UsagesRemaining.Value <= 0)
                throw new InvalidOperationException("No usages remaining on this subscription.");

            UsagesRemaining--;
        }

        MarkAsUpdated();
    }

    public void MarkAsExpired()
    {
        Status = SubscriptionStatus.Expired;
        MarkAsUpdated();
    }

    // Generates a short, human-readable code the customer can read out loud
    // or type in at the merchant's counter (e.g. "MBR-4X7K9P").
    private static string GenerateRedemptionCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // no O/0/I/1 to avoid confusion
        var random = new Random();
        var code = new string(Enumerable.Range(0, 6)
            .Select(_ => chars[random.Next(chars.Length)])
            .ToArray());

        return $"MBR-{code}";
    }
}