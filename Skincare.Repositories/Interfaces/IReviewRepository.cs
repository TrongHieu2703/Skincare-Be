using Skincare.BusinessObjects.DTOs;
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
    }
}
