using System;
using System.Collections.Generic;

namespace Skincare.BusinessObjects.DTOs
{
    public class CreateOrderDto
    {
        public int CustomerId { get; set; }        // Sử dụng AccountId nếu đã dùng Account Entity
        public int? VoucherId { get; set; }        // Optional
        public bool IsPrepaid { get; set; }
        public string Status { get; set; }         // Pending, Shipped, Delivered, etc.
        public decimal TotalPrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public decimal TotalAmount { get; set; }   // TotalPrice - DiscountPrice
        public List<OrderItemDto> OrderItems { get; set; }
        public List<TransactionDto> Transactions { get; set; }
    }
}
