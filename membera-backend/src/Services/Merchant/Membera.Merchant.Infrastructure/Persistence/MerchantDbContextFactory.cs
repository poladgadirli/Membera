using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Membera.Merchant.Infrastructure.Persistence;

public class MerchantDbContextFactory : IDesignTimeDbContextFactory<MerchantDbContext>
{
    public MerchantDbContext CreateDbContext(string[] args)
    {
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "postgres";

        var optionsBuilder = new DbContextOptionsBuilder<MerchantDbContext>();

        optionsBuilder.UseNpgsql(
            $"Host=localhost;Port=5432;Database=membera_merchant;Username=postgres;Password={password}");

        return new MerchantDbContext(optionsBuilder.Options);
    }
}