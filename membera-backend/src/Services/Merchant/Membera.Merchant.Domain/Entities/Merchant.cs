using Membera.Shared.Domain;

namespace Membera.Merchant.Domain.Entities;

public class Merchant : BaseEntity
{
    public Guid OwnerId { get; private set; }
    public string BusinessName { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private Merchant() { }

    public Merchant(Guid ownerId, string businessName)
    {
        OwnerId = ownerId;
        BusinessName = businessName;
        IsActive = true;
    }

    public void UpdateProfile(string businessName, string? description)
    {
        BusinessName = businessName;
        Description = description;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }
}