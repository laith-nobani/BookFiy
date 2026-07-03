using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Application.Dtos.Tenant
{
    public class TenantDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
