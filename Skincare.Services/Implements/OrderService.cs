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
        private readonly IProductRepository _productRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IVoucherRepository _voucherRepository;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IInventoryRepository inventoryRepository,
            IVoucherRepository voucherRepository,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _inventoryRepository = inventoryRepository ?? throw new ArgumentNullException(nameof(inventoryRepository));
            _voucherRepository = voucherRepository ?? throw new ArgumentNullException(nameof(voucherRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

                // Validate all products before processing
                foreach (var item in orderDto.OrderItems)
                {
                    var product = await _productRepository.GetProductByIdAsync(item.ProductId);
                    if (product == null)
                        throw new NotFoundException($"Sản phẩm với ID {item.ProductId} không tồn tại.");

                    if (!product.IsAvailable)
                        throw new InvalidOperationException($"Sản phẩm '{product.Name}' hiện không khả dụng.");

                    // Check product's stock
                    var availableStock = product.Stock ?? 0;
                    if (availableStock <= 0)
                        throw new InvalidOperationException($"Sản phẩm '{product.Name}' đã hết hàng.");

                    if (availableStock < item.ItemQuantity)
                        throw new InvalidOperationException($"Sản phẩm '{product.Name}' chỉ còn {availableStock} sản phẩm.");
                }

                // Nếu VoucherId không hợp lệ, đặt về null
                if (orderDto.VoucherId.HasValue && orderDto.VoucherId.Value <= 0)
                    orderDto.VoucherId = null;

                // Add additional voucher validation before order creation
                if (orderDto.VoucherId.HasValue)
                {
                    var voucher = await _voucherRepository.GetVoucherByIdAsync(orderDto.VoucherId.Value);
                    
                    // Validate voucher exists
                    if (voucher == null)
                        throw new NotFoundException($"Voucher with ID {orderDto.VoucherId.Value} not found.");
                    
                    // Validate voucher is not expired
                    if (voucher.ExpiredAt < DateTime.UtcNow)
                        throw new InvalidOperationException("This voucher has expired.");
                    
                    // Validate voucher is active (has quantity or is infinity)
                    if (!voucher.IsInfinity && voucher.Quantity <= 0)
                        throw new InvalidOperationException("This voucher has been fully redeemed.");
                    
                    // Additional validation like minimum order value
                    if (orderDto.TotalPrice < voucher.MinOrderValue)
                        throw new InvalidOperationException($"Order total must be at least {voucher.MinOrderValue} to use this voucher.");
                }

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

                // Create order first
                var createdOrder = await _orderRepository.CreateOrderAsync(order);

                // If order is created successfully, update product stock
                foreach (var item in createdOrder.OrderItems)
                {
                    // Update the product stock
                    var product = await _productRepository.GetProductByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        // Calculate new stock value
                        int currentStock = product.Stock ?? 0;
                        product.Stock = Math.Max(0, currentStock - item.ItemQuantity);
                        
                        // Update product
                        await _productRepository.UpdateProductAsync(product);
                        _logger.LogInformation($"Updated product stock for product {item.ProductId}, " +
                            $"deducted {item.ItemQuantity} units, new stock: {product.Stock}");
                    }
                }

                // If a voucher was applied, decrement its quantity if it's not set to infinity
                if (createdOrder.VoucherId.HasValue)
                {
                    try
                    {
                        // Fetch the voucher to get its current state
                        var voucher = await _voucherRepository.GetVoucherByIdAsync(createdOrder.VoucherId.Value);
                        
                        // Only decrement quantity if the voucher is not set to infinity and has remaining quantity
                        if (voucher != null && !(voucher.IsInfinity) && voucher.Quantity > 0)
                        {
                            // Create update DTO with decremented quantity
                            var updateVoucherDto = new UpdateVoucherDto
                            {
                                Quantity = voucher.Quantity - 1
                            };
                            
                            // Update the voucher in the database
                            await _voucherRepository.UpdateVoucherAsync(createdOrder.VoucherId.Value, updateVoucherDto);
                            
                            _logger.LogInformation($"Decremented quantity for voucher {voucher.VoucherId} after applying to order {createdOrder.Id}, " +
                                $"new quantity: {voucher.Quantity - 1}");
                        }
                    }
                    catch (Exception voucherEx)
                    {
                        // Log the error but don't fail the order creation
                        _logger.LogWarning(voucherEx, $"Failed to update voucher quantity for ID {createdOrder.VoucherId.Value}");
                    }
                }

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

        public async Task<OrderDetailDto> GetOrderDetailAsync(int id)
        {
            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(id);
                if (order == null)
                    throw new NotFoundException($"Order with ID {id} not found.");
                
                return MapToDetailDto(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetOrderDetailAsync for ID {id}");
                throw;
            }
        }

        private OrderDto MapToDto(Order order)
        {
            // Check if order.Customer exists before trying to map
            var customerInfo = order.Customer != null ? new CustomerInfoDto
            {
                Username = order.Customer.Username ?? "—",
                Email = order.Customer.Email ?? "—",
                PhoneNumber = order.Customer.PhoneNumber ?? "—",
                Address = order.Customer.Address ?? "—"
            } : new CustomerInfoDto
            {
                Username = "—",
                Email = "—",
                PhoneNumber = "—",
                Address = "—"
            };

            // Tạo thông tin thanh toán từ transaction gần nhất
            var latestTransaction = order.Transactions?.OrderByDescending(t => t.CreatedDate).FirstOrDefault();
            var paymentInfo = latestTransaction != null ? new PaymentInfoDto
            {
                PaymentMethod = latestTransaction.PaymentMethod,
                Status = latestTransaction.Status,
                Amount = latestTransaction.Amount,
                CreatedDate = latestTransaction.CreatedDate
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
                OrderItems = order.OrderItems.Select(oi => {
                    // Log product details for debugging
                    _logger.LogInformation($"Mapping OrderItem with ProductId {oi.ProductId}: " +
                        $"Product is {(oi.Product == null ? "NULL" : "loaded")}, " +
                        $"Product Name: {oi.Product?.Name ?? "N/A"}, " +
                        $"Image Path: {oi.Product?.Image ?? "N/A"}");

                    return new OrderItemDetailDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product?.Name ?? $"Product #{oi.ProductId}",
                        ProductImage = oi.Product?.Image,
                        ProductPrice = oi.Product?.Price ?? 0,
                        ItemQuantity = oi.ItemQuantity
                    };
                }).ToList()
            };
        }

        private OrderDetailDto MapToDetailDto(Order order)
        {
            // Get latest transaction for payment info
            var latestTransaction = order.Transactions?
                .OrderByDescending(t => t.CreatedDate)
                .FirstOrDefault();
            
            return new OrderDetailDto
            {
                Id = order.Id,
                Status = order.Status,
                UpdatedAt = order.UpdatedAt,
                TotalPrice = order.TotalPrice,
                DiscountPrice = order.DiscountPrice,
                TotalAmount = order.TotalAmount,
                IsPrepaid = order.IsPrepaid,
                
                // Ensure customer is properly mapped with fallback values
                Customer = order.Customer != null 
                    ? new CustomerDetailDto
                    {
                        Id = order.Customer.Id,
                        Username = order.Customer.Username ?? "—",
                        Email = order.Customer.Email ?? "—",
                        PhoneNumber = order.Customer.PhoneNumber ?? "—",
                        Address = order.Customer.Address ?? "—"
                    } 
                    : new CustomerDetailDto
                    {
                        Id = 0,
                        Username = "—",
                        Email = "—",
                        PhoneNumber = "—",
                        Address = "—"
                    },
                
                // Map voucher information with null checking
                Voucher = order.Voucher != null 
                    ? new VoucherDetailDto
                    {
                        Id = order.Voucher.Id,
                        Code = order.Voucher.Code,
                        Name = order.Voucher.Name,
                        Value = order.Voucher.Value,
                        IsPercent = order.Voucher.IsPercent,
                        MinOrderValue = order.Voucher.MinOrderValue,
                        MaxDiscountValue = order.Voucher.MaxDiscountValue
                    } 
                    : null,
                
                // Map payment information
                Payment = latestTransaction != null 
                    ? new PaymentDetailDto
                    {
                        PaymentMethod = latestTransaction.PaymentMethod,
                        Status = latestTransaction.Status,
                        Amount = latestTransaction.Amount,
                        CreatedDate = latestTransaction.CreatedDate
                    } 
                    : null,
                
                // Map order items with product details
                Items = order.OrderItems
                    .Select(oi => new OrderItemDetailDto
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product?.Name ?? $"Product #{oi.ProductId}",
                        ProductImage = oi.Product?.Image ?? string.Empty,
                        ProductPrice = oi.Product?.Price ?? 0,
                        ItemQuantity = oi.ItemQuantity
                    })
                    .ToList()
            };
        }
    }
}
