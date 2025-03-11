//using Skincare.BusinessObjects.Entities;
//using Skincare.Repositories.Context;
//using Skincare.Repositories.Interfaces;
//using Microsoft.Extensions.Logging;
//using System.Threading.Tasks;


//namespace Skincare.Repositories.Implements
//{
//    public class PaymentRepository : IPaymentRepository
//    {
//        private readonly SWP391Context _context;
//        private readonly ILogger<PaymentRepository> _logger;

//        public PaymentRepository(SWP391Context context, ILogger<PaymentRepository> logger)
//        {
//            _context = context;
//            _logger = logger;
//        }

//        public async Task<Payment> ProcessPaymentAsync(Payment payment)
//        {
//            _context.Payments.Add(payment);
//            await _context.SaveChangesAsync();
//            return payment;
//        }

//        public async Task<Payment> GetPaymentByIdAsync(int id)
//        {
//            return await _context.Payments.FindAsync(id);
//        }

//    }
//}
