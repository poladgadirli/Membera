using Membera.Merchant.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using MerchantEntity = Membera.Merchant.Domain.Entities.Merchant;

namespace Membera.Merchant.Infrastructure.Persistence;

public class MerchantRepository : IMerchantRepository
{
    private readonly MerchantDbContext _context;

    public MerchantRepository(MerchantDbContext context)
    {
        _context = context;
    }

    public async Task<MerchantEntity?> GetByIdAsync(Guid id)
    {
        return await _context.Merchants.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<MerchantEntity?> GetByOwnerIdAsync(Guid ownerId)
    {
        return await _context.Merchants.FirstOrDefaultAsync(m => m.OwnerId == ownerId);
    }

    public async Task AddAsync(MerchantEntity merchant)
    {
        await _context.Merchants.AddAsync(merchant);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(MerchantEntity merchant)
    {
        _context.Merchants.Update(merchant);
        await _context.SaveChangesAsync();
    }
}