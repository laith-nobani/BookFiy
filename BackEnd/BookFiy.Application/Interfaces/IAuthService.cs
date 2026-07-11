using BookFiy.Application.Comman;
using BookFiy.Application.Dtos.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Application.Interfaces
{
    public interface IAuthService
    {

        public Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
        public Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request);
        public Task<Result<RegisterResponse>> ConfirmRegisterAsync(RegisterConfirmRequest request);
        public Task<Result<bool>> ResendOtpAsync(string email);
        public Task<Result<RefreshTokenResponse>> RefreshToken(RefreshTokenRequest request); 
        public Task<Result<bool>> LogoutAsync(string refreshToken);
        public Task<Result<bool>> ResetPasswordAsync(ResetPasswordRequest request);
    }
}
