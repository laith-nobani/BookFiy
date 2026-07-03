using BookFiy.Domain.Entites;
using Microsoft.AspNetCore.Identity;
using System;

namespace BookFiy.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public Guid TenantId { get; set; } 
        public string FullName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Tenant Tenant { get; set; } = null!;
        public Employee? Employee { get; set; }
        public bool IsDeleted { get; set; } = false;

        public ApplicationUser()
        {
        }

        public static ApplicationUser Create(
            string userName,
            string email,
            string firstName,
            string lastName,
            string phoneNumber,
            Guid tenantId)
        {
            return new ApplicationUser
            {
                UserName = userName,
                Email = email,
                TenantId = tenantId,
                FullName = $"{firstName} {lastName}",
                PhoneNumber = phoneNumber,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void UpdateUser(string userName, string email, string fullName,string phoneNumber)
        {
            if (!string.IsNullOrWhiteSpace(userName))
                UserName = userName;
            if (!string.IsNullOrWhiteSpace(email))
                Email = email;
            if (!string.IsNullOrWhiteSpace(fullName))
               FullName = fullName;
            if (!string.IsNullOrWhiteSpace(phoneNumber))
                PhoneNumber = phoneNumber;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SoftDelete()
        {
            IsDeleted = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}