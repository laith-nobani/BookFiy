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

            var result = await _authService.LoginAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {

            var result = await _authService.RegisterAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);

            }
            return Ok(result.Data);
        }

        [HttpPost("register/confirm-register")]
        public async Task<IActionResult> ConfirmRegister(RegisterConfirmRequest request)
        {

            var result = await _authService.ConfirmRegisterAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);

        }
        [HttpPost("register/resend-otp")]
        public async Task<IActionResult> ResendOtp(string email)
        {
            var result = await _authService.ResendOtpAsync(email);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Message);
        }

        [HttpPost("Refresh-Token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
        {
            var result = await _authService.RefreshToken(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout(string refreshToken)
        {
            var result = await _authService.LogoutAsync(refreshToken);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Message);
        }

        [HttpPost("Reset-Password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var result = await _authService.ResetPasswordAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Message);

        }
    }
}
