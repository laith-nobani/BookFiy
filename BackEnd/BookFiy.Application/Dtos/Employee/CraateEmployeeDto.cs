using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Application.Dtos.Employee
{
    public class CraateEmployeeDto
    {
        public string FirstName { get; set; } 
        public string LastName { get; set; }
        public string JobTitle { get; set; }
        public string Bio { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public Guid TenantId { get; set; }
    }
}
