using Skincare.BusinessObjects.Entities;
using Skincare.Services.Interfaces;
using Skincare.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Skincare.BusinessObjects.DTOs;
using Microsoft.Extensions.Logging;
using System;

namespace Skincare.Services.Implements
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<AccountService> _logger;

        public AccountService(IAccountRepository accountRepository, ILogger<AccountService> logger)
        {
            _accountRepository = accountRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Account>> GetAllAccountsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all accounts.");
                return await _accountRepository.GetAllAccountsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all accounts.");
                throw;
            }
        }

        public async Task<Account> GetAccountByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching account with ID: {id}");
                return await _accountRepository.GetAccountByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching account with ID: {id}");
                throw;
            }
        }

        public async Task<Account> GetByEmailAsync(string email)
        {
            try
            {
                _logger.LogInformation($"Fetching account with email: {email}");
                return await _accountRepository.GetByEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching account with email: {email}");
                throw;
            }
        }

        public async Task<Account> CreateAccountAsync(Account account)
        {
            try
            {
                _logger.LogInformation($"Creating account for email: {account.Email}");

                var newAccount = new Account
                {
                    Username = account.Username,
                    Email = account.Email,
                    PasswordHash = account.PasswordHash,
                    Role = "User", // ✅ Role mặc định
                    Address = null, // Để null cho người dùng tự cập nhật sau
                    Avatar = null,
                    PhoneNumber = null,
                    CreatedAt = DateTime.UtcNow
                };

                return await _accountRepository.CreateAccountAsync(newAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while creating account for email: {account.Email}");
                throw;
            }
        }


        public async Task UpdateProfileAsync(int userId, UProfileDTO profileDto)
        {
            try
            {
                var account = await _accountRepository.GetAccountByIdAsync(userId);
                if (account == null)
                    throw new Exception($"User ID {userId} not found");

                account.Username = profileDto.Username;
                account.Email = profileDto.Email;
                account.Address = profileDto.Address;
                account.Avatar = profileDto.Avatar;
                account.PhoneNumber = profileDto.PhoneNumber;

                await _accountRepository.UpdateAccountAsync(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating profile for User ID {userId}");
                throw;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto passwordDto)
        {
            try
            {
                var account = await _accountRepository.GetAccountByIdAsync(userId);
                if (account == null)
                    throw new Exception($"User ID {userId} not found");

                // Kiểm tra mật khẩu hiện tại
                if (!BCrypt.Net.BCrypt.Verify(passwordDto.CurrentPassword, account.PasswordHash))
                    return false;

                // Cập nhật mật khẩu mới
                account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordDto.NewPassword);
                await _accountRepository.UpdateAccountAsync(account);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error changing password for User ID {userId}");
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
                    _logger.LogWarning($"User profile not found for account ID: {id}");
                    return null;
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
    }
}