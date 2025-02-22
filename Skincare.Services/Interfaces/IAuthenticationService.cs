using System.Threading.Tasks;
using Skincare.BusinessObjects.DTOs;

namespace Skincare.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> LoginAsync(LoginRequest loginRequest);
        Task<bool> RegisterAsync(RegisterRequest registerRequest);
    }
}