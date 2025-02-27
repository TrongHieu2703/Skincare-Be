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
        private readonly IEmailService _emailService;

        public AuthenticationService(IAccountRepository accountRepository, ILogger<AuthenticationService> logger, IConfiguration configuration, IEmailService emailService)
        {
            _accountRepository = accountRepository;
            _logger = logger;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest)
        {
            try
            {
                _logger.LogInformation($"Attempting login for: {loginRequest.Email}");
                var account = await _accountRepository.GetByEmailAsync(loginRequest.Email);

                if (account == null || string.IsNullOrEmpty(account.PasswordHash) || !VerifyPasswordHash(loginRequest.Password, account.PasswordHash))
                {
                    _logger.LogWarning("Invalid login attempt");
                    return null;
                }

                _logger.LogInformation($"DEBUG ACCOUNT DATA: ID: {account.Id}, Email: {account.Email}, Username: {account.Username}, " +
                    $"PasswordHash: {account.PasswordHash}, Role: {account.Role}, Status: {account.Status}, Address: {account.Address}, " +
                    $"PhoneNumber: {account.PhoneNumber}, CreatedAt: {account.CreatedAt}, RefreshToken: {account.RefreshToken}, RefreshTokenExpiry: {account.RefreshTokenExpiry}");

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

        public async Task<bool> RegisterAsync(RegisterRequest registerRequest)
        {
            try
            {
                var existingAccount = await _accountRepository.GetByEmailAsync(registerRequest.Email);
                if (existingAccount != null)
                {
                    _logger.LogWarning("Email already registered");
                    return false;
                }

                var newAccount = new Account
                {
                    Username = registerRequest.Username,
                    Email = registerRequest.Email,
                    PasswordHash = CreatePasswordHash(registerRequest.Password),
                    Role = "User",
                    CreatedAt = DateTime.UtcNow,
                    PhoneNumber = registerRequest.PhoneNumber,
                    Address = registerRequest.Address,
                    Avatar = registerRequest.Avatar,
                    Status = "active" 
                };

                await _accountRepository.CreateAccountAsync(newAccount);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed");
                throw;
            }
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var account = await _accountRepository.GetByEmailAsync(request.Email);
            if (account == null)
            {
                _logger.LogWarning("Email not found for password reset");
                return false;
            }

            var otp = GenerateOtp();
            account.OtpCode = otp;
            account.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
            await _accountRepository.UpdateAccountAsync(account);

            // TODO: Send OTP to email
            var emailSubject = "Your Password Reset OTP";
            var emailBody = $"Your OTP code is {otp}. It is valid for 10 minutes.";
            await _emailService.SendEmailAsync(account.Email, emailSubject, emailBody);

            _logger.LogInformation($"OTP sent to {request.Email}: {otp}");

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var account = await _accountRepository.GetByEmailAsync(request.Email);

            if (account == null || account.OtpCode != request.OtpCode || account.OtpExpiry < DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid OTP or email for password reset");
                return false;
            }

            account.PasswordHash = CreatePasswordHash(request.NewPassword);
            account.OtpCode = null;
            account.OtpExpiry = null;
            await _accountRepository.UpdateAccountAsync(account);

            return true;
        }

        private string CreatePasswordHash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

        private bool VerifyPasswordHash(string password, string storedHash) => BCrypt.Net.BCrypt.Verify(password, storedHash);

        private string GenerateJwtToken(Account account)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new Claim(ClaimTypes.Name, account.Username),
            new Claim(ClaimTypes.Email, account.Email),
            new Claim(ClaimTypes.Role, account.Role)
        };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        //private string GenerateRefreshToken()
        //{
        //    var randomBytes = new byte[64];
        //    using (var rng = RandomNumberGenerator.Create())
        //    {
        //        rng.GetBytes(randomBytes);
        //        return Convert.ToBase64String(randomBytes);
        //    }
        //}

        private string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}
