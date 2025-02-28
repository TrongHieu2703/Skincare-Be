using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
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
            var orders = await _orderRepository.GetAllOrdersAsync();
            return orders.Select(MapToDto).ToList();
        }

        public async Task<OrderDto> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            return order != null ? MapToDto(order) : null;
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId)
        {
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
            return orders.Select(MapToDto).ToList();
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto orderDto)
        {
            // Chuyển đổi DTO sang Entity
            var order = new Order
            {
                CustomerId = orderDto.CustomerId,
                VoucherId = orderDto.VoucherId,
                TotalPrice = orderDto.TotalPrice,
                DiscountPrice = orderDto.DiscountPrice,
                TotalAmount = orderDto.TotalPrice - (orderDto.DiscountPrice ?? 0),
                IsPrepaid = orderDto.IsPrepaid,
                Status = "Pending",  // Mặc định Pending
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

        public async Task<OrderDto> UpdateOrderAsync(int id, UpdateOrderDto orderDto)
        {
            var existingOrder = await _orderRepository.GetOrderByIdAsync(id);
            if (existingOrder == null) return null;

            var updatedOrder = await _orderRepository.UpdateOrderAsync(existingOrder, orderDto);
            return MapToDto(updatedOrder);
        }

        public async Task DeleteOrderAsync(int id)
        {
            await _orderRepository.DeleteOrderAsync(id);
        }

        private OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                VoucherId = order.VoucherId,
                TotalPrice = order.TotalPrice,
                DiscountPrice = order.DiscountPrice,
                TotalAmount = order.TotalAmount,
                IsPrepaid = order.IsPrepaid,
                Status = order.Status,
                UpdatedAt = order.UpdatedAt,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    ItemQuantity = oi.ItemQuantity
                }).ToList(),
                Transactions = order.Transactions.Select(tx => new TransactionDto
                {
                    TransactionId = tx.TransactionId,
                    OrderId = tx.OrderId,
                    PaymentMethod = tx.PaymentMethod,
                    Amount = tx.Amount,
                    Status = tx.Status,
                    CreatedDate = tx.CreatedDate
                }).ToList()
            };
        }
    }
}
