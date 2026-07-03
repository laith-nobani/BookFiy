using System;

namespace BookFiy.Application.Dtos.Auth
{
    public class RegisterConfirmRequest
    {
        public string Email { get; set; }
        public string Code { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public Guid TenantId { get; set; }
    }
}
