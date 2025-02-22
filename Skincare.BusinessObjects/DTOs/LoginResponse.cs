namespace Skincare.BusinessObjects.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; }         // 🔑 JWT Access Token
        public string Role { get; set; }          // 🛡️ User Role
        public string Username { get; set; }      // 👤 Username for Navbar
        public DateTime Expiration { get; set; }  // ⏰ Token Expiration
        public string Message { get; set; }       // 📨 Custom message (e.g., "Login successful")
    }
}
