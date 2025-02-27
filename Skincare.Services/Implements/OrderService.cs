using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
using Microsoft.Extensions.Logging;
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

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
        {
            var order = new Order
            {
                CustomerId = createOrderDto.CustomerId,
                VoucherId = createOrderDto.VoucherId,
                IsPrepaid = createOrderDto.IsPrepaid,
                Status = "Pending",
                OrderItems = createOrderDto.OrderItems.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    ItemQuantity = item.ItemQuantity
                }).ToList()
            };

            var createdOrder = await _orderRepository.CreateOrderAsync(order);
            return MapToDto(createdOrder);
        }

        public async Task<OrderDto> UpdateOrderAsync(int id, UpdateOrderDto updateOrderDto)
        {
            var existingOrder = await _orderRepository.GetOrderByIdAsync(id);
            if (existingOrder == null) return null;

            var updatedOrder = await _orderRepository.UpdateOrderAsync(existingOrder, updateOrderDto);
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
                }).ToList()
            };
        }
    }
}
