namespace Skincare.BusinessObjects.DTOs
{
    public class PaymentDetails
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string TransactionId { get; set; }
    }
}
