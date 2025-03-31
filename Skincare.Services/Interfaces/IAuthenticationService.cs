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
        string GenerateJwtToken(Account account);
        
        Task<string?> UploadAvatarForRegistration(IFormFile? avatar);
    }
}
