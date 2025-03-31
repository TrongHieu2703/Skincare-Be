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
        private readonly FileService _fileService;

        public AccountService(
            IAccountRepository accountRepository, 
            ILogger<AccountService> logger,
            FileService fileService)
        {
            _accountRepository = accountRepository;
            _logger = logger;
            _fileService = fileService;
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

                if (!string.IsNullOrEmpty(profileDto.Email) && profileDto.Email != account.Email)
                {
                    var existingUserWithEmail = await _accountRepository.GetByEmailAsync(profileDto.Email);
                    if (existingUserWithEmail != null && existingUserWithEmail.Id != userId)
                    {
                        throw new DuplicateEmailException($"Email {profileDto.Email} already exists.");
                    }
                }

                if (!string.IsNullOrEmpty(profileDto.PhoneNumber) && profileDto.PhoneNumber != account.PhoneNumber)
                {
                    var existingUserWithPhone = await _accountRepository.GetByPhoneNumberAsync(profileDto.PhoneNumber);
                    if (existingUserWithPhone != null && existingUserWithPhone.Id != userId)
                    {
                        throw new DuplicatePhoneNumberException($"Phone number {profileDto.PhoneNumber} already exists.");
                    }
                }

                account.Username = string.IsNullOrEmpty(profileDto.Username) ? account.Username : profileDto.Username;
                account.Email = string.IsNullOrEmpty(profileDto.Email) ? account.Email : profileDto.Email;
                account.Address = string.IsNullOrEmpty(profileDto.Address) ? account.Address : profileDto.Address;
                account.PhoneNumber = string.IsNullOrEmpty(profileDto.PhoneNumber) ? account.PhoneNumber : profileDto.PhoneNumber;
                
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

        public async Task<string?> UploadAvatarAsync(int userId, IFormFile? avatar)
        {
            try
            {
                _logger.LogInformation($"Uploading avatar for user ID: {userId}");
                
                var account = await _accountRepository.GetAccountByIdAsync(userId);
                if (account == null)
                {
                    throw new NotFoundException($"User with ID {userId} not found.");
                }

                if (avatar == null || avatar.Length == 0)
                {
                    // No avatar to upload, return null
                    return null;
                }

                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(avatar.ContentType.ToLower()))
                {
                    throw new ArgumentException("Invalid file type. Only jpg, jpeg, png and gif are allowed.");
                }

                if (!string.IsNullOrEmpty(account.Avatar))
                {
                    _fileService.DeleteFile(account.Avatar);
                }

                var fileUrl = await _fileService.SaveFileAsync(avatar, "avatar-images");
                
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
    }
}