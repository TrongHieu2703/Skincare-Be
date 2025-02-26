using Skincare.BusinessObjects.DTOs;
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;

    public ReviewService(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<IEnumerable<ReviewDto>> GetReviewsByProductIdAsync(int productId)
    {
        return await _reviewRepository.GetReviewsByProductIdAsync(productId);
    }

    public async Task<ReviewDto> GetReviewByIdAsync(int id)
    {
        return await _reviewRepository.GetReviewByIdAsync(id);
    }

    public async Task<ReviewDto> CreateReviewAsync(CreateReviewDto createReviewDto)
    {
        return await _reviewRepository.CreateReviewAsync(createReviewDto);
    }

    public async Task<ReviewDto> UpdateReviewAsync(int id, UpdateReviewDto updateReviewDto)
    {
        return await _reviewRepository.UpdateReviewAsync(id, updateReviewDto);
    }

    public async Task DeleteReviewAsync(int id)
    {
        await _reviewRepository.DeleteReviewAsync(id);
    }
}
