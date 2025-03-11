namespace Skincare.BusinessObjects.DTOs
{
    public class UProfileDTO
    {
        public required string Username { get; set; } = string.Empty;
        public required string Email { get; set; } = string.Empty;
        public required string Address { get; set; } = string.Empty;
        public required string Avatar { get; set; } = string.Empty;
        public required string PhoneNumber { get; set; } = string.Empty;
    }
}
