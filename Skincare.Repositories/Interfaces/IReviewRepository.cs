using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Repositories.Interfaces
{
    public interface IReviewRepository
    {
        Task<IEnumerable<ReviewDto>> GetReviewsByProductIdAsync(int productId);
        Task<ReviewDto> GetReviewByIdAsync(int id);
        Task<ReviewDto> CreateReviewAsync(CreateReviewDto createReviewDto);
        Task<ReviewDto> UpdateReviewAsync(int id, UpdateReviewDto updateReviewDto);
        Task DeleteReviewAsync(int id);
        
        // New methods to replace direct context access
        Task<bool> OrderItemExistsAsync(int orderItemId);
        Task<OrderItem> GetOrderItemWithOrderAsync(int orderItemId);
        Task<bool> ReviewExistsAsync(int orderItemId, int customerId, int productId);
        Task<bool> ProductExistsAsync(int productId);
        Task<Account> GetCustomerAsync(int customerId);
        
        // Add the new method to get review by order item and customer
        Task<ReviewDto> GetReviewByOrderItemAsync(int orderItemId, int customerId);
    }
}
