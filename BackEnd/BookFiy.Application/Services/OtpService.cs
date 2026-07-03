using BookFiy.Domain.Entites;
using BookFiy.Domain.IRepositories;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BookFiy.Application.Services
{
    public class OtpService : Interfaces.IOtpService
    {
        private readonly IOtpRepository _otpRepository;
        public OtpService(IOtpRepository otpRepository)
        {
            _otpRepository = otpRepository;
        }

        public async Task<string> CreateOtpAsync(string email, TimeSpan ttl)
        {
            var code = GenerateCode(6);
            var hash = HashCode(code);
            var otp = new Otp
            {
                Email = email,
                CodeHash = hash,
                ExpiresAt = DateTime.UtcNow.Add(ttl),
                CreatedAt = DateTime.UtcNow
            };
            await _otpRepository.AddAsync(otp);
            return code; 
        }

        public async Task<bool> VerifyOtpAsync(string email, string code)
        {
            var now = DateTime.UtcNow;
            var otp = await _otpRepository.GetLatestAsync(email, now);
            if (otp == null) return false;
            var hash = HashCode(code);
            if (hash != otp.CodeHash) return false;
            otp.IsUsed = true;
            await _otpRepository.UpdateAsync(otp);
            return true;
        }

        private static string GenerateCode(int length)
        {
            var rng = RandomNumberGenerator.Create();
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            var digits = new char[length];
            for (int i = 0; i < length; i++)
            {
                digits[i] = (char)('0' + (bytes[i] % 10));
            }
            return new string(digits);
        }

        private static string HashCode(string code)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(code);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
