using Skincare.BusinessObjects.DTOs;
using Skincare.Services.Interfaces;
using Skincare.Repositories.Interfaces;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using Skincare.BusinessObjects.Entities;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Skincare.Services.Implements
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<AuthenticationService> _logger;

        // Secret Key dùng để ký JWT (nên lưu trong file cấu hình)
        private readonly string _jwtSecret = "your_secret_key_here"; // ✅ Replace with real key

        public AuthenticationService(IAccountRepository accountRepository, ILogger<AuthenticationService> logger)
        {
            _accountRepository = accountRepository;
            _logger = logger;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest)
        {
            try
            {
                _logger.LogInformation($"Attempting to log in user with email: {loginRequest.Email}");
                var account = await _accountRepository.GetByEmailAsync(loginRequest.Email);

                if (account == null || !VerifyPasswordHash(loginRequest.Password, account.PasswordHash))
                {
                    _logger.LogWarning($"Login failed for user with email: {loginRequest.Email}");
                    return null;
                }

                // Gán giá trị mặc định cho các trường có thể null
                account.Username = account.Username ?? string.Empty;
                account.Address = account.Address ?? string.Empty;
                account.Avatar = account.Avatar ?? string.Empty;
                account.PhoneNumber = account.PhoneNumber ?? string.Empty;
                account.Status = account.Status ?? string.Empty;
                account.Role = account.Role ?? "User";

                // ✅ Generate JWT token
                var token = GenerateJwtToken(account);

                _logger.LogInformation($"User logged in successfully with email: {loginRequest.Email}");

                return new LoginResponse
                {
                    Token = token,
                    Role = account.Role,
                    Username = account.Username,
                    Expiration = DateTime.UtcNow.AddHours(2),
                    Message = "Login successful"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while logging in user with email: {loginRequest.Email}");
                throw;
            }
        }

        public async Task<bool> RegisterAsync(RegisterRequest registerRequest)
        {
            try
            {
                _logger.LogInformation($"Attempting to register user with email: {registerRequest.Email}");

                var existingAccount = await _accountRepository.GetByEmailAsync(registerRequest.Email);
                if (existingAccount != null)
                {
                    _logger.LogWarning($"Registration failed. User with email: {registerRequest.Email} already exists.");
                    return false;
                }

                var account = new Account
                {
                    Username = registerRequest.Username,
                    Email = registerRequest.Email,
                    PasswordHash = CreatePasswordHash(registerRequest.Password),
                    Role = "User", // ✅ Default role
                    CreatedAt = DateTime.UtcNow
                };

                await _accountRepository.CreateAccountAsync(account);
                _logger.LogInformation($"User registered successfully with email: {registerRequest.Email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while registering user with email: {registerRequest.Email}");
                throw;
            }
        }

        // ✅ Hash Password using BCrypt
        private string CreatePasswordHash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // ✅ Verify Password using BCrypt
        private bool VerifyPasswordHash(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

        // ✅ JWT Token Generation (Updated)
        private string GenerateJwtToken(Account account)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("mjRqNXAsz/VWrs8FrPpHiLf4byuGEkBdv0WohkODuv4="
);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()), // 🆔 User ID
                new Claim(ClaimTypes.Name, account.Username),               // 👤 Username
                new Claim(ClaimTypes.Email, account.Email),                 // 📧 Email
                new Claim(ClaimTypes.Role, account.Role)                    // 🛡️ Role
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2), // ⏰ Token valid for 2 hours
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
