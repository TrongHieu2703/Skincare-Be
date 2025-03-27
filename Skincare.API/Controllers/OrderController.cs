using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Exceptions; // import custom exceptions
using Skincare.Services.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace Skincare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderService orderService,
            ICartService cartService,
            ILogger<OrderController> logger)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                return Ok(new { message = "Fetched all orders successfully", data = orders });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all orders");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                return Ok(new { message = "Fetched order successfully", data = order });
            }
            catch (NotFoundException nfex)
            {
                _logger.LogWarning(nfex, $"Order not found with ID {id}");
                return NotFound(new { message = nfex.Message, errorCode = "ORDER_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching order with ID {id}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // Lấy lịch sử đơn hàng của user đang đăng nhập
        [HttpGet("user")]
        public async Task<IActionResult> GetOrdersByUser()
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation($"GetOrdersByUser called for user ID: {userIdStr}");
                
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                    return Unauthorized(new { message = "Invalid user token" });
                
                var orders = await _orderService.GetOrdersByUserIdAsync(userId);
                _logger.LogInformation($"Retrieved {orders.Count()} orders for user {userId}");
                
                // Log first order's items if any
                var firstOrder = orders.FirstOrDefault();
                if (firstOrder != null && firstOrder.OrderItems.Any())
                {
                    var firstItem = firstOrder.OrderItems.First();
                    _logger.LogInformation($"Sample order item: ProductId={firstItem.ProductId}, ProductName={firstItem.ProductName}, HasImage={(firstItem.ProductImage != null)}");
                }
                
                return Ok(new { message = "Fetched orders for user successfully", data = orders });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching orders for user");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            if (createOrderDto == null)
                return BadRequest(new { message = "Order data is null" });
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid request data", errors = ModelState });
            try
            {
                // Lấy userId từ token JWT
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation($"Creating order with user ID from token: {userIdStr}");
                
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    _logger.LogWarning("Invalid user token when creating order");
                    return Unauthorized(new { message = "Invalid user token" });
                }

                // Gán userId từ token vào createOrderDto
                createOrderDto.CustomerId = userId;
                _logger.LogInformation($"Setting CustomerId to {userId} from JWT token");

                var createdOrder = await _orderService.CreateOrderAsync(createOrderDto);
                
                // Sau khi đặt hàng thành công, xóa tất cả items trong giỏ hàng
                try
                {
                    await _cartService.ClearUserCartAsync(userId);
                    _logger.LogInformation($"Successfully cleared cart for user {userId} after order creation");
                }
                catch (Exception cartEx)
                {
                    // Log lỗi nhưng không làm ảnh hưởng đến việc đặt hàng thành công
                    _logger.LogWarning(cartEx, $"Failed to clear cart for user {userId} after order creation");
                }
                
                return CreatedAtAction(nameof(GetOrderById), new { id = createdOrder.Id }, 
                    new { message = "Order created successfully", data = createdOrder });
            }
            catch (NotFoundException nfex)
            {
                _logger.LogWarning(nfex, "Product not found during order creation");
                return BadRequest(new { 
                    message = nfex.Message,
                    errorCode = "PRODUCT_NOT_FOUND"
                });
            }
            catch (InvalidOperationException ioex)
            {
                _logger.LogWarning(ioex, "Inventory validation failed during order creation");
                return BadRequest(new { 
                    message = ioex.Message,
                    errorCode = "INSUFFICIENT_INVENTORY"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderDto updateOrderDto)
        {
            if (updateOrderDto == null)
                return BadRequest(new { message = "Update data is null" });
            try
            {
                var updatedOrder = await _orderService.UpdateOrderAsync(id, updateOrderDto);
                return Ok(new { message = "Order updated successfully", data = updatedOrder });
            }
            catch (NotFoundException nfex)
            {
                _logger.LogWarning(nfex, $"Order not found with ID {id}");
                return NotFound(new { message = nfex.Message, errorCode = "ORDER_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating order with ID {id}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                await _orderService.DeleteOrderAsync(id);
                return NoContent();
            }
            catch (NotFoundException nfex)
            {
                _logger.LogWarning(nfex, $"Order not found with ID {id}");
                return NotFound(new { message = nfex.Message, errorCode = "ORDER_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting order with ID {id}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // Lấy chi tiết đơn hàng an toàn (không lộ thông tin nhạy cảm)
        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            try
            {
                var orderDetail = await _orderService.GetOrderDetailAsync(id);
                return Ok(new { message = "Order detail retrieved successfully", data = orderDetail });
            }
            catch (NotFoundException nfex)
            {
                _logger.LogWarning(nfex, $"Order not found with ID {id}");
                return NotFound(new { message = nfex.Message, errorCode = "ORDER_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching order detail with ID {id}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
    }
}
