using Microsoft.AspNetCore.Http;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using System.Threading.Tasks;

namespace Skincare.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> LoginAsync(LoginRequest loginRequest);
        Task<LoginResponse> RegisterAsync(RegisterRequest registerRequest);
        Task<LoginResponse> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeRefreshTokenAsync(int userId);
        string GenerateJwtToken(Account account);
        string GenerateRefreshToken();
        
        Task<string?> UploadAvatarForRegistration(IFormFile? avatar);
    }
}
