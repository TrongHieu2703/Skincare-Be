using Skincare.BusinessObjects.DTOs;
using Skincare.Services.Interfaces;
using Skincare.Repositories.Interfaces;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text;
using Skincare.BusinessObjects.Entities;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;

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
                var account = await _accountRepository.GetByEmailAsync(loginRequest.Email);
                if (account == null || !VerifyPasswordHash(loginRequest.Password, account.PasswordHash))
                {
                    _logger.LogWarning("Invalid login attempt for {Email}", loginRequest.Email);
                    return null;
                }

                var token = GenerateJwtToken(account);
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
                _logger.LogError(ex, "Login failed");
                throw;
            }
        }

        public async Task<LoginResponse> RegisterAsync(RegisterRequest registerRequest)
        {
            try
            {
                if (await _accountRepository.GetByEmailAsync(registerRequest.Email) != null)
                {
                    _logger.LogWarning("Email already registered: {Email}", registerRequest.Email);
                    return null;
                }

                var newAccount = new Account
                {
                    Username = registerRequest.Username,
                    Email = registerRequest.Email,
                    PasswordHash = HashPassword(registerRequest.Password),
                    Role = "User",
                    CreatedAt = DateTime.UtcNow,
                    PhoneNumber = registerRequest.PhoneNumber,
                    Address = registerRequest.Address,
                    Avatar = registerRequest.Avatar,
                    Status = "active"
                };

                await _accountRepository.CreateAccountAsync(newAccount);
                var token = GenerateJwtToken(newAccount);

                return new LoginResponse
                {
                    Token = token,
                    Role = newAccount.Role,
                    Username = newAccount.Username,
                    Expiration = DateTime.UtcNow.AddHours(2),
                    Message = "Registration successful"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed");
                throw;
            }
        }

        private string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);

        private bool VerifyPasswordHash(string password, string storedHash) => BCrypt.Net.BCrypt.Verify(password, storedHash);

        public string GenerateJwtToken(Account account)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new Claim(ClaimTypes.Email, account.Email),
                new Claim(ClaimTypes.Name, account.Username),
                new Claim(ClaimTypes.Role, account.Role)
            };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}