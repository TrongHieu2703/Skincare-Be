using Microsoft.AspNetCore.Mvc;
using Skincare.Services.Interfaces;
using Skincare.BusinessObjects.DTOs;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Skincare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(IAuthenticationService authService, ILogger<AuthenticationController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest == null)
                return BadRequest(new { Message = "Login request is null" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _authService.LoginAsync(loginRequest);
                if (result == null)
                    return Unauthorized(new { Message = "Invalid email or password" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { Message = "Internal server error", Error = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            if (registerRequest == null)
                return BadRequest(new { Message = "Register request is null" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _authService.RegisterAsync(registerRequest);
                if (!result)
                    return BadRequest(new { Message = "Registration failed. Email might be in use." });

                return Ok(new { Message = "User registered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new { Message = "Internal server error", Error = ex.Message });
            }
        }        

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest forgotPasswordRequest)
        {
            try
            {
                var result = await _authService.ForgotPasswordAsync(forgotPasswordRequest);
                if (!result)
                    return BadRequest(new { Message = "Failed to send OTP. Check email address." });

                return Ok(new { Message = "OTP sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password");
                return StatusCode(500, new { Message = "Internal server error", Error = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest resetPasswordRequest)
        {
            try
            {
                var result = await _authService.ResetPasswordAsync(resetPasswordRequest);
                if (!result)
                    return BadRequest(new { Message = "Failed to reset password. Check OTP and email." });

                return Ok(new { Message = "Password reset successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                return StatusCode(500, new { Message = "Internal server error", Error = ex.Message });
            }
        }
    }
}