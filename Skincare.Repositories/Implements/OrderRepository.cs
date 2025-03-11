using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.BusinessObjects.Exceptions;
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
            throw new NotFoundException("Order not found.");

        _logger.LogInformation($"Updating order ID {existingOrder.Id}");

        // Kiểm tra trạng thái hợp lệ trước khi cập nhật
        var validStatusTransitions = new Dictionary<string, List<string>>
        {
            { "Pending", new List<string> { "Confirmed", "Cancelled" } },
            { "Confirmed", new List<string> { "Processing", "Cancelled" } },
            { "Processing", new List<string> { "Shipped", "Cancelled" } },
            { "Shipped", new List<string> { "Delivered" } },
            { "Delivered", new List<string>() } // Không thể cập nhật đơn hàng đã giao
        };

        if (!string.IsNullOrEmpty(updateOrderDto.Status))
        {
            if (!validStatusTransitions.ContainsKey(existingOrder.Status))
            {
                throw new InvalidOperationException($"Current status '{existingOrder.Status}' is invalid.");
            }

            if (!validStatusTransitions[existingOrder.Status].Contains(updateOrderDto.Status))
            {
                throw new InvalidOperationException($"Cannot change status from '{existingOrder.Status}' to '{updateOrderDto.Status}'.");
            }

            existingOrder.Status = updateOrderDto.Status;
        }

        // Kiểm tra VoucherId hợp lệ (nếu có)
        if (updateOrderDto.VoucherId.HasValue)
        {
            bool voucherExists = await _context.Vouchers.AnyAsync(v => v.Id == updateOrderDto.VoucherId);
            if (!voucherExists)
            {
                throw new NotFoundException($"Voucher with ID {updateOrderDto.VoucherId} does not exist.");
            }
            existingOrder.VoucherId = updateOrderDto.VoucherId;
        }

        // Cập nhật thông tin cơ bản
        existingOrder.UpdatedAt = DateTime.UtcNow;
        existingOrder.IsPrepaid = updateOrderDto.IsPrepaid ?? existingOrder.IsPrepaid;

        // Kiểm tra tổng tiền hợp lệ
        if (updateOrderDto.TotalAmount.HasValue)
        {
            if (updateOrderDto.TotalAmount.Value < 0)
                throw new ArgumentException("TotalAmount cannot be negative.");

            existingOrder.TotalAmount = updateOrderDto.TotalAmount.Value;
        }

        // Cập nhật paymentInfo nếu có
        if (updateOrderDto.Transactions != null && updateOrderDto.Transactions.Any())
        {
            var latestTransaction = updateOrderDto.Transactions.OrderByDescending(t => t.CreatedDate).First();

            existingOrder.Transactions.Add(new Transaction
            {
                PaymentMethod = latestTransaction.PaymentMethod,
                Status = latestTransaction.Status,
                Amount = latestTransaction.Amount,
                CreatedDate = latestTransaction.CreatedDate ?? DateTime.UtcNow
            });

            _logger.LogInformation($"Added transaction to order ID {existingOrder.Id}");
        }

        _context.Orders.Update(existingOrder);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Order ID {existingOrder.Id} updated successfully.");

        return existingOrder;
    }
    catch (NotFoundException nfex)
    {
        _logger.LogWarning(nfex, $"Order update failed: {nfex.Message}");
        throw;
    }
    catch (InvalidOperationException ioex)
    {
        _logger.LogWarning(ioex, $"Order update failed: {ioex.Message}");
        throw;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error updating order ID {existingOrder.Id}");
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
