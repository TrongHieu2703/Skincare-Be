using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.BusinessObjects.Exceptions; // <-- import custom exceptions
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Skincare.Services.Implements
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IConfiguration _configuration;

        public AuthenticationService(
            IAccountRepository accountRepository,
            ILogger<AuthenticationService> logger,
            IConfiguration configuration)
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
                // Giữ nguyên logic trả về null -> Unauthorized
                if (account == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, account.PasswordHash))
                {
                    _logger.LogWarning("Invalid login attempt for {Email}", loginRequest.Email);
                    return null; // Controller sẽ trả 401
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
                // Kiểm tra email tồn tại
                var existingAccount = await _accountRepository.GetByEmailAsync(registerRequest.Email);
                if (existingAccount != null)
                {
                    // Thay vì return null, quăng DuplicateEmailException
                    throw new DuplicateEmailException($"Email {registerRequest.Email} already exists.");
                }

                var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password);

                var newAccount = new Account
                {
                    Username = registerRequest.Username,
                    Email = registerRequest.Email,
                    PasswordHash = passwordHash,
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
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
