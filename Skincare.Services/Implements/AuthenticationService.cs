using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using BCrypt.Net;



namespace Skincare.Services.Implements
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IConfiguration _configuration;

        public AuthenticationService(IAccountRepository accountRepository, IConfiguration configuration)
        {
            _accountRepository = accountRepository;
            _configuration = configuration;
        }



        public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest)
        {
            var user = await _accountRepository.GetByEmailAsync(loginRequest.Email);
            if (user == null || !VerifyPassword(loginRequest.Password, user.PasswordHash))
                return null;

            var token = GenerateJwtToken(user);
            return new LoginResponse { Token = token, UserId = user.Id, Email = user.Email };
        }

        public async Task<bool> RegisterAsync(RegisterRequest registerRequest)
        {
            var existingUser = await _accountRepository.GetByEmailAsync(registerRequest.Email);
            if (existingUser != null) return false;

            var newUser = new Account
            {
                Username = registerRequest.Username,
                Email = registerRequest.Email,
                PasswordHash = HashPassword(registerRequest.Password),
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            await _accountRepository.CreateAccountAsync(newUser);
            return true;
        }

        private string GenerateJwtToken(Account user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),//nên để id chính là claim đầu tiên để xác thực
                new Claim(ClaimTypes.Email, user.Email),//claim thứ 2 thường là email hoặc username
                new Claim(ClaimTypes.Role, user.Role),//ở đây role cần thiết hơn username do đã có email rồi, nên để là claim thứ 3
                new Claim(ClaimTypes.Name, user.Username)
            };

            var token = new JwtSecurityToken(
                //_configuration["Jwt:Issuer"],
                //_configuration["Jwt:Audience"],
                claims : claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }



    }
}
