using Membera.Shared.Domain;

namespace Membera.Merchant.Domain.Entities;

public class SubscriptionPlan : BaseEntity
{
    public Guid MerchantId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int DurationInDays { get; private set; }
    public int? UsageLimit { get; private set; }
    public TimeOnly ActiveFrom { get; private set; }
    public TimeOnly ActiveUntil { get; private set; }
    public bool IsActive { get; private set; }

    private SubscriptionPlan() { }

    public SubscriptionPlan(
        Guid merchantId,
        string name,
        string? description,
        decimal price,
        int durationInDays,
        int? usageLimit,
        TimeOnly activeFrom,
        TimeOnly activeUntil)
    {
        MerchantId = merchantId;
        Name = name;
        Description = description;
        Price = price;
        DurationInDays = durationInDays;
        UsageLimit = usageLimit;
        ActiveFrom = activeFrom;
        ActiveUntil = activeUntil;
        IsActive = true;
    }

    public void UpdateDetails(
        string name,
        string? description,
        decimal price,
        int durationInDays,
        int? usageLimit,
        TimeOnly activeFrom,
        TimeOnly activeUntil)
    {
        Name = name;
        Description = description;
        Price = price;
        DurationInDays = durationInDays;
        UsageLimit = usageLimit;
        ActiveFrom = activeFrom;
        ActiveUntil = activeUntil;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }
}