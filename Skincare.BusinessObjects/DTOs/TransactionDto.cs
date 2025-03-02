using System;

namespace Skincare.BusinessObjects.DTOs
{
    public class TransactionDto
    {
        // Không bắt buộc gửi TransactionId và OrderId từ client
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
