using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
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
        private readonly IFileService _fileService;

        public AuthenticationService(
            IAccountRepository accountRepository,
            ILogger<AuthenticationService> logger,
            IConfiguration configuration,
            IFileService fileService)
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
                var account = await _accountRepository.GetByEmailAsync(loginRequest.Email);
                if (account == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, account.PasswordHash))
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
                // 1. Validate input
                if (string.IsNullOrEmpty(registerRequest.Email) || string.IsNullOrEmpty(registerRequest.Password))
                {
                    throw new ArgumentException("Email và mật khẩu không được để trống");
                }

                // 2. Kiểm tra email tồn tại
                var existingAccount = await _accountRepository.GetByEmailAsync(registerRequest.Email);
                if (existingAccount != null)
                {
                    throw new Exception("Email đã được sử dụng");
                }

                // 3. Xử lý avatar nếu có
                string avatarPath = null;
                if (!string.IsNullOrEmpty(registerRequest.Avatar))
                {
                    try
                    {
                        // Kiểm tra định dạng base64
                        if (!registerRequest.Avatar.Contains("base64,"))
                        {
                            throw new Exception("Định dạng ảnh không hợp lệ");
                        }

                        // Lưu ảnh và lấy đường dẫn
                        avatarPath = await _fileService.SaveImageAsync(registerRequest.Avatar);
                        
                        if (string.IsNullOrEmpty(avatarPath))
                        {
                            throw new Exception("Không thể lưu ảnh đại diện");
                        }

                        _logger.LogInformation($"Avatar saved successfully at: {avatarPath}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing avatar during registration");
                        throw new Exception($"Lỗi xử lý ảnh đại diện: {ex.Message}");
                    }
                }

                // 4. Tạo tài khoản mới
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password);
                var newAccount = new Account
                {
                    Username = registerRequest.Username?.Trim(),
                    Email = registerRequest.Email.Trim().ToLower(),
                    PasswordHash = passwordHash,
                    PhoneNumber = registerRequest.PhoneNumber?.Trim(),
                    Address = registerRequest.Address?.Trim(),
                    Avatar = avatarPath, // Lưu đường dẫn ảnh
                    Role = "User",
                    Status = "active",
                    CreatedAt = DateTime.UtcNow
                };

                // 5. Lưu vào database
                var createdAccount = await _accountRepository.CreateAccountAsync(newAccount);

                // 6. Tạo JWT token
                var token = GenerateJwtToken(createdAccount);

                // 7. Trả về response
                return new LoginResponse
                {
                    Token = token,
                    Role = createdAccount.Role,
                    Username = createdAccount.Username,
                    Expiration = DateTime.UtcNow.AddHours(2),
                    Message = "Đăng ký thành công",
                    Avatar = avatarPath, // Thêm đường dẫn avatar vào response
                    Email = createdAccount.Email,
                    PhoneNumber = createdAccount.PhoneNumber,
                    Address = createdAccount.Address
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed: {Message}", ex.Message);
                throw new Exception($"Đăng ký thất bại: {ex.Message}");
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
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}