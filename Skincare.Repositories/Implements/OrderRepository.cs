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

                // 1) Kiểm tra trạng thái cũ -> trạng thái mới có hợp lệ không
                var validStatusTransitions = new Dictionary<string, List<string>>
{
    // Đơn mới tạo có thể "Confirmed" hoặc "Cancelled"
    { "Pending",    new List<string> { "Confirmed", "Cancelled" } },
    // Đơn đã xác nhận có thể "Processing" hoặc "Cancelled"
    { "Confirmed",  new List<string> { "Processing", "Cancelled" } },
    // Đang xử lý có thể "Shipped" hoặc "Cancelled"
    { "Processing", new List<string> { "Shipped", "Cancelled" } },
    // Đang vận chuyển có thể "Delivered" hoặc "Cancelled" (tùy chính sách)
    { "Shipped",    new List<string> { "Delivered", "Cancelled" } },
    // Đơn đã giao xong thì không chuyển trạng thái nữa
    { "Delivered",  new List<string>() },
    // Đơn đã huỷ thì không chuyển nữa
    { "Cancelled",  new List<string>() }
};


                // Chỉ xử lý nếu client gửi lên trường Status
                if (!string.IsNullOrEmpty(updateOrderDto.Status))
                {
                    var currentStatus = existingOrder.Status;         // Trạng thái hiện tại
                    var newStatus = updateOrderDto.Status;            // Trạng thái yêu cầu

                    // Kiểm tra xem currentStatus có trong dictionary không
                    if (!validStatusTransitions.ContainsKey(currentStatus))
                    {
                        throw new InvalidOperationException($"Current status '{currentStatus}' is invalid.");
                    }

                    // Kiểm tra xem newStatus có được phép chuyển từ currentStatus không
                    if (!validStatusTransitions[currentStatus].Contains(newStatus))
                    {
                        throw new InvalidOperationException($"Cannot change status from '{currentStatus}' to '{newStatus}'.");
                    }

                    // Cập nhật trạng thái
                    existingOrder.Status = newStatus;

                    // 2) Nếu có logic Transaction khi chuyển trạng thái
                    //    Ví dụ: Pending -> Confirmed => tạo transaction "Payment"
                    //           Hoặc Shipped -> Cancelled => tạo transaction "Refund"
                    if (currentStatus == "Pending" && newStatus == "Confirmed")
                    {
                        // Gỉả sử user trả tiền ngay khi Confirmed
                        existingOrder.Transactions.Add(new Transaction
                        {
                            PaymentMethod = "Cash",  // Change from "CreditCard" to "Cash"
                            Status = "Paid",
                            Amount = existingOrder.TotalAmount,
                            CreatedDate = DateTime.UtcNow
                        });
                        _logger.LogInformation("Auto-created a Payment transaction on status change Pending -> Confirmed.");
                    }
                    else if (newStatus == "Cancelled")
                    {
                        // Nếu huỷ đơn, có thể ghi nhận transaction "Refund" (nếu đã thu tiền)
                        // Giả sử logic: chỉ refund nếu đơn ở trạng thái "Confirmed"/"Processing"/"Shipped"
                        if (currentStatus != "Pending")
                        {
                            existingOrder.Transactions.Add(new Transaction
                            {
                                PaymentMethod = "Cash",  // Change from "CreditCard" to "Cash"
                                Status = "Refunded",
                                Amount = existingOrder.TotalAmount,
                                CreatedDate = DateTime.UtcNow
                            });
                            _logger.LogInformation("Auto-created a Refund transaction on order cancellation.");
                        }
                    }
                }

                // 3) Kiểm tra VoucherId hợp lệ (nếu có)
                if (updateOrderDto.VoucherId.HasValue)
                {
                    bool voucherExists = await _context.Vouchers
                        .AnyAsync(v => v.Id == updateOrderDto.VoucherId);
                    if (!voucherExists)
                    {
                        throw new NotFoundException($"Voucher with ID {updateOrderDto.VoucherId} does not exist.");
                    }
                    existingOrder.VoucherId = updateOrderDto.VoucherId;
                }

                // 4) Cập nhật các trường khác
                existingOrder.UpdatedAt = DateTime.UtcNow;
                existingOrder.IsPrepaid = updateOrderDto.IsPrepaid ?? existingOrder.IsPrepaid;

                if (updateOrderDto.TotalAmount.HasValue)
                {
                    if (updateOrderDto.TotalAmount.Value < 0)
                        throw new ArgumentException("TotalAmount cannot be negative.");
                    existingOrder.TotalAmount = updateOrderDto.TotalAmount.Value;
                }

                // 5) Nếu client có gửi kèm transaction mới (ví dụ update thêm payment)
                if (updateOrderDto.Transactions != null && updateOrderDto.Transactions.Any())
                {
                    // Lấy transaction mới nhất client gửi
                    var latestTx = updateOrderDto.Transactions
                                        .OrderByDescending(t => t.CreatedDate)
                                        .First();

                    existingOrder.Transactions.Add(new Transaction
                    {
                        PaymentMethod = latestTx.PaymentMethod,
                        Status = latestTx.Status,
                        Amount = latestTx.Amount,
                        CreatedDate = latestTx.CreatedDate ?? DateTime.UtcNow
                    });

                    _logger.LogInformation($"Added a new transaction from UpdateOrderDto to order ID {existingOrder.Id}");
                }

                // 6) Lưu thay đổi
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
                _logger.LogError(ex, $"Error updating order ID {existingOrder?.Id}");
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
                    .Include(o => o.Customer)
                    .Include(o => o.Voucher)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
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
