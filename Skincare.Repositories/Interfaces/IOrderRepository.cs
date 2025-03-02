using Skincare.BusinessObjects.Entities;
using Skincare.BusinessObjects.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order> GetOrderByIdAsync(int id);
        Task<Order> CreateOrderAsync(Order order);
        Task<Order> GetOrderByUser(int OrderId, int CustomerId);
        Task<Order> UpdateOrderAsync(Order existingOrder, UpdateOrderDto updateOrderDto);
        Task DeleteOrderAsync(int id);

        // Mới: Lấy đơn hàng của user
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId);
    }
}
