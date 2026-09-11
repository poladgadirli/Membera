using Membera.Merchant.Domain.Enums;

namespace Membera.Merchant.Application.Merchants.CreateMerchant;

public record CreateMerchantCommand(Guid OwnerId, string BusinessName, BusinessCategory Category = BusinessCategory.Other);
