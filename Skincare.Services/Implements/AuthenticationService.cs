using Skincare.BusinessObjects.DTOs;
using Skincare.Services.Interfaces;
using Skincare.Repositories.Interfaces;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using Skincare.BusinessObjects.Entities;
using System;

namespace Skincare.Services.Implements
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<AuthenticationService> _logger;

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

                // Generate JWT token (implementation not shown)
                var token = GenerateJwtToken(account);

                _logger.LogInformation($"User logged in successfully with email: {loginRequest.Email}");
                return new LoginResponse
                {
                    Token = token,
                    Role = account.Role
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
                    PasswordHash = CreatePasswordHash(registerRequest.Password)
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
            using (var hmac = new HMACSHA512())
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hash);
            }
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            using (var hmac = new HMACSHA512())
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hash) == storedHash;
            }
        }

        private string GenerateJwtToken(Account account)
        {
            // Implementation for generating JWT token (not shown)
            return "generated-jwt-token";
        }
    }
}