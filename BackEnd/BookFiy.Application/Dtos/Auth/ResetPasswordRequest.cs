using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Application.Dtos.Auth
{
    public class ResetPasswordRequest
    {
        public string Email { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
