namespace Membera.Merchant.Application.Merchants.UploadMerchantLogo;

public record UploadMerchantLogoCommand(
    Guid MerchantId,
    Stream FileStream,
    string FileName,
    string ContentType
);
