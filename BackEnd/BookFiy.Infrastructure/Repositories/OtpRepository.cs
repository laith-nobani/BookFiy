using BookFiy.Domain.Entites;
using BookFiy.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Infrastructure.Repositories
{
    public class OtpRepository : Domain.IRepositories.IOtpRepository
    {
        private readonly AppDbContext _context;
        public OtpRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Otp otp)
        {
            await _context.Otps.AddAsync(otp);
            await _context.SaveChangesAsync();
        }

        public async Task<Otp?> GetLatestAsync(string email, DateTime now)
        {
            return await _context.Otps
                .Where(o => o.Email == email && o.ExpiresAt > now && !o.IsUsed)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(Otp otp)
        {

            var existingOtp = await _context.Otps.FindAsync(otp.Id);
            if (existingOtp != null)
            {
                existingOtp.IsUsed = otp.IsUsed;
                _context.Otps.Update(existingOtp);
                await _context.SaveChangesAsync();
            }


        }
    }
}
