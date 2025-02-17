using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skincare.BusinessObjects.Entities;

namespace Skincare.Repositories.Interfaces
{
    public interface IAccountRepository
    {
        Task<Account?> GetByEmailAsync(string email);
        Task AddAccountAsync(Account account);
        Task SaveChangesAsync();
    }
}
