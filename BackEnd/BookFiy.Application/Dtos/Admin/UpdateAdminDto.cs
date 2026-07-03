using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Application.Dtos.Admin
{
    public class UpdateAdminDto
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string userName { get; set; } = null!;

    }
}
