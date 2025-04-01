using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Exceptions; // import custom exceptions
using Skincare.Services.Interfaces;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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
            catch (DuplicatePhoneNumberException dpex)
            {
                // Số điện thoại trùng -> 409 Conflict
                _logger.LogWarning(dpex, "Phone number already exists");
                return Conflict(new { message = dpex.Message, errorCode = "DUPLICATE_PHONE" });
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

        [HttpPost("register-with-avatar")]
        public async Task<IActionResult> RegisterWithAvatar(
            [FromForm] string username, 
            [FromForm] string email, 
            [FromForm] string password, 
            [FromForm] string phoneNumber, 
            [FromForm] string address, 
            IFormFile? avatar) // Explicitly mark as nullable
        {
            try
            {
                _logger.LogInformation("Starting register-with-avatar: Username={Username}, Email={Email}, HasAvatar={HasAvatar}", 
                    username, email, avatar != null);
                
                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Invalid request data", errors = ModelState });
                
                // Upload avatar trước
                string? avatarUrl = null;
                if (avatar != null && avatar.Length > 0)
                {
                    try
                    {
                        _logger.LogInformation("Uploading avatar file: {FileName}, {FileSize}KB, {ContentType}", 
                            avatar.FileName, avatar.Length / 1024, avatar.ContentType);
                        
                        // Upload avatar và lấy URL
                        avatarUrl = await _authenticationService.UploadAvatarForRegistration(avatar);
                        
                        _logger.LogInformation("Avatar uploaded successfully. URL: {AvatarUrl}", avatarUrl);
                    }
                    catch (ArgumentException ex)
                    {
                        // Nếu có lỗi khi upload avatar, ghi log nhưng vẫn tiếp tục đăng ký
                        _logger.LogWarning(ex, "Error uploading avatar during registration, continuing without avatar");
                    }
                }
                
                // Tạo request đăng ký với avatar URL
                var request = new RegisterRequest
                {
                    Username = username,
                    Email = email,
                    Password = password,
                    PhoneNumber = phoneNumber,
                    Address = address,
                    Avatar = avatarUrl  // Có thể null nếu không có avatar
                };
                
                _logger.LogInformation("Creating account with Username={Username}, Email={Email}, AvatarUrl={AvatarUrl}", 
                    request.Username, request.Email, request.Avatar);
                
                var response = await _authenticationService.RegisterAsync(request);
                
                _logger.LogInformation("Registration successful. User ID: {UserId}, Username: {Username}, Avatar: {Avatar}", 
                    response.Id, response.Username, response.Avatar);
                
                return Ok(new { message = "Registration successful", data = response });
            }
            catch (DuplicateEmailException dex)
            {
                _logger.LogWarning(dex, "Email already exists");
                return Conflict(new { message = dex.Message, errorCode = "DUPLICATE_EMAIL" });
            }
            catch (DuplicatePhoneNumberException dpex)
            {
                _logger.LogWarning(dpex, "Phone number already exists");
                return Conflict(new { message = dpex.Message, errorCode = "DUPLICATE_PHONE" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration with avatar");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
    }
}
