using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Context;
using Skincare.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skincare.Repositories.Implements
{
    public class OrderRepository : IOrderRepository
    {
        private readonly SWP391Context _context;
        private readonly Microsoft.Extensions.Logging.ILogger<OrderRepository> _logger;

        public OrderRepository(SWP391Context context, Microsoft.Extensions.Logging.ILogger<OrderRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.Transactions)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all orders");
                throw;
            }
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Voucher)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Include(o => o.Transactions)
                    .FirstOrDefaultAsync(o => o.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching order with ID {id}");
                throw;
            }
        }

       public async Task<Order> CreateOrderAsync(Order order)
{
    try
    {
        // Kiểm tra voucher tồn tại nếu có
        if (order.VoucherId.HasValue)
        {
            var voucherExists = await _context.Vouchers.AnyAsync(v => v.Id == order.VoucherId.Value);
            if (!voucherExists)
            {
                _logger.LogWarning($"Voucher with ID {order.VoucherId.Value} does not exist. Skipping voucher.");
                order.VoucherId = null;
            }
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating order");
        throw;
    }
}


        public async Task<Order> UpdateOrderAsync(Order existingOrder, UpdateOrderDto updateOrderDto)
        {
            try
            {
                if (existingOrder == null)
                    throw new Exception("Order not found");

                // Cập nhật thông tin đơn hàng
                existingOrder.VoucherId = updateOrderDto.VoucherId ?? existingOrder.VoucherId;
                existingOrder.Status = updateOrderDto.Status ?? existingOrder.Status;
                existingOrder.TotalPrice = updateOrderDto.TotalPrice ?? existingOrder.TotalPrice;
                existingOrder.DiscountPrice = updateOrderDto.DiscountPrice ?? existingOrder.DiscountPrice;
                existingOrder.TotalAmount = updateOrderDto.TotalAmount ?? existingOrder.TotalPrice - (updateOrderDto.DiscountPrice ?? 0);
                existingOrder.IsPrepaid = updateOrderDto.IsPrepaid ?? existingOrder.IsPrepaid;
                existingOrder.UpdatedAt = DateTime.UtcNow;

                // Xóa các OrderItems & Transactions cũ (nếu cập nhật toàn bộ)
                _context.OrderItems.RemoveRange(existingOrder.OrderItems);
                _context.Transactions.RemoveRange(existingOrder.Transactions);
                await _context.SaveChangesAsync();

                // Thêm OrderItems mới
                existingOrder.OrderItems = updateOrderDto.OrderItems.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    ItemQuantity = item.ItemQuantity
                }).ToList();

                // Thêm Transactions mới
                existingOrder.Transactions = updateOrderDto.Transactions.Select(tx => new Transaction
                {
                    PaymentMethod = tx.PaymentMethod,
                    Status = tx.Status,
                    Amount = tx.Amount,
                    CreatedDate = tx.CreatedDate ?? DateTime.UtcNow
                }).ToList();

                _context.Orders.Update(existingOrder);
                await _context.SaveChangesAsync();

                return existingOrder;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order");
                throw;
            }
        }

        public async Task DeleteOrderAsync(int id)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order != null)
                {
                    _context.Orders.Remove(order);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting order with ID {id}");
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId)
{
    try
    {
        return await _context.Orders
            .Where(o => o.CustomerId == userId)
            .Include(o => o.OrderItems)
            .Include(o => o.Transactions)
            .ToListAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error fetching orders for user {userId}");
        throw;
    }
}

        public async Task<Order> GetOrderByUser(int OrderId, int CustomerId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Voucher)
                    .Include(o => o.Transactions)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == OrderId && o.CustomerId == CustomerId);
                
                if (order == null)
                {
                    _logger.LogWarning($"Order with ID {OrderId} and CustomerId {CustomerId} not found");
                    throw new Exception("Order not found");
                }
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching order with ID {OrderId} and CustomerId {CustomerId}");
                throw;
            }
        }
    }
}
