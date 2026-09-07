using Microsoft.EntityFrameworkCore;
using MerchantEntity = Membera.Merchant.Domain.Entities.Merchant;

namespace Membera.Merchant.Infrastructure.Persistence;

public class MerchantDbContext : DbContext
{
    public MerchantDbContext(DbContextOptions<MerchantDbContext> options) : base(options)
    {
    }

    public DbSet<MerchantEntity> Merchants => Set<MerchantEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MerchantDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}