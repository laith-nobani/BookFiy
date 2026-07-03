using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Application.Dtos.Admin
{
    public class AdminDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
