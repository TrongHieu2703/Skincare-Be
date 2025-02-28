using Microsoft.AspNetCore.Mvc;
using Skincare.Services.Interfaces;
using Skincare.BusinessObjects.DTOs;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Skincare.API.Controllers
{
    [ApiController]
    [Route("api/authentication")]
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
                if (response == null)
                    return BadRequest(new { Message = "Email already exists." });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new { Message = "Internal server error", Error = ex.Message });
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
            // Đăng xuất thường không cần xử lý phức tạp vì JWT là stateless.
            // Bạn có thể xử lý phía client bằng cách xóa token khỏi localStorage/sessionStorage.
            return Ok(new { Message = "Logged out successfully." });
        }
    }
}