using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Context;
using Skincare.Repositories.Interfaces;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Skincare.Repositories.Implements
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly SWP391Context _context;

        public AuthenticationRepository(SWP391Context context)
        {
            _context = context;
        }

        public async Task<Account> GetByEmailAsync(string email)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);
        }

        public async Task CreateAccountAsync(Account account)
        {
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();
        }
    }
}