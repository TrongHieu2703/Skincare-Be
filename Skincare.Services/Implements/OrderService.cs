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
                    ProductName = oi.Product?.Name ?? $"Sản phẩm #{oi.ProductId}",
                    ProductImage = oi.Product?.Image,
                    ProductPrice = oi.Product?.Price ?? 0,
                    ItemQuantity = oi.ItemQuantity
                }).ToList()
            };
        }

        public async Task<OrderDto> GetOrderByUser(int OrderId, int CustomerId)
        {
            var order = await _orderRepository.GetOrderByUser(OrderId, CustomerId);
            return order != null ? MapToDto(order) : null;
        }
    }
}
