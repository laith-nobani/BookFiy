using BookFiy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Domain.Entites
{
    public class Employee : BaseEntity
    {
        public Guid UserId { get; set; }
        public string JobTitle { get; set; }
        public string Bio { get; set; }
        public Guid CreatedBy { get; set; }

        public ApplicationUser User { get; set; }
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; }

        public List<Service> services { get; set; } = new List<Service>();

        public bool IsDeleted { get; set; } = false;
        public Employee() { }

        public Employee Create(Guid userId, string jobTitle, string bio, Guid tenantId,Guid createdBy)
        {
            return new Employee
            {
                Id = userId,
                UserId = userId,
                JobTitle = jobTitle,
                Bio = bio,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }
        public Employee Update(string jobTitle, string bio) 
        {
            if(!string.IsNullOrEmpty(jobTitle))
              this.JobTitle = jobTitle;

            if(!string.IsNullOrEmpty(bio))
                this.Bio = bio;

            this.User.UpdatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            return this;
        }

        public void SoftDelete()
        {
            IsDeleted = true;
            UpdatedAt = DateTime.UtcNow;
        }



    }
}
