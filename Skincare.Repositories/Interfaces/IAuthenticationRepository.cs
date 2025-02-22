using System.Threading.Tasks;
using Skincare.BusinessObjects.Entities;

namespace Skincare.Repositories.Interfaces
{
    public interface IAuthenticationRepository
    {
        Task<Account> GetByEmailAsync(string email);
        Task CreateAccountAsync(Account account);
    }
}