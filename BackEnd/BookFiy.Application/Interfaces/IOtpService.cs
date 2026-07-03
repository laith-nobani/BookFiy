using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Application.Interfaces
{
    public interface IOtpService
    {
        public Task<string> CreateOtpAsync(string email, TimeSpan ttl);
        public Task<bool> VerifyOtpAsync(string email, string code);

    }
}
