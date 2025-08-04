using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Skincare.BusinessObjects.DTOs
{
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "Customer ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid customer ID")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Order items are required")]
        [MinLength(1, ErrorMessage = "Order must contain at least one item")]
        public List<CreateOrderItemDto> OrderItems { get; set; } = new List<CreateOrderItemDto>();

        [Required(ErrorMessage = "Shipping address is required")]
        [StringLength(200, MinimumLength = 10, ErrorMessage = "Shipping address must be between 10 and 200 characters")]
        public string ShippingAddress { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(15, MinimumLength = 10, ErrorMessage = "Phone number must be between 10 and 15 characters")]
        public string PhoneNumber { get; set; }

        [StringLength(20, ErrorMessage = "Payment method cannot exceed 20 characters")]
        [RegularExpression(@"^(cash|cod|bank|momo|vnpay|paypal)$", ErrorMessage = "Invalid payment method")]
        public string PaymentMethod { get; set; } = "cod";

        [StringLength(50, ErrorMessage = "Voucher code cannot exceed 50 characters")]
        [RegularExpression(@"^[A-Z0-9_-]*$", ErrorMessage = "Voucher code can only contain uppercase letters, numbers, hyphens, and underscores")]
        public string? VoucherCode { get; set; }

        [StringLength(500, ErrorMessage = "Order notes cannot exceed 500 characters")]
        public string? OrderNotes { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Shipping fee cannot be negative")]
        public decimal? ShippingFee { get; set; }

        public bool IsPrepaid { get; set; } = false;
    }

    public class CreateOrderItemDto
    {
        [Required(ErrorMessage = "Product ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid product ID")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Unit price cannot be negative")]
        public decimal? UnitPrice { get; set; }
    }
}
