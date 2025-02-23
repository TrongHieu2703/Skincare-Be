using System;

namespace Skincare.BusinessObjects.DTOs
{
    public class TransactionDto
    {
        public string PaymentMethod { get; set; }   // CreditCard, PayPal, etc.
        public string Status { get; set; }         // Completed, Pending, etc.
        public decimal Amount { get; set; }
        public DateTime? CreatedDate { get; set; }  // Optional, default to DateTime.Now if null
    }
}
