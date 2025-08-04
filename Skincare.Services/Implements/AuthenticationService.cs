using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.BusinessObjects.Exceptions;
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Skincare.Services.Implements
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly FileService _fileService;

        public AuthenticationService(
            IAccountRepository accountRepository,
            ILogger<AuthenticationService> logger,
            IConfiguration configuration,
            FileService fileService)
        {
            _accountRepository = accountRepository;
            _logger = logger;
            _configuration = configuration;
            _fileService = fileService;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest)
        {
            try
            {
                _logger.LogInformation("Login attempt for {Email}", loginRequest.Email);
                var account = await _accountRepository.GetByEmailAsync(loginRequest.Email);
                if (account == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, account.PasswordHash))
                {
                    _logger.LogWarning("Invalid login attempt for {Email}", loginRequest.Email);
                    return null;
                }

                var token = GenerateJwtToken(account);
                var refreshToken = GenerateRefreshToken();
                var refreshTokenExpiry = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"]));

                // Update account with refresh token
                account.RefreshToken = refreshToken;
                account.RefreshTokenExpiry = refreshTokenExpiry;
                await _accountRepository.UpdateAccountAsync(account);

                _logger.LogInformation("Login successful for {Email}, userId: {UserId}", loginRequest.Email, account.Id);

                return new LoginResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    Role = account.Role,
                    Username = account.Username,
                    Expiration = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"])),
                    RefreshTokenExpiration = refreshTokenExpiry,
                    Message = "Login successful",
                    Id = account.Id,
                    Email = account.Email,
                    Avatar = account.Avatar,
                    PhoneNumber = account.PhoneNumber,
                    Address = account.Address
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for {Email}", loginRequest.Email);
                throw;
            }
        }

        public async Task<LoginResponse> RegisterAsync(RegisterRequest registerRequest)
        {
            try
            {
                var existingAccount = await _accountRepository.GetByEmailAsync(registerRequest.Email);
                if (existingAccount != null)
                {
                    throw new DuplicateEmailException($"Email {registerRequest.Email} already exists.");
                }

                if (!string.IsNullOrEmpty(registerRequest.PhoneNumber))
                {
                    var existingPhoneAccount = await _accountRepository.GetByPhoneNumberAsync(registerRequest.PhoneNumber);
                    if (existingPhoneAccount != null)
                    {
                        throw new DuplicatePhoneNumberException($"Phone number {registerRequest.PhoneNumber} already exists.");
                    }
                }

                var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password);
                var refreshToken = GenerateRefreshToken();
                var refreshTokenExpiry = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"]));

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
                    Status = "active",
                    RefreshToken = refreshToken,
                    RefreshTokenExpiry = refreshTokenExpiry
                };

                await _accountRepository.CreateAccountAsync(newAccount);
                var token = GenerateJwtToken(newAccount);

                _logger.LogInformation("Registration successful for {Email}, userId: {UserId}", newAccount.Email, newAccount.Id);

                return new LoginResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    Role = newAccount.Role,
                    Username = newAccount.Username,
                    Expiration = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"])),
                    RefreshTokenExpiration = refreshTokenExpiry,
                    Message = "Registration successful",
                    Id = newAccount.Id,
                    Email = newAccount.Email,
                    Avatar = newAccount.Avatar,
                    PhoneNumber = newAccount.PhoneNumber,
                    Address = newAccount.Address
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for {Email}", registerRequest.Email);
                throw;
            }
        }

        public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var account = await _accountRepository.GetByRefreshTokenAsync(refreshToken);
                if (account == null || account.RefreshTokenExpiry <= DateTime.UtcNow)
                {
                    _logger.LogWarning("Invalid or expired refresh token");
                    return null;
                }

                var newToken = GenerateJwtToken(account);
                var newRefreshToken = GenerateRefreshToken();
                var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"]));

                // Update account with new refresh token
                account.RefreshToken = newRefreshToken;
                account.RefreshTokenExpiry = newRefreshTokenExpiry;
                await _accountRepository.UpdateAccountAsync(account);

                _logger.LogInformation("Token refreshed successfully for userId: {UserId}", account.Id);

                return new LoginResponse
                {
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    Role = account.Role,
                    Username = account.Username,
                    Expiration = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"])),
                    RefreshTokenExpiration = newRefreshTokenExpiry,
                    Message = "Token refreshed successfully",
                    Id = account.Id,
                    Email = account.Email,
                    Avatar = account.Avatar,
                    PhoneNumber = account.PhoneNumber,
                    Address = account.Address
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                throw;
            }
        }

        public async Task<bool> RevokeRefreshTokenAsync(int userId)
        {
            try
            {
                var account = await _accountRepository.GetByIdAsync(userId);
                if (account == null)
                {
                    return false;
                }

                account.RefreshToken = null;
                account.RefreshTokenExpiry = null;
                await _accountRepository.UpdateAccountAsync(account);

                _logger.LogInformation("Refresh token revoked for userId: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking refresh token for userId: {UserId}", userId);
                throw;
            }
        }

        public string GenerateJwtToken(Account account)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("UserId", account.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new Claim(ClaimTypes.Email, account.Email),
                new Claim(ClaimTypes.Name, account.Username),
                new Claim(ClaimTypes.Role, account.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<string?> UploadAvatarForRegistration(IFormFile? avatar)
        {
            try
            {
                if (avatar == null || avatar.Length == 0)
                {
                    // No avatar to upload, return null
                    return null;
                }

                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(avatar.ContentType.ToLower()))
                {
                    throw new ArgumentException("Invalid file type. Only jpg, jpeg, png and gif are allowed.");
                }

                var fileUrl = await _fileService.SaveFileAsync(avatar, "avatar-images");
                
                return fileUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading avatar for registration");
                throw;
            }
        }
    }
}