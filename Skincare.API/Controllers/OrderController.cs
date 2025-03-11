using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Exceptions; // import custom exceptions
using Skincare.Services.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Skincare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;
        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
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
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                    return Unauthorized(new { message = "Invalid user token" });
                var orders = await _orderService.GetOrdersByUserIdAsync(userId);
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
                return CreatedAtAction(nameof(GetOrderById), new { id = createdOrder.Id }, new { message = "Order created successfully", data = createdOrder });
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
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                    return Unauthorized(new { message = "Invalid user token" });
                    
                var order = await _orderService.GetOrderByUser(id, userId);
                return Ok(new { message = "Fetched order detail successfully", data = order });
            }
            catch (NotFoundException nfex)
            {
                _logger.LogWarning(nfex, $"Order detail not found with ID {id} for this user");
                return NotFound(new { message = nfex.Message, errorCode = "ORDER_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching detailed order with ID {id}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
    }
}
