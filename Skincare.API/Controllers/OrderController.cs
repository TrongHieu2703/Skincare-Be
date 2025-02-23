using Microsoft.AspNetCore.Mvc;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all orders");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                    return NotFound();

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching order with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            if (createOrderDto == null)
                return BadRequest("Order data is null");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Mapping DTO to Entity
                var newOrder = new Order
                {
                    CustomerId = createOrderDto.CustomerId,
                    VoucherId = createOrderDto.VoucherId,
                    TotalPrice = createOrderDto.TotalPrice,
                    DiscountPrice = createOrderDto.DiscountPrice,
                    TotalAmount = createOrderDto.TotalAmount,
                    IsPrepaid = createOrderDto.IsPrepaid,
                    Status = createOrderDto.Status,
                    UpdatedAt = DateTime.Now,
                    OrderItems = createOrderDto.OrderItems.Select(item => new OrderItem
                    {
                        ProductId = item.ProductId,
                        ItemQuantity = item.ItemQuantity
                    }).ToList(),
                    Transactions = createOrderDto.Transactions.Select(tx => new Transaction
                    {
                        PaymentMethod = tx.PaymentMethod,
                        Status = tx.Status,
                        Amount = tx.Amount,
                        CreatedDate = tx.CreatedDate ?? DateTime.Now
                    }).ToList()
                };

                // Gọi service để lưu đơn hàng
                var createdOrder = await _orderService.CreateOrderAsync(newOrder);

                return CreatedAtAction(nameof(GetOrderById), new { id = createdOrder.Id }, createdOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderDto updateOrderDto)
        {
            if (updateOrderDto == null)
                return BadRequest("Update data is null");

            try
            {
                var existingOrder = await _orderService.GetOrderByIdAsync(id);
                if (existingOrder == null)
                    return NotFound($"Order with ID {id} not found");

                // Cập nhật thông tin đơn hàng
                existingOrder.VoucherId = updateOrderDto.VoucherId ?? existingOrder.VoucherId;
                existingOrder.Status = updateOrderDto.Status ?? existingOrder.Status;
                existingOrder.TotalPrice = updateOrderDto.TotalPrice ?? existingOrder.TotalPrice;
                existingOrder.DiscountPrice = updateOrderDto.DiscountPrice ?? existingOrder.DiscountPrice;
                existingOrder.TotalAmount = updateOrderDto.TotalAmount ?? existingOrder.TotalAmount;
                existingOrder.IsPrepaid = updateOrderDto.IsPrepaid ?? existingOrder.IsPrepaid;
                existingOrder.UpdatedAt = DateTime.UtcNow;

                // Cập nhật OrderItems nếu có
                if (updateOrderDto.OrderItems != null)
                {
                    existingOrder.OrderItems.Clear();
                    foreach (var item in updateOrderDto.OrderItems)
                    {
                        existingOrder.OrderItems.Add(new OrderItem
                        {
                            ProductId = item.ProductId,
                            ItemQuantity = item.ItemQuantity
                        });
                    }
                }

                // Cập nhật Transactions nếu có
                if (updateOrderDto.Transactions != null)
                {
                    existingOrder.Transactions.Clear();
                    foreach (var transaction in updateOrderDto.Transactions)
                    {
                        existingOrder.Transactions.Add(new Transaction
                        {
                            PaymentMethod = transaction.PaymentMethod,
                            Status = transaction.Status,
                            Amount = transaction.Amount,
                            CreatedDate = transaction.CreatedDate ?? DateTime.UtcNow
                        });
                    }
                }

                await _orderService.UpdateOrderAsync(existingOrder);
                return Ok(existingOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating order with ID {id}");
                return StatusCode(500, "Internal server error");
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
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting order with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}