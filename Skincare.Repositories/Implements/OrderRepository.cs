using Microsoft.EntityFrameworkCore;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Context;
using Skincare.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using System;
using System.Linq;

namespace Skincare.Repositories.Implements
{
    public class OrderRepository : IOrderRepository
    {
        private readonly SWP391Context _context;
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(SWP391Context context, ILogger<OrderRepository> logger)
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
                    .Include(o => o.OrderItems)
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
                if (order.VoucherId.HasValue)
                {
                    var voucherExists = await _context.Vouchers.AnyAsync(v => v.Id == order.VoucherId);
                    if (!voucherExists)
                    {
                        order.VoucherId = null;
                        _logger.LogWarning($"Voucher with ID {order.VoucherId} does not exist. Skipping voucher.");
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

        public async Task<Order> UpdateOrderAsync(Order existingOrder, UpdateOrderDto updatedOrder)
        {
            try
            {
                if (existingOrder == null)
                    throw new Exception($"Order not found");

                existingOrder.VoucherId = updatedOrder.VoucherId ?? existingOrder.VoucherId;
                existingOrder.Status = updatedOrder.Status ?? existingOrder.Status;
                existingOrder.TotalPrice = updatedOrder.TotalPrice ?? existingOrder.TotalPrice;
                existingOrder.DiscountPrice = updatedOrder.DiscountPrice ?? existingOrder.DiscountPrice;
                existingOrder.TotalAmount = updatedOrder.TotalAmount ?? existingOrder.TotalAmount;
                existingOrder.IsPrepaid = updatedOrder.IsPrepaid ?? existingOrder.IsPrepaid;
                existingOrder.UpdatedAt = DateTime.UtcNow;

                // Xóa OrderItems & Transactions cũ
                _context.OrderItems.RemoveRange(existingOrder.OrderItems);
                _context.Transactions.RemoveRange(existingOrder.Transactions);
                await _context.SaveChangesAsync();

                // Thêm OrderItems mới
                existingOrder.OrderItems = updatedOrder.OrderItems.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    ItemQuantity = item.ItemQuantity
                }).ToList();

                // Thêm Transactions mới
                existingOrder.Transactions = updatedOrder.Transactions.Select(tx => new Transaction
                {
                    PaymentMethod = tx.PaymentMethod,
                    Status = tx.Status,
                    Amount = tx.Amount,
                    CreatedDate = tx.CreatedDate ?? DateTime.UtcNow
                }).ToList();

                // Save lại đơn hàng sau khi cập nhật
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
    }
}
