using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Exceptions; // import NotFoundException, ...
using Skincare.Repositories.Interfaces;
using Skincare.Repositories.Context;
using Skincare.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skincare.Services.Implements
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly SWP391Context _context;

        public ReviewService(IReviewRepository reviewRepository, SWP391Context context)
        {
            _reviewRepository = reviewRepository;
            _context = context;
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByProductIdAsync(int productId)
        {
            // Trả về danh sách rỗng nếu không có review, không cần quăng exception.
            return await _reviewRepository.GetReviewsByProductIdAsync(productId);
        }

        public async Task<ReviewDto> GetReviewByIdAsync(int id)
        {
            var review = await _reviewRepository.GetReviewByIdAsync(id);
            if (review == null)
            {
                throw new NotFoundException($"Review with ID {id} not found.");
            }
            return review;
        }

        public async Task<ReviewDto> CreateReviewAsync(CreateReviewDto createReviewDto)
        {
            // Kiểm tra tồn tại của OrderDetail
            bool orderItemExists = await _context.OrderItems.AnyAsync(o => o.Id == createReviewDto.OrderDetailId);
            if (!orderItemExists)
            {
                throw new NotFoundException($"OrderDetail with ID {createReviewDto.OrderDetailId} does not exist.");
            }
            // Kiểm tra tồn tại của Product
            bool productExists = await _context.Products.AnyAsync(p => p.Id == createReviewDto.ProductId);
            if (!productExists)
            {
                throw new NotFoundException($"Product with ID {createReviewDto.ProductId} does not exist.");
            }

            return await _reviewRepository.CreateReviewAsync(createReviewDto);
        }

        public async Task<ReviewDto> UpdateReviewAsync(int id, UpdateReviewDto updateReviewDto)
        {
            var updatedReview = await _reviewRepository.UpdateReviewAsync(id, updateReviewDto);
            if (updatedReview == null)
            {
                throw new NotFoundException($"Review with ID {id} not found for update.");
            }
            return updatedReview;
        }

        public async Task DeleteReviewAsync(int id)
        {
            var review = await _reviewRepository.GetReviewByIdAsync(id);
            if (review == null)
            {
                throw new NotFoundException($"Review with ID {id} not found for deletion.");
            }
            await _reviewRepository.DeleteReviewAsync(id);
        }
    }
}
