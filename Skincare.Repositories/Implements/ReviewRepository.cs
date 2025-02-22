using Microsoft.EntityFrameworkCore;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Context;
using Skincare.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;

namespace Skincare.Repositories.Implements
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly SWP391Context _context;
        private readonly ILogger<ReviewRepository> _logger;

        public ReviewRepository(SWP391Context context, ILogger<ReviewRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Review>> GetAllReviewsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all reviews.");
                return await _context.Reviews.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all reviews.");
                throw;
            }
        }

        public async Task<IEnumerable<Review>> GetByProductIdAsync(int productId)
        {
            try
            {
                _logger.LogInformation($"Fetching reviews for product ID: {productId}");
                return await _context.Reviews.Where(r => r.ProductId == productId).ToListAsync();
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
                return await _context.Reviews.FindAsync(id);
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
                _logger.LogInformation("Adding a new review.");
                await _context.Reviews.AddAsync(review);
                await _context.SaveChangesAsync();
                return review;
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
                var review = await _context.Reviews.FindAsync(id);
                if (review != null)
                {
                    _context.Reviews.Remove(review);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting review with ID: {id}");
                throw;
            }
        }
    }
}