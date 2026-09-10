namespace Membera.Merchant.Application.Merchants.GetMerchantByOwnerId;

public record GetMerchantByOwnerIdResult(
    Guid Id,
    Guid OwnerId,
    string BusinessName,
    string? Description,
    string? LogoUrl,
    bool IsActive
);