using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Context;
using Skincare.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ReviewRepository : IReviewRepository
{
    private readonly SWP391Context _context;

    public ReviewRepository(SWP391Context context)
    {
        _context = context;
    }

    // GET reviews by product
    public async Task<IEnumerable<ReviewDto>> GetReviewsByProductIdAsync(int productId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.ProductId == productId)
            .ToListAsync();

        return reviews.Select(r => new ReviewDto
        {
            Id = r.Id,
            OrderDetailId = r.OrderDetailId,
            CustomerId = r.CustomerId,
            ProductId = r.ProductId,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt
        });
    }

    // GET review by id
    public async Task<ReviewDto> GetReviewByIdAsync(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null) return null;

        return new ReviewDto
        {
            Id = review.Id,
            OrderDetailId = review.OrderDetailId,
            CustomerId = review.CustomerId,
            ProductId = review.ProductId,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };
    }

    // CREATE review
    public async Task<ReviewDto> CreateReviewAsync(CreateReviewDto createReviewDto)
    {
        var review = new Review
        {
            OrderDetailId = createReviewDto.OrderDetailId,
            CustomerId = createReviewDto.CustomerId,
            ProductId = createReviewDto.ProductId,
            Rating = createReviewDto.Rating,
            Comment = createReviewDto.Comment,
            CreatedAt = System.DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return new ReviewDto
        {
            Id = review.Id,
            OrderDetailId = review.OrderDetailId,
            CustomerId = review.CustomerId,
            ProductId = review.ProductId,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };
    }

    // UPDATE review
    public async Task<ReviewDto> UpdateReviewAsync(int id, UpdateReviewDto updateReviewDto)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null) return null;

        review.Rating = updateReviewDto.Rating ?? review.Rating;
        review.Comment = updateReviewDto.Comment ?? review.Comment;

        await _context.SaveChangesAsync();

        return new ReviewDto
        {
            Id = review.Id,
            OrderDetailId = review.OrderDetailId,
            CustomerId = review.CustomerId,
            ProductId = review.ProductId,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };
    }

    // DELETE review
    public async Task DeleteReviewAsync(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review != null)
        {
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
        }
    }
}
