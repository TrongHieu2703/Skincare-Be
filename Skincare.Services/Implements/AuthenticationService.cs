using Skincare.BusinessObjects.DTOs;
using Skincare.Services.Interfaces;
using Skincare.Repositories.Interfaces;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;

        public AuthenticationService(IAccountRepository accountRepository, ILogger<AuthenticationService> logger, IConfiguration configuration)
        {
            _accountRepository = accountRepository;
            _logger = logger;
            _configuration = configuration;
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

                // ✅ Generate JWT token
                var token = GenerateJwtToken(account);

                _logger.LogInformation($"User logged in successfully with email: {loginRequest.Email}");

                return new LoginResponse
                {
                    Token = token,
                    Role = account.Role ?? "User",
                    Username = account.Username ?? string.Empty,
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
                    Role = "User",
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

        private string CreatePasswordHash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

        private string GenerateJwtToken(Account account)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new Claim(ClaimTypes.Name, account.Username),
                new Claim(ClaimTypes.Email, account.Email),
                new Claim(ClaimTypes.Role, account.Role ?? "User")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
