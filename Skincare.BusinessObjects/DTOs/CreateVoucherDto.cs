using System;
using System.ComponentModel.DataAnnotations;

namespace Skincare.BusinessObjects.DTOs
{
    public class CreateVoucherDto
    {
        [Required(ErrorMessage = "Voucher code is required")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Voucher code must be between 3 and 20 characters")]
        [RegularExpression(@"^[A-Z0-9_-]+$", ErrorMessage = "Voucher code can only contain uppercase letters, numbers, hyphens, and underscores")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Voucher name is required")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Voucher name must be between 5 and 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Voucher description is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Voucher description must be between 10 and 500 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Voucher value is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Voucher value cannot be negative")]
        public decimal Value { get; set; }

        [Required(ErrorMessage = "IsPercent flag is required")]
        public bool IsPercent { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartedAt { get; set; }

        [Required(ErrorMessage = "Expiry date is required")]
        public DateTime ExpiredAt { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Minimum order value cannot be negative")]
        public decimal? MinOrderValue { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Maximum discount cannot be negative")]
        public decimal? MaxDiscount { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Usage limit per user must be greater than 0")]
        public int? UsageLimitPerUser { get; set; }

        [StringLength(50, ErrorMessage = "Voucher type cannot exceed 50 characters")]
        [RegularExpression(@"^(discount|shipping|cashback|gift)$", ErrorMessage = "Invalid voucher type")]
        public string Type { get; set; } = "discount";

        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
        [RegularExpression(@"^(active|inactive|expired)$", ErrorMessage = "Status must be active, inactive, or expired")]
        public string Status { get; set; } = "active";

        public bool IsInfinity { get; set; } = false;
    }
}
