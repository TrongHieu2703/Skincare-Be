using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.BusinessObjects.Exceptions; 
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Skincare.Services.Implements
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<AccountService> _logger;
        private readonly GoogleDriveService _googleDriveService;

        public AccountService(
            IAccountRepository accountRepository, 
            ILogger<AccountService> logger,
            GoogleDriveService googleDriveService)
        {
            _accountRepository = accountRepository;
            _logger = logger;
            _googleDriveService = googleDriveService;
        }

        public async Task<IEnumerable<AccountDto>> GetAllAccountsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all accounts.");
                var accounts = await _accountRepository.GetAllAccountsAsync();

                return accounts.Select(a => new AccountDto
                {
                    Id = a.Id,
                    Username = a.Username,
                    Email = a.Email,
                    Address = a.Address,
                    Avatar = a.Avatar,
                    PhoneNumber = a.PhoneNumber,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt,
                    Role = a.Role
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all accounts.");
                throw;
            }
        }

        public async Task<AccountDto> GetAccountByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching account with ID: {id}");
                var account = await _accountRepository.GetAccountByIdAsync(id);
                if (account == null)
                {
                    // Quăng NotFoundException thay vì return null
                    throw new NotFoundException($"Account with ID {id} not found.");
                }

                return new AccountDto
                {
                    Id = account.Id,
                    Username = account.Username,
                    Email = account.Email,
                    Address = account.Address,
                    Avatar = account.Avatar,
                    PhoneNumber = account.PhoneNumber,
                    Status = account.Status,
                    CreatedAt = account.CreatedAt,
                    Role = account.Role
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching account with ID: {id}");
                throw;
            }
        }

        public async Task<AccountDto> GetByEmailAsync(string email)
        {
            try
            {
                _logger.LogInformation($"Fetching account with email: {email}");
                var account = await _accountRepository.GetByEmailAsync(email);
                if (account == null)
                {
                    // Quăng NotFoundException nếu muốn
                    throw new NotFoundException($"Account with email {email} not found.");
                }

                return new AccountDto
                {
                    Id = account.Id,
                    Username = account.Username,
                    Email = account.Email,
                    Address = account.Address,
                    Avatar = account.Avatar,
                    PhoneNumber = account.PhoneNumber,
                    Status = account.Status,
                    CreatedAt = account.CreatedAt,
                    Role = account.Role
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching account with email: {email}");
                throw;
            }
        }

        public async Task<AccountDto> CreateAccountAsync(CreateAccountDto createDto)
        {
            try
            {
                _logger.LogInformation($"Creating account for email: {createDto.Email}");
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(createDto.Password);

                var newAccount = new Account
                {
                    Username = createDto.Username,
                    Email = createDto.Email,
                    PasswordHash = passwordHash,
                    Role = "User",
                    Address = createDto.Address,
                    Avatar = createDto.Avatar,
                    PhoneNumber = createDto.PhoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    Status = "active"
                };

                var createdAccount = await _accountRepository.CreateAccountAsync(newAccount);

                return new AccountDto
                {
                    Id = createdAccount.Id,
                    Username = createdAccount.Username,
                    Email = createdAccount.Email,
                    Address = createdAccount.Address,
                    Avatar = createdAccount.Avatar,
                    PhoneNumber = createdAccount.PhoneNumber,
                    Status = createdAccount.Status,
                    CreatedAt = createdAccount.CreatedAt,
                    Role = createdAccount.Role
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while creating account for email: {createDto.Email}");
                throw;
            }
        }

        public async Task UpdateProfileAsync(int userId, UProfileDTO profileDto)
        {
            try
            {
                var account = await _accountRepository.GetAccountByIdAsync(userId);
                if (account == null)
                {
                    throw new NotFoundException($"User ID {userId} not found.");
                }

                account.Username = string.IsNullOrEmpty(profileDto.Username) ? account.Username : profileDto.Username;
                account.Email = string.IsNullOrEmpty(profileDto.Email) ? account.Email : profileDto.Email;
                account.Address = string.IsNullOrEmpty(profileDto.Address) ? account.Address : profileDto.Address;
                account.PhoneNumber = string.IsNullOrEmpty(profileDto.PhoneNumber) ? account.PhoneNumber : profileDto.PhoneNumber;
                
                // Chỉ cập nhật avatar nếu có giá trị mới
                if (!string.IsNullOrEmpty(profileDto.Avatar))
                {
                    account.Avatar = profileDto.Avatar;
                }

                await _accountRepository.UpdateAccountAsync(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating profile for User ID {userId}");
                throw;
            }
        }

        public async Task DeleteAccountAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting account with ID: {id}");
                await _accountRepository.DeleteAccountAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting account with ID: {id}");
                throw;
            }
        }

        public async Task<UProfileDTO> GetUserProfile(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching user profile for account ID: {id}");
                var account = await _accountRepository.GetAccountByIdAsync(id);
                if (account == null)
                {
                    // Quăng NotFoundException
                    throw new NotFoundException($"User profile not found for account ID {id}.");
                }

                return new UProfileDTO
                {
                    Username = account.Username,
                    Email = account.Email,
                    Address = account.Address,
                    Avatar = account.Avatar,
                    PhoneNumber = account.PhoneNumber
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching user profile for account ID: {id}");
                throw;
            }
        }

        public async Task<string> UploadAvatarAsync(int userId, IFormFile avatar)
        {
            try
            {
                _logger.LogInformation($"Uploading avatar for user ID: {userId}");
                
                var account = await _accountRepository.GetAccountByIdAsync(userId);
                if (account == null)
                {
                    throw new NotFoundException($"User with ID {userId} not found.");
                }

                // Validate file
                if (avatar == null || avatar.Length == 0)
                {
                    throw new ArgumentException("No avatar file uploaded");
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(avatar.ContentType.ToLower()))
                {
                    throw new ArgumentException("Invalid file type. Only jpg, jpeg, png and gif are allowed.");
                }

                // Delete old avatar if it exists
                if (!string.IsNullOrEmpty(account.Avatar) && account.Avatar.Contains("drive.google.com"))
                {
                    await DeleteOldAvatar(account.Avatar);
                }

                // Upload to Google Drive
                var (fileId, _, _, fileUrl) = await _googleDriveService.UploadFile(avatar);
                
                // Update account with new avatar URL
                account.Avatar = fileUrl;
                await _accountRepository.UpdateAccountAsync(account);
                
                return fileUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading avatar for user ID: {userId}");
                throw;
            }
        }

        private async Task DeleteOldAvatar(string avatarUrl)
        {
            try
            {
                // Extract file ID from the URL
                var fileId = string.Empty;
                
                if (avatarUrl.Contains("uc?id="))
                {
                    fileId = avatarUrl.Split("uc?id=")[1].Split("&")[0];
                }
                else if (avatarUrl.Contains("/d/"))
                {
                    fileId = avatarUrl.Split("/d/")[1].Split("/")[0];
                }

                if (!string.IsNullOrEmpty(fileId))
                {
                    await _googleDriveService.DeleteFile(fileId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to delete old avatar: {ex.Message}");
                // Just log the error but continue
            }
        }
    }
}
