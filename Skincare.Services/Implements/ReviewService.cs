using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;

namespace Skincare.Services.Implements
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(IReviewRepository reviewRepository, ILogger<ReviewService> logger)
        {
            _reviewRepository = reviewRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Review>> GetAllReviewsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all reviews.");
                return await _reviewRepository.GetAllReviewsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all reviews.");
                throw;
            }
        }

        public async Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId)
        {
            try
            {
                _logger.LogInformation($"Fetching reviews for product ID: {productId}");
                return await _reviewRepository.GetByProductIdAsync(productId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching reviews for product ID: {productId}");
                throw;
            }
        }

        public async Task<Review> GetReviewByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching review with ID: {id}");
                return await _reviewRepository.GetReviewByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching review with ID: {id}");
                throw;
            }
        }

        public async Task<Review> AddReviewAsync(Review review)
        {
            try
            {
                _logger.LogInformation($"Adding a new review for product ID: {review.ProductId}");
                return await _reviewRepository.AddReviewAsync(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a new review.");
                throw;
            }
        }

        public async Task DeleteReviewAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting review with ID: {id}");
                await _reviewRepository.DeleteReviewAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting review with ID: {id}");
                throw;
            }
        }
    }
}