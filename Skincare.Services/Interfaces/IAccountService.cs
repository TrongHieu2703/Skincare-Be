using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Services.Interfaces
{
    public interface IAccountService
    {
        Task<IEnumerable<Account>> GetAllAccountsAsync();
        Task<Account> GetAccountByIdAsync(int id);
        Task<Account> GetByEmailAsync(string email);
        Task<Account> CreateAccountAsync(Account account);
        Task DeleteAccountAsync(int id);
        Task<UProfileDTO> GetUserProfile(int id);

        Task UpdateProfileAsync(int userId, UProfileDTO profileDto);
        //Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto passwordDto);
    }
}
