namespace Skincare.BusinessObjects.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public string Role { get; set; }
        public string Username { get; set; }
        public DateTime Expiration { get; set; }
        public string Message { get; set; }
        // public string RefreshToken { get; set; } // nếu bạn muốn hỗ trợ Refresh Token
    }
}
