using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Exceptions; // import custom exceptions
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
                return BadRequest(new { message = "Invalid request data", errors = ModelState });

            try
            {
                var response = await _authenticationService.RegisterAsync(request);
                // Nếu thành công, service trả về LoginResponse
                return Ok(new { message = "Registration successful", data = response });
            }
            catch (DuplicateEmailException dex)
            {
                // Email trùng -> 409 Conflict
                _logger.LogWarning(dex, "Email already exists");
                return Conflict(new { message = dex.Message, errorCode = "DUPLICATE_EMAIL" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid request data", errors = ModelState });

            try
            {
                var response = await _authenticationService.LoginAsync(request);
                if (response == null)
                {
                    // Không tạo custom exception, ta vẫn trả 401
                    return Unauthorized(new { message = "Invalid email or password." });
                }

                return Ok(new { message = "Login successful", data = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Với JWT (stateless), logout = client xóa token
            return Ok(new { message = "Logged out successfully." });
        }
    }
}
