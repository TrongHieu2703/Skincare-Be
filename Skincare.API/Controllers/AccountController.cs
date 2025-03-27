using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Exceptions; // import custom exceptions
using Skincare.Services.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Skincare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAccountService accountService, ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        // ============ API 1: Lấy thông tin user đăng nhập ============
        [HttpGet("user-profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                    return Unauthorized(new { message = "Invalid user token" });

                var profile = await _accountService.GetUserProfile(userId);
                
                // Log để debug
                _logger.LogInformation($"User profile fetched. Avatar URL: {profile.Avatar}");
                
                return Ok(new { message = "User profile fetched successfully", data = profile });
            }
            catch (NotFoundException nfex)
            {
                _logger.LogWarning(nfex, "User not found");
                return NotFound(new { message = nfex.Message, errorCode = "USER_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user profile");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // ============ API 2: Cập nhật thông tin user đăng nhập ============
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UProfileDTO profileDto)
        {
            if (profileDto == null)
                return BadRequest(new { message = "Profile data is null" });

            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out var userId))
                    return Unauthorized(new { message = "Invalid user token" });

                await _accountService.UpdateProfileAsync(userId, profileDto);
                return Ok(new { message = "Profile updated successfully" });
            }
            catch (NotFoundException nfex)
            {
                _logger.LogWarning(nfex, "User not found while updating profile");
                return NotFound(new { message = nfex.Message, errorCode = "USER_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // ============ API 3: Lấy toàn bộ tài khoản (chỉ Admin) ============
        [HttpGet]
        [Authorize(Roles = "Admin")] // chỉ admin mới có thể xem danh sách
        public async Task<IActionResult> GetAllAccounts()
        {
            try
            {
                var accounts = await _accountService.GetAllAccountsAsync();
                return Ok(new { message = "Accounts retrieved successfully", data = accounts });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all accounts");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // ============ API 4: Xóa tài khoản theo ID (chỉ Admin) ============
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // chỉ admin mới có thể xóa
        public async Task<IActionResult> DeleteAccount(int id)
        {
            try
            {
                await _accountService.DeleteAccountAsync(id);
                return Ok(new { message = $"Account with ID {id} deleted successfully" });
            }
            catch (NotFoundException nfex)
            {
                _logger.LogWarning(nfex, "Attempt to delete non-existing account");
                return NotFound(new { message = nfex.Message, errorCode = "ACCOUNT_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting account with ID {id}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // ============ API 5: Upload avatar ============
        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile avatar)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                    return Unauthorized(new { message = "Invalid user token" });

                var avatarUrl = await _accountService.UploadAvatarAsync(userId, avatar);
                return Ok(new { message = "Avatar uploaded successfully", avatarUrl });
            }
            catch (NotFoundException nfex)
            {
                _logger.LogWarning(nfex, "User not found while uploading avatar");
                return NotFound(new { message = nfex.Message, errorCode = "USER_NOT_FOUND" });
            }
            catch (ArgumentException aex)
            {
                _logger.LogWarning(aex, "Invalid file upload attempt");
                return BadRequest(new { message = aex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading avatar");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // ============ API 6: Cập nhật thông tin user đăng nhập với avatar ============
        [HttpPut("update-profile-with-avatar")]
        public async Task<IActionResult> UpdateProfileWithAvatar([FromForm] string username, [FromForm] string email,
            [FromForm] string address, [FromForm] string phoneNumber, IFormFile avatar)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out var userId))
                    return Unauthorized(new { message = "Invalid user token" });

                // Upload avatar nếu có
                string avatarUrl = null;
                if (avatar != null && avatar.Length > 0)
                {
                    avatarUrl = await _accountService.UploadAvatarAsync(userId, avatar);
                }

                // Tạo đối tượng profile để update
                var profileDto = new UProfileDTO
                {
                    Username = username,
                    Email = email,
                    Address = address,
                    PhoneNumber = phoneNumber,
                    Avatar = avatarUrl ?? "" // Nếu không có avatar mới, giữ nguyên avatar cũ
                };

                await _accountService.UpdateProfileAsync(userId, profileDto);
                return Ok(new { message = "Profile updated successfully", avatarUrl });
            }
            catch (NotFoundException nfex)
            {
                _logger.LogWarning(nfex, "User not found while updating profile");
                return NotFound(new { message = nfex.Message, errorCode = "USER_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile with avatar");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
    }
}
