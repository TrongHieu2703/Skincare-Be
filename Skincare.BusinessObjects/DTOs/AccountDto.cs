using System;

namespace Skincare.BusinessObjects.DTOs
{
    public class AccountDto
    {
        public int Id { get; set; }
        public required string Username { get; set; } = string.Empty;
        public required string Email { get; set; } = string.Empty;
        public required string Address { get; set; } = string.Empty;
        public required string Avatar { get; set; } = string.Empty;
        public required string PhoneNumber { get; set; } = string.Empty;
        public required string Status { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public required string Role { get; set; } = string.Empty;
    }
}

