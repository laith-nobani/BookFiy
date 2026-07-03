using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Domain.IRepositories
{
    public interface IOtpRepository
    {
        public Task AddAsync(Entites.Otp otp);
        public Task<Entites.Otp?> GetLatestAsync(string email, DateTime now);
        public Task UpdateAsync(Entites.Otp otp);
    }
}
