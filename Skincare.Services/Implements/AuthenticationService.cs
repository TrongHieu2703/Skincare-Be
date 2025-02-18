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
            if (user == null || user.PasswordHash != loginRequest.Password)
                return null; // Password hash check should be improved with hashing.

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
                PasswordHash = registerRequest.Password, // Hash password properly in production
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            await _accountRepository.AddAsync(newUser);
            return true;
        }

        private string GenerateJwtToken(Account user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim("userId", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
