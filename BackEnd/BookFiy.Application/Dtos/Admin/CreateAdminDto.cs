using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Application.Dtos.Admin
{
    public class CreateAdminDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public Guid TenantId { get; set; }
    }
}
