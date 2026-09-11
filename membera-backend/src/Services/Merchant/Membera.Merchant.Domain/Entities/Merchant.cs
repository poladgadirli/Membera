using Membera.Merchant.Domain.Enums;
using Membera.Shared.Domain;

namespace Membera.Merchant.Domain.Entities;

public class Merchant : BaseEntity
{
    public Guid OwnerId { get; private set; }
    public string BusinessName { get; private set; }
    public string? Description { get; private set; }
    public string? LogoUrl { get; private set; }
    public BusinessCategory Category { get; private set; }
    public bool IsActive { get; private set; }

    private Merchant() { }

    public Merchant(Guid ownerId, string businessName, BusinessCategory category = BusinessCategory.Other)
    {
        OwnerId = ownerId;
        BusinessName = businessName;
        Category = category;
        IsActive = true;
    }

    public void UpdateProfile(string businessName, string? description, BusinessCategory category)
    {
        BusinessName = businessName;
        Description = description;
        Category = category;
        MarkAsUpdated();
    }

    public void UpdateLogo(string logoUrl)
    {
        LogoUrl = logoUrl;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }
}
