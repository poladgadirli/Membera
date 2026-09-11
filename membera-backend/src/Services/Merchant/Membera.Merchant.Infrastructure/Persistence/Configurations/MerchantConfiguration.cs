using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MerchantEntity = Membera.Merchant.Domain.Entities.Merchant;

namespace Membera.Merchant.Infrastructure.Persistence.Configurations;

public class MerchantConfiguration : IEntityTypeConfiguration<MerchantEntity>
{
    public void Configure(EntityTypeBuilder<MerchantEntity> builder)
    {
        builder.ToTable("Merchants");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.OwnerId)
            .IsRequired();

        builder.Property(m => m.BusinessName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Description)
            .HasMaxLength(1000);

        builder.Property(m => m.LogoUrl)
            .HasMaxLength(500);

        builder.Property(m => m.BusinessCategory)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(m => m.IsActive)
            .IsRequired();
    }
}