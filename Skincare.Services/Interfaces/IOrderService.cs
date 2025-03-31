using Skincare.BusinessObjects.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto> GetOrderByIdAsync(int id);
        Task<OrderDto> GetOrderByUser(int OrderId, int CustomerId);
        Task<OrderDto> CreateOrderAsync(CreateOrderDto orderDto);
        Task<OrderDto> UpdateOrderAsync(int id, UpdateOrderDto orderDto);
        Task DeleteOrderAsync(int id);
        Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId);
        Task<OrderDetailDto> GetOrderDetailAsync(int id);
    }
}
