using Skincare.BusinessObjects.DTOs;

public class AuthResponse
{
    public string Token { get; set; }
    public AccountDto Account { get; set; } // Thêm thông tin account đầy đủ
} 