using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.BusinessObjects.Exceptions; // import custom exceptions
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skincare.Services.Implements
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderService> _logger;
        public OrderService(IOrderRepository orderRepository, ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            try
            {
                var orders = await _orderRepository.GetAllOrdersAsync();
                return orders.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllOrdersAsync");
                throw;
            }
        }

        public async Task<OrderDto> GetOrderByIdAsync(int id)
        {
            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(id);
                if (order == null)
                    throw new NotFoundException($"Order with ID {id} not found.");
                return MapToDto(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetOrderByIdAsync for ID {id}");
                throw;
            }
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId)
        {
            try
            {
                var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
                return orders.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetOrdersByUserIdAsync for user ID {userId}");
                throw;
            }
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto orderDto)
        {
            try
            {
                if (orderDto.CustomerId <= 0)
                    throw new ArgumentException("CustomerId must be greater than 0");

                // Nếu VoucherId không hợp lệ, đặt về null
                if (orderDto.VoucherId.HasValue && orderDto.VoucherId.Value <= 0)
                    orderDto.VoucherId = null;

                decimal totalAmount = orderDto.TotalPrice - (orderDto.DiscountPrice ?? 0);

                var order = new Order
                {
                    CustomerId = orderDto.CustomerId,
                    VoucherId = orderDto.VoucherId,
                    TotalPrice = orderDto.TotalPrice,
                    DiscountPrice = orderDto.DiscountPrice,
                    TotalAmount = totalAmount,
                    IsPrepaid = orderDto.IsPrepaid,
                    Status = string.IsNullOrWhiteSpace(orderDto.Status) ? "Pending" : orderDto.Status,
                    UpdatedAt = DateTime.UtcNow,
                    OrderItems = orderDto.OrderItems.Select(item => new OrderItem
                    {
                        ProductId = item.ProductId,
                        ItemQuantity = item.ItemQuantity
                    }).ToList(),
                    Transactions = orderDto.Transactions.Select(tx => new Transaction
                    {
                        PaymentMethod = tx.PaymentMethod,
                        Status = tx.Status,
                        Amount = tx.Amount,
                        CreatedDate = tx.CreatedDate ?? DateTime.UtcNow
                    }).ToList()
                };

                var createdOrder = await _orderRepository.CreateOrderAsync(order);
                return MapToDto(createdOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateOrderAsync");
                throw;
            }
        }

        public async Task<OrderDto> UpdateOrderAsync(int id, UpdateOrderDto orderDto)
 {
    try
    {
        var existingOrder = await _orderRepository.GetOrderByIdAsync(id);
        if (existingOrder == null)
            throw new NotFoundException($"Order with ID {id} not found for update.");

        var updatedOrder = await _orderRepository.UpdateOrderAsync(existingOrder, orderDto);
        return MapToDto(updatedOrder);
    }
    catch (InvalidOperationException invalidEx)
    {
        _logger.LogWarning(invalidEx, "Invalid status transition");
        throw new ArgumentException(invalidEx.Message);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error in UpdateOrderAsync for ID {id}");
        throw;
    }
    }


      public async Task DeleteOrderAsync(int id)
{
    try
    {
        var existingOrder = await _orderRepository.GetOrderByIdAsync(id);
        if (existingOrder == null)
            throw new NotFoundException($"Order with ID {id} not found for delete.");

        // ✅ Kiểm tra trạng thái đơn hàng trước khi xoá
        var deletableStatuses = new List<string> { "Pending", "Confirmed", "Cancelled" };
        if (!deletableStatuses.Contains(existingOrder.Status))
            throw new InvalidOperationException($"Cannot delete order with status '{existingOrder.Status}'. Only orders with 'Pending', 'Confirmed', or 'Cancelled' status can be deleted.");

        await _orderRepository.DeleteOrderAsync(id);
    }
    catch (NotFoundException nfex)
    {
        _logger.LogWarning(nfex, $"Order with ID {id} not found for delete.");
        throw;
    }
    catch (InvalidOperationException ioex)
    {
        _logger.LogWarning(ioex, $"Invalid delete operation for Order ID {id}");
        throw;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error in DeleteOrderAsync for ID {id}");
        throw;
    }
}


        public async Task<OrderDto> GetOrderByUser(int OrderId, int CustomerId)
        {
            try
            {
                var order = await _orderRepository.GetOrderByUser(OrderId, CustomerId);
                if (order == null)
                    throw new NotFoundException($"Order with ID {OrderId} not found for this user.");
                return MapToDto(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetOrderByUser for OrderID {OrderId} and CustomerID {CustomerId}");
                throw;
            }
        }

        private OrderDto MapToDto(Order order)
        {
            // Tạo thông tin thanh toán từ transaction gần nhất
            var latestTransaction = order.Transactions?.OrderByDescending(t => t.CreatedDate).FirstOrDefault();
            var paymentInfo = latestTransaction != null ? new PaymentInfoDto
            {
                PaymentMethod = latestTransaction.PaymentMethod,
                Status = latestTransaction.Status,
                Amount = latestTransaction.Amount,
                CreatedDate = latestTransaction.CreatedDate
            } : null;

            // Tạo thông tin khách hàng
            var customerInfo = order.Customer != null ? new CustomerInfoDto
            {
                Username = order.Customer.Username,
                Email = order.Customer.Email,
                PhoneNumber = order.Customer.PhoneNumber,
                Address = order.Customer.Address
            } : null;

            // Tạo thông tin voucher
            var voucherInfo = order.Voucher != null ? new VoucherInfoDto
            {
                Code = order.Voucher.Code,
                Name = order.Voucher.Name,
                Value = order.Voucher.Value,
                IsPercent = order.Voucher.IsPercent
            } : null;

            return new OrderDto
            {
                Id = order.Id,
                Status = order.Status,
                UpdatedAt = order.UpdatedAt,
                TotalPrice = order.TotalPrice,
                DiscountPrice = order.DiscountPrice,
                TotalAmount = order.TotalAmount,
                IsPrepaid = order.IsPrepaid,
                Voucher = voucherInfo,
                CustomerInfo = customerInfo,
                PaymentInfo = paymentInfo,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDetailDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? $"Product #{oi.ProductId}",
                    ProductImage = oi.Product?.Image,
                    ProductPrice = oi.Product?.Price ?? 0,
                    ItemQuantity = oi.ItemQuantity
                }).ToList()
            };
        }
    }
}
