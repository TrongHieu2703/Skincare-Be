using System;
using System.Collections.Generic;

namespace Skincare.BusinessObjects.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int? VoucherId { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public bool IsPrepaid { get; set; }
        public string Status { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
        public List<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
    }
}
