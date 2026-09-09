using Membera.Merchant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Membera.Merchant.Infrastructure.Persistence.Configurations;

public class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
{
    public void Configure(EntityTypeBuilder<UserSubscription> builder)
    {
        builder.ToTable("UserSubscriptions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.SubscriptionPlanId)
            .IsRequired();

        builder.Property(s => s.RedemptionCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.StartedAt)
            .IsRequired();

        builder.Property(s => s.ExpiresAt)
            .IsRequired();

        builder.Property(s => s.UsagesRemaining);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.StripeSessionId)
            .HasMaxLength(500);

        builder.HasIndex(s => s.RedemptionCode)
            .IsUnique();

        builder.HasIndex(s => s.StripeSessionId);

        builder.HasIndex(s => s.UserId);
    }
}
