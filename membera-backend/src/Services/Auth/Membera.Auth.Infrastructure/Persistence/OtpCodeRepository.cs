using Membera.Auth.Application.Abstractions;
using Membera.Auth.Domain.Entities;
using Membera.Auth.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Membera.Auth.Infrastructure.Persistence;

public class OtpCodeRepository : IOtpCodeRepository
{
    private readonly AuthDbContext _context;

    public OtpCodeRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(OtpCode otpCode)
    {
        await _context.OtpCodes.AddAsync(otpCode);
        await _context.SaveChangesAsync();
    }

    public async Task<OtpCode?> GetLatestAsync(Guid userId, OtpPurpose purpose, string code)
    {
        return await _context.OtpCodes
            .Where(o => o.UserId == userId && o.Purpose == purpose && o.Code == code)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(OtpCode otpCode)
    {
        _context.OtpCodes.Update(otpCode);
        await _context.SaveChangesAsync();
    }
}
