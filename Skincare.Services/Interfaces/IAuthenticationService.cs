using Skincare.BusinessObjects.DTOs;
using System.Threading.Tasks;

namespace Skincare.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> LoginAsync(LoginRequest loginRequest);
        Task<bool> RegisterAsync(RegisterRequest registerRequest);
    }
}
