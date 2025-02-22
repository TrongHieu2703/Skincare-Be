using Microsoft.EntityFrameworkCore;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Context;
using Skincare.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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
                return await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);
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
                _context.Accounts.Update(account);
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