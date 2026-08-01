using BookFiy.Application.Comman;
using BookFiy.Application.Dtos.Auth;
using BookFiy.Application.Interfaces;
using BookFiy.Domain.Entites;
using BookFiy.Domain.Entities;
using BookFiy.Domain.IRepositories;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BookFiy.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IValidator<LoginRequest> _loginValidator;
        private readonly IValidator<RegisterRequest> _registerValidator;
        private readonly IEmailService _emailService;
        private readonly IOtpService _otpService;
        private readonly IRefreshTokenRepository _tokenRepository;

        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration, IValidator<LoginRequest> loginValidator, IValidator<RegisterRequest> registerValidator,
            IEmailService emailService, IOtpService otpService, IRefreshTokenRepository tokenRepository)
        {
            _userManager = userManager;
            _configuration = configuration;
            _loginValidator = loginValidator;
            _registerValidator = registerValidator;
            _emailService = emailService;
            _otpService = otpService;
            _tokenRepository = tokenRepository;
        }
        public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
        {
            var validationResult = await _loginValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return Result<LoginResponse>.Failure(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)), ErrorType.Validation);
            }

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return Result<LoginResponse>.Failure("Invalid email or password.", ErrorType.Validation);
            }

            var token = await GenerateJwtToken(user);

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            };

            await _tokenRepository.AddAsync(refreshToken);


            var res= new LoginResponse
            {
                FullName = user.FullName,
                RoleName = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "Role",
                Username = user.UserName ?? "",
                RefreshToken = refreshToken.Token,
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(
                  Convert.ToInt32(_configuration["Jwt:durationInMinutes"])
                  ),
                TenantId = user.TenantId,
                userId= user.Id
            };

            return Result<LoginResponse>.Success(res);

        }
        public async Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request)
        {
            var validationResult = await _registerValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return Result<RegisterResponse>.Failure(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)),ErrorType.Validation);
            }

            var existing = await _userManager.FindByEmailAsync(request.Email);
            if (existing != null)
            {
                return Result<RegisterResponse>.Failure("Email is already registered.", ErrorType.Conflict);
            }


            var otp = await _otpService.CreateOtpAsync(request.Email, TimeSpan.FromMinutes(5));
            await _emailService.SendOtpByEmailAsync(request.Email, otp);

            var res= new RegisterResponse
            {
                FullName = $"{request.FirstName} {request.LastName}",
                RoleName = "PendingVerification",
                UserName = request.UserName,
                Email = request.Email,
                TenantId = request.TenantId
            };
            return Result<RegisterResponse>.Success(res);
        }

        public async Task<Result<RegisterResponse>> ConfirmRegisterAsync(RegisterConfirmRequest request)
        {
            var ok = await _otpService.VerifyOtpAsync(request.Email, request.Code);
            if (!ok) 
            {
                return Result<RegisterResponse>.Failure("Invalid or expired OTP.",ErrorType.Validation);
            }

            var existing = await _userManager.FindByEmailAsync(request.Email);
            if (existing != null)
                return Result<RegisterResponse>.Failure("Email is already registered.", ErrorType.Conflict);

            var newUser = ApplicationUser.Create(request.UserName, request.Email, request.FirstName, request.LastName, request.PhoneNumber, request.TenantId);
            var result = await _userManager.CreateAsync(newUser, request.Password);
            if (!result.Succeeded)
            {
               return Result<RegisterResponse>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)), ErrorType.Validation);
            }
            await _userManager.AddToRoleAsync(newUser, "Customer");

            var res= new RegisterResponse
            {
                FullName = newUser.FullName,
                RoleName = "Customer",
                UserName = newUser.UserName ?? "Username",
                Email = newUser.Email,
                TenantId = newUser.TenantId
            };

            return Result<RegisterResponse>.Success(res);

        }

        public async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? "Name"),
                new Claim(ClaimTypes.Email, user.Email ?? "email"),
                new Claim("tenant_id", user.TenantId.ToString()),
            };
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:key"]!));

            var creds = new SigningCredentials(
                 key,
                 SecurityAlgorithms.HmacSha256
             );

            claims = claims.Concat(userRoles.Select(role => new Claim(ClaimTypes.Role, role))).ToArray();

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:issuer"],
                audience: _configuration["Jwt:audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                  Convert.ToInt32(_configuration["Jwt:durationInMinutes"])
                  ),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        public async Task<Result<bool>> ResendOtpAsync(string email)
        {
            var existing = await _userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                return Result<bool>.Failure("Email is already registered.", ErrorType.Conflict);

            }
            var otp = await _otpService.CreateOtpAsync(email, TimeSpan.FromMinutes(5));
            await _emailService.SendOtpByEmailAsync(email, otp);

            return Result<bool>.Success(true, "OTP sent successfully.");

        }

        public async Task<Result<RefreshTokenResponse>> RefreshToken(RefreshTokenRequest request)
        {
            if (request == null)
            {
                return Result<RefreshTokenResponse>.Failure("Request cannot be null.", ErrorType.Validation);
            }

            if (string.IsNullOrEmpty(request.RefreshToken))
            {
               return Result<RefreshTokenResponse>.Failure("Refresh token cannot be null or empty.", ErrorType.Validation);
            }

            var refreshToken = await _tokenRepository.GetByTokenAsync(request.RefreshToken);
            if (refreshToken == null)
            {
                return  Result<RefreshTokenResponse>.Failure("Refresh token not found.", ErrorType.Validation);
            }

            if (refreshToken.IsRevoked || refreshToken.IsExpired)
            {
                return Result<RefreshTokenResponse>.Failure("Refresh token is either revoked or expired.", ErrorType.Validation);
            }

            var user = await _userManager.FindByIdAsync(refreshToken.UserId.ToString());
            if (user == null)
            {
                return Result<RefreshTokenResponse>.Failure("User associated with the refresh token not found.", ErrorType.Validation);
            }

            var newToken = await GenerateJwtToken(user);
            if (newToken != null) {
                await _tokenRepository.RevokeAsync(refreshToken);
                var newRefreshToken = new RefreshToken
                {
                    UserId = user.Id,
                    Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                    ExpiresAt = DateTime.UtcNow.AddDays(30)
                };
                await _tokenRepository.AddAsync(newRefreshToken);
                var res= new RefreshTokenResponse
                {
                    Token = newToken,
                    RefreshToken = newRefreshToken.Token
                };
                return Result<RefreshTokenResponse>.Success(res);
            }
           
            return Result<RefreshTokenResponse>.Failure("Failed to generate new JWT token.", ErrorType.Validation);
        }

        public async Task<Result<bool>> LogoutAsync(string refreshToken)
        {
            var token =await _tokenRepository.GetByTokenAsync(refreshToken);
            if (token == null)
            {
                return Result<bool>.Failure("Refresh token not found.", ErrorType.Validation);
            }

            if (token.IsRevoked)
            {
                return Result<bool>.Failure("Refresh token is already revoked.", ErrorType.Validation);
            }
            if (token.IsExpired)
            {
                return Result<bool>.Failure("Refresh token is expired.", ErrorType.Validation);
            }

            await _tokenRepository.RevokeAsync(token);
            return Result<bool>.Success(true, "Refresh token revoked successfully.");


        }

        public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result<bool>.Failure("User not found.", ErrorType.Validation);
            }

            var isOldPasswordValid = await _userManager.CheckPasswordAsync(user, request.OldPassword);
            if (!isOldPasswordValid)
            {
                return Result<bool>.Failure("Old password is incorrect.", ErrorType.Validation);
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            if (resetToken == null)
            {
                return Result<bool>.Failure("Failed to generate password reset token.", ErrorType.Validation);
            }

            var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

            if (!resetResult.Succeeded)
            {
                return Result<bool>.Failure(string.Join(", ", resetResult.Errors.Select(e => e.Description)), ErrorType.Validation);
            }
           
            return Result<bool>.Success(true, "Password reset successfully.");


        }
    }
}

