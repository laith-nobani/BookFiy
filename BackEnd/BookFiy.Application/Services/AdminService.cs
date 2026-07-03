using BookFiy.Application.Dtos.Admin;
using BookFiy.Application.Dtos.Auth;
using BookFiy.Application.Interfaces;
using BookFiy.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;


        public AdminService(UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }
        public async Task<AdminDto> CreateAdminAsync(CreateAdminDto admin)
        {
            if (admin == null)
                throw new ArgumentNullException(nameof(admin));

            var userExists = await _userManager.FindByEmailAsync(admin.Email);
            if (userExists != null)
                throw new InvalidOperationException("User with this email already exists.");


            var user = ApplicationUser.Create(admin.UserName, admin.Email, admin.FirstName, admin.LastName, admin.PhoneNumber, admin.TenantId);

            var temporaryPassword = GenerateTemporaryPassword();

            var result = await _userManager.CreateAsync(user, temporaryPassword);

            if (!result.Succeeded)
                throw new InvalidOperationException("Failed to create admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));


             await _emailService.CreateAndSendTemporaryPasswordAsync(admin.Email, temporaryPassword);
        
            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, "Admin");
                if (!roleResult.Succeeded)
                    throw new InvalidOperationException("Failed to assign admin role: " + string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }

            return new AdminDto
            {
                Id = user.Id,
                Name = $"{admin.FirstName} {admin.LastName}",
                Email = admin.Email,
                Role = "Admin"
            };


        }

        public string GenerateTemporaryPassword(int length = 12)
        {
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            var random = new Random();
            var passwordChars = new char[length];
            for (int i = 0; i < length; i++)
            {
                passwordChars[i] = validChars[random.Next(validChars.Length)];
            }
            return new string(passwordChars);
        }

        public async Task<List<AdminDto>> GetAllAdminsAsync()
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");

            return admins.Select(a => new AdminDto
            {
                Id = a.Id,
                Name = a.FullName,
                Email = a.Email!,
                Role = "Admin"
            }).ToList();
        }
        public async Task<AdminDto> GetAdminByIdAsync(Guid adminId)
        {
            var user = await _userManager.FindByIdAsync(adminId.ToString());
            if (user == null)
                throw new KeyNotFoundException("Admin not found.");
            return new AdminDto
            {
                Id = user.Id,
                Name = $"{user.FullName}",
                Email = user.Email,
                Role = "Admin"
            };


        }
        public async Task UpdateAdminAsync(Guid adminId, UpdateAdminDto admin)
        {
            var user = await _userManager.FindByIdAsync(adminId.ToString());
            if (user == null)
                throw new KeyNotFoundException("Admin not found.");

            user.UpdateUser(admin.userName, admin.Email, $"{admin.FirstName} {admin.LastName}", admin.PhoneNumber);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException("Failed to update admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            await _userManager.UpdateAsync(user);
        }

    }
 }
