//using Skincare.BusinessObjects.DTOs;
//using Skincare.Services.Interfaces;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//using Skincare.BusinessObjects.Entities;
//using Skincare.Repositories.Interfaces;

//namespace Skincare.Services.Implements
//{
//    public class PaymentService : IPaymentService
//    {
//        private readonly ILogger<PaymentService> _logger;
//        private readonly IPaymentRepository _paymentRepository;

//        public PaymentService(ILogger<PaymentService> logger, IPaymentRepository paymentRepository)
//        {
//            _logger = logger;
//            _paymentRepository = paymentRepository;
//        }

//        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest paymentRequest)
//        {
//            _logger.LogInformation("Processing payment...");

//            bool isSuccessful = paymentRequest.Amount > 0;
//            string status = isSuccessful ? "Success" : "Failed";

//            var payment = new Payment
//            {
//                OrderId = paymentRequest.OrderId,
//                Amount = paymentRequest.Amount,
//                PaymentMethod = paymentRequest.PaymentMethod,
//                PaymentDate = DateTime.UtcNow
//            };

//            // 👉 Lưu vào DB
//            await _paymentRepository.ProcessPaymentAsync(payment);

//            return new PaymentResult
//            {
//                IsSuccess = isSuccessful,
//                ErrorMessage = isSuccessful ? null : "Payment failed"
//            };
//        }


//        public async Task<PaymentDetails> GetPaymentDetailsAsync(int id)
//        {
//            return new PaymentDetails
//            {
//                PaymentId = id,
//                Amount = 100,
//                PaymentMethod = "Credit Card",
//                PaymentDate = System.DateTime.UtcNow
//            };
//        }

//        public async Task<PaymentStatus> GetPaymentStatusAsync(int id)
//        {
//            return new PaymentStatus
//            {
//                Status = "Completed"
//            };
//        }
//    }
//}
