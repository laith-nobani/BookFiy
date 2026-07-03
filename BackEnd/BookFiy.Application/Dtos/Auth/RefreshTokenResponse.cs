using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Application.Dtos.Auth
{
    public class RefreshTokenResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
