using Microsoft.EntityFrameworkCore;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Context;
using Skincare.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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
                return await _context.Orders.Include(o => o.OrderItems).ToListAsync();
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
                return await _context.Orders.Include(o => o.OrderItems)
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
                // Kiểm tra VoucherId nếu có
                if (order.VoucherId.HasValue)
                {
                    var voucherExists = await _context.Vouchers
                                                      .AnyAsync(v => v.Id == order.VoucherId);

                    if (!voucherExists)
                    {
                        // Nếu voucher không tồn tại => bỏ qua VoucherId
                        order.VoucherId = null;
                        _logger.LogWarning($"Voucher with ID {order.VoucherId} does not exist. Skipping voucher.");
                    }
                }

                // Thêm đơn hàng
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


        public async Task UpdateOrderAsync(Order updatedOrder)
        {
            try
            {
                var existingOrder = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.Transactions)
                    .AsSplitQuery() // 👉 Giảm tải truy vấn phức tạp
                    .FirstOrDefaultAsync(o => o.Id == updatedOrder.Id);

                if (existingOrder == null)
                    throw new Exception($"Order with ID {updatedOrder.Id} not found");

                // ✅ Cập nhật thông tin đơn hàng
                existingOrder.VoucherId = updatedOrder.VoucherId;
                existingOrder.Status = updatedOrder.Status;
                existingOrder.TotalPrice = updatedOrder.TotalPrice;
                existingOrder.DiscountPrice = updatedOrder.DiscountPrice;
                existingOrder.TotalAmount = updatedOrder.TotalAmount;
                existingOrder.IsPrepaid = updatedOrder.IsPrepaid;
                existingOrder.UpdatedAt = DateTime.UtcNow;

                // ✅ Xóa OrderItems cũ (chỉ những item có ID > 0)
                var existingOrderItems = existingOrder.OrderItems.Where(oi => oi.Id > 0).ToList();
                _context.OrderItems.RemoveRange(existingOrderItems);

                // ✅ Xóa Transactions cũ
                var existingTransactions = existingOrder.Transactions.Where(t => t.TransactionId > 0).ToList();
                _context.Transactions.RemoveRange(existingTransactions);

                // 👉 Save sau khi xóa để tránh lỗi
                await _context.SaveChangesAsync();

                // ✅ Thêm OrderItems mới
                foreach (var item in updatedOrder.OrderItems)
                {
                    existingOrder.OrderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        ItemQuantity = item.ItemQuantity
                    });
                }

                // ✅ Thêm Transactions mới
                foreach (var transaction in updatedOrder.Transactions)
                {
                    existingOrder.Transactions.Add(new Transaction
                    {
                        PaymentMethod = transaction.PaymentMethod,
                        Status = transaction.Status,
                        Amount = transaction.Amount,
                        CreatedDate = transaction.CreatedDate ?? DateTime.UtcNow
                    });
                }

                // 👉 Save lần 2 sau khi thêm mới
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating order with ID {updatedOrder.Id}");
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