namespace Skincare.BusinessObjects.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public string Role { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Avatar { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public DateTime Expiration { get; set; }
        public string Message { get; set; }
        // public string RefreshToken { get; set; } // nếu bạn muốn hỗ trợ Refresh Token
    }
}
