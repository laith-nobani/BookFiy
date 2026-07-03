using BookFiy.Application.Dtos.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Application.Interfaces
{
    public interface IAuthService
    {

        public Task<LoginResponse> LoginAsync(LoginRequest request);
        public Task<RegisterResponse> RegisterAsync(RegisterRequest request);
        public Task<RegisterResponse> ConfirmRegisterAsync(RegisterConfirmRequest request);
        public Task ResendOtpAsync(string email);
        public Task<RefreshTokenResponse> RefreshToken(RefreshTokenRequest request); 
    }
}
