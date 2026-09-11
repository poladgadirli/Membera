using Membera.Merchant.Domain.Enums;

namespace Membera.Merchant.Application.Merchants.UpdateMerchant;

public record UpdateMerchantCommand(Guid OwnerId, string BusinessName, string? Description, BusinessCategory Category);
