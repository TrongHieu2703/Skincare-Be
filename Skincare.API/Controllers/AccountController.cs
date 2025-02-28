using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.Services.Interfaces;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Skincare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAccountService accountService, ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAccounts()
        {
            try
            {
                var accounts = await _accountService.GetAllAccountsAsync();
                return Ok(accounts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all accounts");
                return StatusCode(500, new { Message = "Internal server error", Error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccountById(int id)
        {
            try
            {
                var account = await _accountService.GetAccountByIdAsync(id);
                if (account == null)
                    return NotFound(new { Message = "Account not found" });

                return Ok(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching account with ID {id}");
                return StatusCode(500, new { Message = "Internal server error", Error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] Account account)
        {
            if (account == null)
                return BadRequest(new { Message = "Account is null" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdAccount = await _accountService.CreateAccountAsync(account);
                return CreatedAtAction(nameof(GetAccountById), new { id = createdAccount.Id }, createdAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account");
                return StatusCode(500, new { Message = "Internal server error", Error = ex.Message });
            }
        }

        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UProfileDTO profileDto)
        {
            if (profileDto == null)
                return BadRequest(new { Message = "Profile data is null" });

            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                await _accountService.UpdateProfileAsync(userId, profileDto);
                return Ok(new { Message = "Profile updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return StatusCode(500, new { Message = "Internal server error", Error = ex.Message });
            }
        }

        //[HttpPut("change-password")]
        //[Authorize]
        //public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto passwordDto)
        //{
        //    if (passwordDto == null)
        //        return BadRequest(new { Message = "Password data is null" });

        //    try
        //    {
        //        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        //        var result = await _accountService.ChangePasswordAsync(userId, passwordDto);
        //        if (!result)
        //            return BadRequest(new { Message = "Incorrect current password or validation failed" });

        //        return Ok(new { Message = "Password changed successfully" });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error changing password");
        //        return StatusCode(500, new { Message = "Internal server error", Error = ex.Message });
        //    }
        //}


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            try
            {
                await _accountService.DeleteAccountAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting account with ID {id}");
                return StatusCode(500, new { Message = "Internal server error", Error = ex.Message });
            }
        }

        [HttpGet("user-profile")]
        [Authorize]
        public async Task<IActionResult> GetAccountInfo()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    return Unauthorized(new { Message = "Unauthorized" });

                var userProfile = await _accountService.GetUserProfile(int.Parse(userId));
                if (userProfile == null)
                    return NotFound(new { Message = "User not found" });

                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching account info");
                return StatusCode(500, new { Message = "Internal server error", Error = ex.Message });
            }
        }
    }
}