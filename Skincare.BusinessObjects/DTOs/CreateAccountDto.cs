using System.ComponentModel.DataAnnotations;

namespace Skincare.BusinessObjects.DTOs
{
    // DTO dành cho Admin (hoặc 1 luồng đặc biệt) tạo tài khoản
    public class CreateAccountDto
    {
        [Required, MinLength(3)]
        public required string Username { get; set; }

        [Required, EmailAddress]
        public required string Email { get; set; }

        [Required, MinLength(6)]
        public required string Password { get; set; }

        public required string PhoneNumber { get; set; }
        public required string Address { get; set; }
        public required string Avatar { get; set; }
    }
}
