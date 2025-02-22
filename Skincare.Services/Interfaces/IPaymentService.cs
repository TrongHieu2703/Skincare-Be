using System.Threading.Tasks;
using Skincare.BusinessObjects.DTOs;

namespace Skincare.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResult> ProcessPaymentAsync(PaymentRequest paymentRequest);
        Task<PaymentDetails> GetPaymentDetailsAsync(int id);
        Task<PaymentStatus> GetPaymentStatusAsync(int id);
    }
}