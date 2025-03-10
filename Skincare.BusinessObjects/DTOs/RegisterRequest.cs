using System.ComponentModel.DataAnnotations;

namespace Skincare.BusinessObjects.DTOs
{
    // DTO dành cho người dùng tự đăng ký
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [MinLength(3, ErrorMessage = "Tên đăng nhập phải có ít nhất 3 ký tự")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; }

        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Avatar { get; set; } // Đổi thành string để nhận URL ảnh
    }
}
