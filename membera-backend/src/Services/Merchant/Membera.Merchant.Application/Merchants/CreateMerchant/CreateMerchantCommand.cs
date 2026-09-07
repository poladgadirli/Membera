namespace Membera.Merchant.Application.Merchants.CreateMerchant;

public record CreateMerchantCommand(Guid OwnerId, string BusinessName);