using System;
using System.Collections.Generic;
using MailKit.Net.Smtp;
using MimeKit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using BookFiy.Domain.Entities;
using BookFiy.Application.Interfaces;

namespace BookFiy.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration? _configuration;
        private readonly UserManager<ApplicationUser>? _userManager;

   
        public EmailService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public Task SendOtpByEmailAsync(string email, string otp)
        {
            return SendEmail(email, "Your OTP Code", $"Your OTP code is: {otp}");
        }



        public async Task<string> CreateAndSendTemporaryPasswordAsync(string email,string tempPassword)
        {
            if (_userManager == null) throw new InvalidOperationException("UserManager not configured for EmailService.");

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                throw new InvalidOperationException("User not found.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, token, tempPassword);
            if (!resetResult.Succeeded)
                throw new InvalidOperationException(string.Join(", ", resetResult.Errors.Select(e => e.Description)));

            try
            {
                await SendEmailAsync(email, "Temporary password", $"Your temporary password is: {tempPassword}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send temporary password email: {ex.Message}");
            }

            return tempPassword;
        }

 

        public async Task SendEmail(string to, string subject, string body)
        {
            await SendEmailAsync(to, subject, body);
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            if (_configuration == null) throw new InvalidOperationException("Configuration not provided for EmailService.");

            var smtpHost = _configuration["SmtpSettings:Host"];
            var smtpPort = int.TryParse(_configuration["SmtpSettings:Port"], out var p) ? p : 587;
            var smtpUser = _configuration["SmtpSettings:User"];
            var smtpPass = _configuration["SmtpSettings:Pass"];
            var smtpFrom = _configuration["SmtpSettings:From"] ?? smtpUser;

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
                throw new InvalidOperationException("SMTP configuration is incomplete for EmailService.");

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(smtpFrom));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new MailKit.Net.Smtp.SmtpClient();
            var useSslOnConnect = bool.TryParse(_configuration["SmtpSettings:UseSslOnConnect"], out var useSsl) && useSsl;
            var secureOption = useSslOnConnect
                ? MailKit.Security.SecureSocketOptions.SslOnConnect
                : MailKit.Security.SecureSocketOptions.StartTls;
            await client.ConnectAsync(smtpHost, smtpPort, secureOption);
            if (!string.IsNullOrEmpty(smtpUser))
            {
                await client.AuthenticateAsync(smtpUser, smtpPass);
            }
            try
            {
                await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MailKit send failed: {ex.Message}");
                throw;
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
        private static string GenerateTemporaryPassword(int length = 12)
        {
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ"; 
            const string lower = "abcdefghijkmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string specials = "!@#$%&*?";
            var all = upper + lower + digits + specials;

            var rand = RandomNumberGenerator.Create();
            var bytes = new byte[length];
            rand.GetBytes(bytes);
            var chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = all[bytes[i] % all.Length];
            }

            var password = new string(chars);
            if (!password.Any(c => upper.Contains(c)))
                password = upper[(int)bytes[0] % upper.Length] + password.Substring(1);
            if (!password.Any(c => lower.Contains(c)))
                password = password.Substring(0, 1) + lower[(int)bytes[1] % lower.Length] + password.Substring(2);
            if (!password.Any(c => digits.Contains(c)))
                password = password.Substring(0, 2) + digits[(int)bytes[2] % digits.Length] + password.Substring(3);
            if (!password.Any(c => specials.Contains(c)))
                password = password.Substring(0, 3) + specials[(int)bytes[3] % specials.Length] + password.Substring(4);

            return password;
        }

      

       
    }
}
