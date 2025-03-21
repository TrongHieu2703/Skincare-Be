﻿using System;

namespace Skincare.BusinessObjects.DTOs
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
