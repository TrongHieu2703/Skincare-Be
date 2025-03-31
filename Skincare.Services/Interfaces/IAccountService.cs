using Skincare.BusinessObjects.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Skincare.Services.Interfaces
{
    public interface IAccountService
    {
        Task<IEnumerable<AccountDto>> GetAllAccountsAsync();
        Task<AccountDto> GetAccountByIdAsync(int id);
        Task<AccountDto> GetByEmailAsync(string email);
        Task<AccountDto> CreateAccountAsync(CreateAccountDto createDto);
        Task DeleteAccountAsync(int id);
        Task<UProfileDTO> GetUserProfile(int id);
        Task UpdateProfileAsync(int userId, UProfileDTO profileDto);
        Task<string?> UploadAvatarAsync(int userId, IFormFile? avatar);
    }
}
