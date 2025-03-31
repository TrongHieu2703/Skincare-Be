using Skincare.BusinessObjects.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Services.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDto>> GetReviewsByProductIdAsync(int productId);
        Task<ReviewDto> GetReviewByIdAsync(int id);
        Task<ReviewDto> CreateReviewAsync(CreateReviewDto createReviewDto);
        Task<ReviewDto> UpdateReviewAsync(int id, UpdateReviewDto updateReviewDto);
        Task DeleteReviewAsync(int id);
        
        // New methods for validation
        Task<bool> ValidateOrderItemExistsAsync(int orderItemId);
        Task<bool> ValidateOrderStatusAsync(int orderItemId);
        Task<bool> CheckExistingReviewAsync(int orderItemId, int customerId, int productId);
        
        // New method to calculate average rating
        Task<double> GetAverageRatingForProductAsync(int productId);
        
        // Add the new method to get review by order item
        Task<ReviewDto> GetReviewByOrderItemAsync(int orderItemId, int customerId);
    }
}
