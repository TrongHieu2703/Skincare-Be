using Skincare.BusinessObjects.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Services.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<Review>> GetAllReviewsAsync();
        Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId);
        Task<Review> GetReviewByIdAsync(int id);
        Task<Review> AddReviewAsync(Review review);
        Task DeleteReviewAsync(int id);
    }
}
