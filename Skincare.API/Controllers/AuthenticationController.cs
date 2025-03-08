using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Skincare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(IAuthenticationService authenticationService, ILogger<AuthenticationController> logger)
        {
            _authenticationService = authenticationService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var response = await _authenticationService.RegisterAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration: {Message}", ex.Message);
                return StatusCode(500, new { 
                    Message = "Đăng ký thất bại", 
                    Error = ex.Message,
                    Details = ex.InnerException?.Message 
                });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var response = await _authenticationService.LoginAsync(request);
                if (response == null)
                    return Unauthorized(new { Message = "Invalid email or password." });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { Message = "Internal server error", Error = ex.Message });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Với JWT (stateless), logout thường do client tự xóa token
            return Ok(new { Message = "Logged out successfully." });
        }
    }
}
