using BookFiy.Api.Extensions;
using BookFiy.Application.Dtos.Auth;
using BookFiy.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookFiy.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {

            return (await _authService.LoginAsync(request))
              .ToActionResult();

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {

            return (await _authService.RegisterAsync(request))
      .             ToActionResult();

        }

        [HttpPost("register/confirm")]
        public async Task<IActionResult> ConfirmRegister(RegisterConfirmRequest request)
        {

            return (await _authService.ConfirmRegisterAsync(request))
                .ToActionResult();

        }
        [HttpPost("register/otp/resend")]
        public async Task<IActionResult> ResendOtp(string email)
        {
            return (await _authService.ResendOtpAsync(email))
                .ToActionResult();
        }

        [HttpPost("token/refresh")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
        {
            return (await _authService.RefreshToken(request))
                .ToActionResult();
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout(string refreshToken)
        {
            return (await _authService.LogoutAsync(refreshToken))
                .ToActionResult();

        }

        [HttpPost("password/reset")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            return (await _authService.ResetPasswordAsync(request))
                .ToActionResult();
        }
    }
}
