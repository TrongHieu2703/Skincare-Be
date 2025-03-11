using System;
using System.Collections.Generic;

namespace Skincare.BusinessObjects.DTOs
{
    public class OrderDto
    {
        // Thông tin cơ bản về đơn hàng
        public int Id { get; set; }
        public string Status { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Thông tin giá cả
        public decimal TotalPrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsPrepaid { get; set; }
        
        // Thông tin voucher
        public VoucherInfoDto Voucher { get; set; }
        
        // Thông tin khách hàng (không bao gồm ID)
        public CustomerInfoDto CustomerInfo { get; set; }
        
        // Danh sách sản phẩm
        public List<OrderItemDetailDto> OrderItems { get; set; } = new List<OrderItemDetailDto>();
        
        // Thông tin thanh toán (không bao gồm ID)
        public PaymentInfoDto PaymentInfo { get; set; }
    }
    
    public class CustomerInfoDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
    }
    
    public class VoucherInfoDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
        public bool IsPercent { get; set; }
    }
    
    public class OrderItemDetailDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal ProductPrice { get; set; }
        public int ItemQuantity { get; set; }
    }
    
    public class PaymentInfoDto
    {
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
