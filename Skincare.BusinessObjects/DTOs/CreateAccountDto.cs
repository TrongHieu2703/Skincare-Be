using System.ComponentModel.DataAnnotations;

namespace Skincare.BusinessObjects.DTOs
{
    // DTO dành cho Admin (hoặc 1 luồng đặc biệt) tạo tài khoản
    public class CreateAccountDto
    {
        [Required, MinLength(3)]
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Avatar { get; set; }
    }
}
