using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Skincare.BusinessObjects.DTOs
{
    public class CreateOrderDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "CustomerId must be greater than 0")]
        public int CustomerId { get; set; }

        // Nếu voucher không bắt buộc, có thể nullable
        public int? VoucherId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "TotalPrice must be greater than 0")]
        public decimal TotalPrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public bool IsPrepaid { get; set; }
        
        [Required]
        public string Status { get; set; }

        [Required]
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
        [Required]
        public List<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
    }
}
