//using Microsoft.AspNetCore.Mvc;
//using Skincare.BusinessObjects.DTOs;
//using Skincare.Services.Interfaces;
//using System.Threading.Tasks;

//namespace Skincare.API.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class PaymentController : ControllerBase
//    {
//        private readonly IPaymentService _paymentService;
//        private readonly ILogger<PaymentController> _logger;

//        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
//        {
//            _paymentService = paymentService;
//            _logger = logger;
//        }

//        [HttpPost("process")]
//        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest paymentRequest)
//        {
//            if (paymentRequest == null)
//                return BadRequest("Payment request is null");

//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            try
//            {
//                var paymentResult = await _paymentService.ProcessPaymentAsync(paymentRequest);
//                if (!paymentResult.IsSuccess)
//                    return BadRequest(paymentResult.ErrorMessage);

//                return Ok(paymentResult);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error processing payment");
//                return StatusCode(500, "Internal server error");
//            }
//        }

//        [HttpGet("{id}")]
//        public async Task<IActionResult> GetPaymentDetails(int id)
//        {
//            try
//            {
//                var paymentDetails = await _paymentService.GetPaymentDetailsAsync(id);
//                if (paymentDetails == null)
//                    return NotFound();

//                return Ok(paymentDetails);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error fetching payment details for ID {id}");
//                return StatusCode(500, "Internal server error");
//            }
//        }

//        [HttpGet("status/{id}")]
//        public async Task<IActionResult> GetPaymentStatus(int id)
//        {
//            try
//            {
//                var paymentStatus = await _paymentService.GetPaymentStatusAsync(id);
//                if (paymentStatus == null)
//                    return NotFound();

//                return Ok(paymentStatus);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error fetching payment status for ID {id}");
//                return StatusCode(500, "Internal server error");
//            }
//        }
//    }
//}