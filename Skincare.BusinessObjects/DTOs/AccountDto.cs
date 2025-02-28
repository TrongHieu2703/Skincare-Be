using System;

namespace Skincare.BusinessObjects.DTOs
{
    public class AccountDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Avatar { get; set; }
        public string PhoneNumber { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Role { get; set; }
    }
}
