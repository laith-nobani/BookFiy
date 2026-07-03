using BookFiy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Domain.Entites
{

    public class Tenant
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty; 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public List<Employee> Employees { get; set; } = new List<Employee>();
    }
}
