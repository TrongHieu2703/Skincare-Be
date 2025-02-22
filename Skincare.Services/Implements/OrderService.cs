using Skincare.BusinessObjects.Entities;
using Skincare.Services.Interfaces;
using Skincare.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;

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

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all orders.");
                return await _orderRepository.GetAllOrdersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all orders.");
                throw;
            }
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching order with ID: {id}");
                return await _orderRepository.GetOrderByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching order with ID: {id}");
                throw;
            }
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            try
            {
                _logger.LogInformation("Creating a new order.");
                return await _orderRepository.CreateOrderAsync(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new order.");
                throw;
            }
        }

        public async Task UpdateOrderAsync(Order order)
        {
            try
            {
                _logger.LogInformation($"Updating order with ID: {order.Id}");
                await _orderRepository.UpdateOrderAsync(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating order with ID: {order.Id}");
                throw;
            }
        }

        public async Task DeleteOrderAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting order with ID: {id}");
                await _orderRepository.DeleteOrderAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting order with ID: {id}");
                throw;
            }
        }
    }
}