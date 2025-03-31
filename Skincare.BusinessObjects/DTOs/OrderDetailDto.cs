using System;
using System.Collections.Generic;

namespace Skincare.BusinessObjects.DTOs
{
    public class OrderDetailDto
    {
        // Order basic info
        public int Id { get; set; }
        public string Status { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Price information
        public decimal TotalPrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsPrepaid { get; set; }
        
        // Customer information
        public CustomerDetailDto Customer { get; set; }
        
        // Voucher information
        public VoucherDetailDto Voucher { get; set; }
        
        // Payment information
        public PaymentDetailDto Payment { get; set; }
        
        // Products
        public List<OrderItemDetailDto> Items { get; set; } = new List<OrderItemDetailDto>();
    }
    
    public class CustomerDetailDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
    }
    
    public class VoucherDetailDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
        public bool IsPercent { get; set; }
        public decimal? MinOrderValue { get; set; }
        public decimal? MaxDiscountValue { get; set; }
    }
    
    public class PaymentDetailDto
    {
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
} 