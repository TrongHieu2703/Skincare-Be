//using System;
//using System.ComponentModel.DataAnnotations;

//namespace Skincare.BusinessObjects.DTOs
//{
//    public class CreatePaymentDto
//    {
//        [Required]
//        public int OrderId { get; set; }

//        [Required]
//        [Range(1, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0")]
//        public decimal Amount { get; set; }

//        [Required]
//        public string PaymentMethod { get; set; } // Phương thức thanh toán (Ví dụ: "Credit Card", "PayPal", "Cash")
//    }
//}
