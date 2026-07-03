using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Application.Interfaces
{
    public interface IEmailService
    {
        public Task<string> CreateAndSendTemporaryPasswordAsync(string email,string tempPassword);
        public Task SendEmail(string to, string subject, string body);
        public Task SendOtpByEmailAsync(string email, string otp);

    
    }
}
