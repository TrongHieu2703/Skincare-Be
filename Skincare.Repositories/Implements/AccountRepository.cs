using Microsoft.EntityFrameworkCore;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Context;
using Skincare.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;

namespace Skincare.Repositories.Implements
{
    public class AccountRepository : IAccountRepository
    {
        private readonly SWP391Context _context;
        private readonly ILogger<AccountRepository> _logger;

        public AccountRepository(SWP391Context context, ILogger<AccountRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Account>> GetAllAccountsAsync()
        {
            try
            {
                return await _context.Accounts.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all accounts");
                throw;
            }
        }

        public async Task<Account> GetAccountByIdAsync(int id)
        {
            try
            {
                return await _context.Accounts.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching account with ID {id}");
                throw;
            }
        }

        public async Task<Account> GetByEmailAsync(string email)
        {
            try
            {
                var account = await _context.Accounts
                    .AsNoTracking()
                    .Where(a => a.Email == email)
                    .Select(a => new Account
                    {
                        Id = a.Id,
                        Email = a.Email ?? string.Empty,  // ✅ Fix lỗi NULL
                        Username = a.Username ?? string.Empty,  // ✅ Fix lỗi NULL
                        PasswordHash = a.PasswordHash ?? string.Empty, // ✅ Fix lỗi NULL
                        Role = a.Role ?? "User", // ✅ Fix lỗi NULL
                        Address = a.Address ?? string.Empty, // ✅ Fix lỗi NULL
                        PhoneNumber = a.PhoneNumber ?? string.Empty, // ✅ Fix lỗi NULL
                        Avatar = a.Avatar ?? string.Empty, // ✅ Fix lỗi NULL
                        Status = a.Status ?? "active", // ✅ Fix lỗi NULL
                        CreatedAt = a.CreatedAt ?? DateTime.UtcNow, // ✅ Fix NULL thành giá trị mặc định
                    })
                    .FirstOrDefaultAsync();

                if (account == null)
                {
                    _logger.LogWarning($"No account found for email: {email}");
                    return null;
                }

                return account;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching account with email {email}");
                throw;
            }
        }




        public async Task<Account> CreateAccountAsync(Account account)
        {
            try
            {
                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();
                return account;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account");
                throw;
            }
        }

        public async Task UpdateAccountAsync(Account account)
        {
            try
            {
                var existingAccount = await _context.Accounts.FindAsync(account.Id);
                if (existingAccount == null)
                {
                    _logger.LogWarning($"Account with ID {account.Id} not found.");
                    return;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating account with ID {account.Id}");
                throw;
            }
        }




        public async Task DeleteAccountAsync(int id)
        {
            try
            {
                var account = await _context.Accounts.FindAsync(id);
                if (account != null)
                {
                    _context.Accounts.Remove(account);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting account with ID {id}");
                throw;
            }
        }
    }
}