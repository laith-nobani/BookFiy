using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Application.Dtos.Auth
{
    public class RegisterResponse
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string RoleName { get; set; }
        public string UserName { get; set; }
        public Guid TenantId { get; set; }

    }
}
