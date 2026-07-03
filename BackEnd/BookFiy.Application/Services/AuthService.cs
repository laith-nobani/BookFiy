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
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var validationResult = await _loginValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var token = await GenerateJwtToken(user);

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            };

            await _tokenRepository.AddAsync(refreshToken);


            return new LoginResponse
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

        }
        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            var validationResult = await _registerValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var existing = await _userManager.FindByEmailAsync(request.Email);
            if (existing != null)
            {
                throw new InvalidOperationException("Email is already registered.");
            }


            var otp = await _otpService.CreateOtpAsync(request.Email, TimeSpan.FromMinutes(5));
            await _emailService.SendOtpByEmailAsync(request.Email, otp);

            return new RegisterResponse
            {
                FullName = $"{request.FirstName} {request.LastName}",
                RoleName = "PendingVerification",
                UserName = request.UserName,
                Email = request.Email,
                TenantId = request.TenantId
            };
        }

        public async Task<RegisterResponse> ConfirmRegisterAsync(RegisterConfirmRequest request)
        {
            var ok = await _otpService.VerifyOtpAsync(request.Email, request.Code);
            if (!ok) throw new InvalidOperationException("Invalid or expired verification code.");

            var existing = await _userManager.FindByEmailAsync(request.Email);
            if (existing != null) throw new InvalidOperationException("Email is already registered.");

            var newUser = ApplicationUser.Create(request.UserName, request.Email, request.FirstName, request.LastName, request.PhoneNumber, request.TenantId);
            var result = await _userManager.CreateAsync(newUser, request.Password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            await _userManager.AddToRoleAsync(newUser, "Customer");

            return new RegisterResponse
            {
                FullName = newUser.FullName,
                RoleName = "Customer",
                UserName = newUser.UserName ?? "Username",
                Email = newUser.Email,
                TenantId = newUser.TenantId
            };

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

        public async Task ResendOtpAsync(string email)
        {
            var existing = await _userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                throw new InvalidOperationException("Email is already registered.");
            }
            var otp = await _otpService.CreateOtpAsync(email, TimeSpan.FromMinutes(5));
            await _emailService.SendOtpByEmailAsync(email, otp);

        }

        public async Task<RefreshTokenResponse> RefreshToken(RefreshTokenRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                throw new ArgumentException("Refresh token is required.", nameof(request.RefreshToken));
            }

            var refreshToken = await _tokenRepository.GetByTokenAsync(request.RefreshToken);
            if (refreshToken == null)
            {
                throw new InvalidOperationException("Invalid refresh token.");
            }

            if (refreshToken.IsRevoked || refreshToken.IsExpired)
            {
                throw new InvalidOperationException("Refresh token is no longer valid.");
            }

            var user = await _userManager.FindByIdAsync(refreshToken.UserId.ToString());
            if (user == null)
            {
                throw new InvalidOperationException("User associated with the refresh token not found.");
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
                return new RefreshTokenResponse
                {
                    Token = newToken,
                    RefreshToken = newRefreshToken.Token
                };
            }
            else
            {
                throw new InvalidOperationException("Failed to generate a new JWT token.");
            }
        }
    }
}

