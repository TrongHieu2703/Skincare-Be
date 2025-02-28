using Skincare.BusinessObjects.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Services.Interfaces
{
    public interface IAccountService
    {
        // Trả về DTO thay vì Entity
        Task<IEnumerable<AccountDto>> GetAllAccountsAsync();
        Task<AccountDto> GetAccountByIdAsync(int id);
        Task<AccountDto> GetByEmailAsync(string email);

        // Tạo tài khoản cho mục đích Admin (hoặc một luồng riêng)
        Task<AccountDto> CreateAccountAsync(CreateAccountDto createDto);

        // Xoá tài khoản
        Task DeleteAccountAsync(int id);

        // Lấy và cập nhật profile (UserProfile)
        Task<UProfileDTO> GetUserProfile(int id);
        Task UpdateProfileAsync(int userId, UProfileDTO profileDto);
    }
}
