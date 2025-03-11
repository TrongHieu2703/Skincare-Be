using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Context;
using Skincare.Repositories.Interfaces;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Skincare.Repositories.Implements
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly SWP391Context _context;
        private readonly ILogger<AuthenticationRepository> _logger;

        public AuthenticationRepository(SWP391Context context, ILogger<AuthenticationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Account> GetByEmailAsync(string email)
        {
            try
            {
                return await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching account by email: {email}");
                throw;
            }
        }

        public async Task CreateAccountAsync(Account account)
        {
            try
            {
                await _context.Accounts.AddAsync(account);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account");
                throw;
            }
        }
    }
}